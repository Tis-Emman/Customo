import type { Dish } from "./menu";

export const API = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080";

export type OrderItem = { name: string; qty: number; opts: string[]; removed: string[]; conflict: boolean };
export type Order = { id: number; table: number; allergies: string[]; items: OrderItem[]; total: number; createdAt: number };
export type NewOrder = { table: number; tableToken: string; allergies: string[]; items: { menuItemId: number; quantity: number; optionIds: number[]; removedIngredientIds: number[] }[] };

async function get<T>(path: string): Promise<T> {
  const r = await fetch(`${API}${path}`);
  if (!r.ok) throw new Error(`${path} failed (${r.status})`);
  return r.json();
}

export const fetchMenu = () => get<Dish[]>("/api/menu");
export const fetchOrders = () => get<Order[]>("/api/orders");

export async function postOrder(body: NewOrder): Promise<Order> {
  const r = await fetch(`${API}/api/orders`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) });
  if (!r.ok) throw new Error(await r.text());
  return r.json();
}

export async function completeOrder(id: number): Promise<void> {
  const r = await fetch(`${API}/api/orders/${id}/complete`, { method: "POST" });
  if (!r.ok) throw new Error(`complete failed (${r.status})`);
}

export type Table = { number: number; taken: boolean; mine: boolean };

export const fetchTables = (token: string | null) =>
  get<Table[]>(`/api/tables${token ? `?token=${encodeURIComponent(token)}` : ""}`);

// Returns the claim token, or null if the table is taken
export async function claimTable(number: number, token: string | null): Promise<string | null> {
  const r = await fetch(`${API}/api/tables/${number}/claim`, {
    method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ token }),
  });
  if (r.status === 409) return null;
  if (!r.ok) throw new Error("claim failed");
  return (await r.json()).token;
}

export const releaseTable = (number: number) => fetch(`${API}/api/tables/${number}/release`, { method: "POST" });
