"use client";
import { createContext, useContext, useReducer, ReactNode, Dispatch } from "react";
import type { Dish, Ingredient, Opt } from "./menu";
import type { Order } from "./api";

export type Line = { key: string; dish: Dish; qty: number; opts: Opt[]; removed: Ingredient[] };
type S = { table: number; tableToken: string | null; allergies: string[]; cart: Line[]; last: Order | null };
type A =
  | { t: "table"; n: number; token: string } | { t: "allergies"; a: string[] } | { t: "add"; l: Line }
  | { t: "remove"; key: string } | { t: "done"; order: Order };

function reduce(s: S, a: A): S {
  switch (a.t) {
    case "table": return { ...s, table: a.n, tableToken: a.token };
    case "allergies": return { ...s, allergies: a.a };
    case "add": return { ...s, cart: [...s.cart, a.l] };
    case "remove": return { ...s, cart: s.cart.filter((l) => l.key !== a.key) };
    case "done": return { ...s, cart: [], last: a.order };
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

const Ctx = createContext<{ s: S; d: Dispatch<A> }>(null!);
export function Provider({ children }: { children: ReactNode }) {
  const [s, d] = useReducer(reduce, { table: 0, tableToken: null, allergies: [], cart: [], last: null });
  return <Ctx.Provider value={{ s, d }}>{children}</Ctx.Provider>;
}
export const useStore = () => useContext(Ctx);
