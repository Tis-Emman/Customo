import type { Dish } from "./menu";

export const API = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080";

export type OrderType = "DineIn" | "TakeOut";
export type OrderItem = { name: string; qty: number; opts: string[]; removed: string[]; conflict: boolean };
export type Order = { id: number; type: OrderType; table: number | null; allergies: string[]; items: OrderItem[]; total: number; createdAt: number };
export type NewOrder = {
  type: OrderType; table: number | null; tableToken: string | null; allergies: string[];
  items: { menuItemId: number; quantity: number; optionIds: number[]; removedIngredientIds: number[] }[];
};

// The API answers errors with a plain message (sometimes JSON-quoted); show it as text
export class ApiError extends Error {
  constructor(public status: number, message: string) { super(message); }
}
async function fail(r: Response): Promise<never> {
  const text = await r.text();
  let msg = text;
  try { const j = JSON.parse(text); if (typeof j === "string") msg = j; } catch { /* plain text */ }
  throw new ApiError(r.status, msg || `Request failed (${r.status})`);
}

async function get<T>(path: string): Promise<T> {
  const r = await fetch(`${API}${path}`, { cache: "no-store" });
  if (!r.ok) await fail(r);
  return r.json();
}
async function send(path: string, method: string, body?: unknown): Promise<Response> {
  const r = await fetch(`${API}${path}`, {
    method, headers: body === undefined ? undefined : { "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  if (!r.ok) await fail(r);
  return r;
}

export const fetchMenu = () => get<Dish[]>("/api/menu");
export const fetchOrders = () => get<Order[]>("/api/orders");

export const postOrder = async (body: NewOrder): Promise<Order> => (await send("/api/orders", "POST", body)).json();
export const completeOrder = async (id: number) => { await send(`/api/orders/${id}/complete`, "POST"); };

// ---------- Tables ----------
export type Table = { number: number; taken: boolean; mine: boolean; ordered: boolean };

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

// Customer gives back a table they picked by mistake (the API refuses once they have ordered)
export const leaveTable = (number: number, token: string) => send(`/api/tables/${number}/leave`, "POST", { token }).catch(() => {});
// Staff free a table from the kitchen screen
export const releaseTable = (number: number) => fetch(`${API}/api/tables/${number}/release`, { method: "POST" });

// ---------- Inventory ----------
export type StockItem = {
  id: number; name: string; unit: string; stock: number | null; lowAt: number | null;
  available: boolean; allergen: string | null; usedIn: string[];
};
export type Movement = { id: number; ingredient: string; unit: string; change: number; reason: string; orderId: number | null; note: string | null; at: number };
export type StockKind = "restock" | "waste" | "count" | "untrack";

export const fetchInventory = () => get<StockItem[]>("/api/inventory");
export const fetchMovements = () => get<Movement[]>("/api/inventory/movements?take=40");
export const saveStockSettings = (id: number, s: { unit: string; lowAt: number | null; isAvailable: boolean }) =>
  send(`/api/inventory/${id}`, "PUT", s);
export const changeStock = (id: number, kind: StockKind, amount: number, note?: string) =>
  send(`/api/inventory/${id}/stock`, "POST", { kind, amount, note: note || null });

// Ingredient status used by the inventory panel and the kitchen badge
export type StockStatus = "off" | "out" | "low" | "ok" | "untracked";
export function stockStatus(i: StockItem): StockStatus {
  if (!i.available) return "off";
  if (i.stock === null) return "untracked";
  if (i.stock <= 0) return "out";
  if (i.lowAt !== null && i.stock <= i.lowAt) return "low";
  return "ok";
}
