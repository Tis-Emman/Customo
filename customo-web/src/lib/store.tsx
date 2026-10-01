"use client";
import { createContext, useContext, useReducer, ReactNode, Dispatch } from "react";
import type { Dish, Opt } from "./menu";
import type { Order } from "./api";

export type Line = { key: string; dish: Dish; qty: number; opts: Opt[] };
type S = { table: number; allergies: string[]; cart: Line[]; last: Order | null };
type A =
  | { t: "table"; n: number } | { t: "allergies"; a: string[] } | { t: "add"; l: Line }
  | { t: "remove"; key: string } | { t: "done"; order: Order };

function reduce(s: S, a: A): S {
  switch (a.t) {
    case "table": return { ...s, table: a.n };
    case "allergies": return { ...s, allergies: a.a };
    case "add": return { ...s, cart: [...s.cart, a.l] };
    case "remove": return { ...s, cart: s.cart.filter((l) => l.key !== a.key) };
    case "done": return { ...s, cart: [], last: a.order };
    default: return s;
  }
}

export const unit = (l: Line) => l.dish.price + l.opts.reduce((t, o) => t + o.delta, 0);
export const total = (c: Line[]) => c.reduce((t, l) => t + unit(l) * l.qty, 0);
export const conflicts = (d: Dish, allergies: string[]) => d.allergens.filter((x) => allergies.includes(x));
export const peso = (n: number) => `₱${n}`;

const Ctx = createContext<{ s: S; d: Dispatch<A> }>(null!);
export function Provider({ children }: { children: ReactNode }) {
  const [s, d] = useReducer(reduce, { table: 5, allergies: [], cart: [], last: null });
  return <Ctx.Provider value={{ s, d }}>{children}</Ctx.Provider>;
}
export const useStore = () => useContext(Ctx);
