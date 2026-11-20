"use client";
import { createContext, useContext, useEffect, useReducer, ReactNode, Dispatch } from "react";
import type { Dish, Ingredient, Opt } from "./menu";
import type { Order, OrderType } from "./api";

export type Line = { key: string; dish: Dish; qty: number; opts: Opt[]; removed: Ingredient[] };
// ready = the saved session has been loaded from this browser (not saved itself)
type S = { mode: OrderType | null; table: number; tableToken: string | null; allergies: string[]; cart: Line[]; last: Order | null; ready: boolean };
type A =
  | { t: "mode"; m: OrderType | null } | { t: "table"; n: number; token: string } | { t: "clearTable" }
  | { t: "allergies"; a: string[] } | { t: "add"; l: Line } | { t: "remove"; key: string }
  | { t: "done"; order: Order } | { t: "reset" } | { t: "hydrate"; s: Partial<S> | null };

const empty: S = { mode: null, table: 0, tableToken: null, allergies: [], cart: [], last: null, ready: false };

function reduce(s: S, a: A): S {
  switch (a.t) {
    case "mode": return { ...s, mode: a.m };
    case "table": return { ...s, table: a.n, tableToken: a.token };
    case "clearTable": return { ...s, table: 0, tableToken: null };
    case "allergies": return { ...s, allergies: a.a };
    case "add": return { ...s, cart: [...s.cart, a.l] };
    case "remove": return { ...s, cart: s.cart.filter((l) => l.key !== a.key) };
    case "done": return { ...s, cart: [], last: a.order };
    // New customer on this device: forget everything about the last one
    case "reset": return { ...empty, ready: true };
    case "hydrate": return { ...empty, ...a.s, ready: true };
    default: return s;
  }
}

export const unit = (l: Line) => l.dish.price + l.opts.reduce((t, o) => t + o.delta, 0);
export const total = (c: Line[]) => c.reduce((t, l) => t + unit(l) * l.qty, 0);
// Allergens the customer declared that are still in the dish after leaving out the removed ingredients
export const conflicts = (d: Dish, allergies: string[], removed: Ingredient[] = []) => {
  const gone = new Set(removed.map((r) => r.id));
  return [...new Set(d.ingredients.filter((i) => !gone.has(i.id) && i.allergen && allergies.includes(i.allergen)).map((i) => i.allergen as string))];
};
// The ingredients to remove to make the dish safe, or null if any of them is required
export function fix(d: Dish, allergies: string[]): Ingredient[] | null {
  const bad = d.ingredients.filter((i) => i.allergen && allergies.includes(i.allergen));
  return bad.length > 0 && bad.every((i) => i.removable) ? bad : null;
}
export const peso = (n: number) => `₱${n}`;
// Why a cart line can't be sent any more (its dish or an ingredient sold out after it was added), or null
export function lineIssue(l: Line, dishes: Dish[] | null): string | null {
  const now = dishes?.find((x) => x.id === l.dish.id);
  if (!now) return null;                       // menu not loaded yet: the API checks again on confirm
  if (!now.available) return "Sold out. Please remove it.";
  const gone = new Set(l.removed.map((r) => r.id));
  const out = now.ingredients.filter((i) => !i.available && !gone.has(i.id));
  return out.length ? `${out.map((i) => i.name).join(", ")} sold out. Please remove and add it again.` : null;
}
// "Table 4" or "Take-out"
export const where = (s: { mode: OrderType | null; table: number }) =>
  s.mode === "TakeOut" ? "Take-out" : s.table ? `Table ${s.table}` : "No table";
// The customer can order once they chose take-out, or dine-in with a table
export const canOrder = (s: S) => s.mode === "TakeOut" || (s.mode === "DineIn" && s.tableToken !== null);

// The session is saved in this browser, so a refresh keeps the table, allergies, and cart
const SAVE = "customo-session";
const Ctx = createContext<{ s: S; d: Dispatch<A>; ready: boolean }>(null!);
export function Provider({ children }: { children: ReactNode }) {
  const [s, d] = useReducer(reduce, empty);
  const ready = s.ready;

  useEffect(() => {
    let saved: Partial<S> | null = null;
    try {
      const raw = localStorage.getItem(SAVE);
      if (raw) saved = JSON.parse(raw);
    } catch { /* storage blocked or bad data: start fresh */ }
    d({ t: "hydrate", s: saved });
  }, []);
  useEffect(() => {
    if (!ready) return;
    try { localStorage.setItem(SAVE, JSON.stringify({ ...s, ready: undefined })); } catch { /* storage blocked */ }
  }, [s, ready]);

  return <Ctx.Provider value={{ s, d, ready }}>{children}</Ctx.Provider>;
}
export const useStore = () => useContext(Ctx);
