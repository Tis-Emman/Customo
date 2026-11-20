"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { ChevronLeft, CircleCheck, X } from "lucide-react";
import {
  ApiError, changeStock, fetchInventory, fetchMovements, Movement, saveStockSettings,
  StockItem, StockKind, stockStatus, StockStatus,
} from "@/lib/api";
import { useLive } from "@/lib/live";

const STATUS: Record<StockStatus, { label: string; cls: string }> = {
  off: { label: "Marked sold out", cls: "bg-[#7A2E1B] text-white" },
  out: { label: "Out of stock", cls: "bg-[#B93A1B] text-white" },
  low: { label: "Low", cls: "bg-[#FBBF24] text-[#2B1600]" },
  ok: { label: "In stock", cls: "bg-[#2F5D3A] text-white" },
  untracked: { label: "Not counted", cls: "bg-[#3A342D] text-[#C9BBA6]" },
};
const FILTERS = [
  { id: "attention", label: "Needs attention", test: (s: StockStatus) => s === "off" || s === "out" || s === "low" },
  { id: "tracked", label: "Counted", test: (s: StockStatus) => s !== "untracked" },
  { id: "all", label: "All", test: () => true },
] as const;
const REASON: Record<string, string> = { Sale: "Order", Restock: "Delivery", Waste: "Waste", Count: "Count" };

const fmt = (n: number) => (Number.isInteger(n) ? String(n) : n.toFixed(2).replace(/0$/, ""));

export default function Inventory() {
  const { data: items, live, failed, reload } = useLive<StockItem[]>(fetchInventory, "MenuChanged", 20000);
  const { data: moves, reload: reloadMoves } = useLive<Movement[]>(fetchMovements, "MenuChanged", 20000);
  const [filter, setFilter] = useState<(typeof FILTERS)[number]["id"]>("all");
  const [q, setQ] = useState("");
  const [openId, setOpenId] = useState<number | null>(null);
  const open = items?.find((i) => i.id === openId) ?? null;

  const counts = { off: 0, out: 0, low: 0, ok: 0, untracked: 0 };
  for (const i of items ?? []) counts[stockStatus(i)]++;
  const f = FILTERS.find((x) => x.id === filter)!;
  const shown = (items ?? [])
    .filter((i) => f.test(stockStatus(i)))
    .filter((i) => !q || (i.name + " " + i.usedIn.join(" ")).toLowerCase().includes(q.toLowerCase()));

  return (
    <main className="min-h-screen bg-[#1E1A16] p-6 text-[#F5EDE0] sm:p-8">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <div className="flex items-center gap-4">
          <Link href="/kitchen" className="flex h-11 items-center gap-1 rounded-full bg-[#3A342D] px-4 font-bold">
            <ChevronLeft aria-hidden="true" className="h-5 w-5" /> Kitchen
          </Link>
          <h1 className="font-display text-4xl font-bold">Inventory</h1>
        </div>
        <span className={`rounded-full px-4 py-2 font-bold ${live ? "bg-[#2F5D3A]" : "bg-[#7A2E1B]"}`}>{live ? "Live" : "Offline"}</span>
      </div>

      <div className="mb-5 grid grid-cols-2 gap-3 sm:grid-cols-4">
        <Stat n={counts.out} label="Out of stock" tone={counts.out ? "bad" : ""} />
        <Stat n={counts.low} label="Low" tone={counts.low ? "warn" : ""} />
        <Stat n={counts.off} label="Marked sold out" tone={counts.off ? "bad" : ""} />
        <Stat n={counts.ok + counts.low + counts.out} label="Being counted" tone="" />
      </div>

      <div className="mb-4 flex flex-wrap items-center gap-2">
        {FILTERS.map((x) => (
          <button key={x.id} aria-pressed={filter === x.id} onClick={() => setFilter(x.id)}
            className={`h-11 cursor-pointer rounded-full px-4 font-bold ${filter === x.id ? "bg-[#F5EDE0] text-[#1E1A16]" : "bg-[#3A342D]"}`}>
            {x.label}
          </button>
        ))}
        <input value={q} onChange={(e) => setQ(e.target.value)} placeholder="Search ingredient or dish"
          className="ml-auto h-11 w-full rounded-xl border border-[#4A423A] bg-[#2B2621] px-4 text-[#F5EDE0] placeholder:text-[#8F806C] sm:w-72" />
      </div>

      {failed && !items && (
        <p role="alert" className="mb-4 rounded-xl bg-[#7A2E1B] p-3 font-bold">
          Can&apos;t reach the server. <button className="underline" onClick={reload}>Try again</button>
        </p>
      )}
      {!items && !failed && <p className="text-[#C9BBA6]">Loading inventory…</p>}
      {items && shown.length === 0 && (
        <p className="flex items-center gap-2 text-[#C9BBA6]">
          {filter === "attention" ? <><CircleCheck aria-hidden="true" className="h-5 w-5 text-[#8FD19E]" /> Nothing is low or out.</> : "No ingredients match."}
        </p>
      )}

      <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
        {shown.map((i) => {
          const st = stockStatus(i);
          return (
            <button key={i.id} onClick={() => setOpenId(i.id)}
              className="flex cursor-pointer flex-col gap-2 rounded-2xl bg-[#2B2621] p-4 text-left hover:bg-[#342E28]">
              <div className="flex items-start justify-between gap-2">
                <span className="font-display text-2xl font-bold">{i.name}</span>
                <span className={`shrink-0 rounded-full px-3 py-1 text-sm font-bold ${STATUS[st].cls}`}>{STATUS[st].label}</span>
              </div>
              <div className="text-xl font-bold">
                {i.stock === null ? <span className="text-[#8F806C]">Not counted</span> : `${fmt(i.stock)} ${i.unit}`}
                {i.lowAt !== null && i.stock !== null && <span className="ml-2 text-sm font-normal text-[#8F806C]">alert at {fmt(i.lowAt)}</span>}
              </div>
              <div className="text-sm text-[#C9BBA6]">
                Used in: {i.usedIn.length ? i.usedIn.join(", ") : "no dishes"}{i.allergen && ` · allergen: ${i.allergen}`}
              </div>
            </button>
          );
        })}
      </div>

      <section className="mt-10">
        <h2 className="mb-3 font-display text-2xl font-bold">Recent stock changes</h2>
        {!moves?.length && <p className="text-[#C9BBA6]">No changes yet. Set a count on an ingredient to start tracking it.</p>}
        <ul className="divide-y divide-[#3A342D] rounded-2xl bg-[#2B2621]">
          {(moves ?? []).map((m) => (
            <li key={m.id} className="flex flex-wrap items-center gap-x-4 gap-y-1 px-4 py-3">
              <span className="w-24 text-sm text-[#8F806C]">
                {new Date(m.at).toLocaleString([], { month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" })}
              </span>
              <span className="flex-1 font-bold">{m.ingredient}</span>
              <span className={`w-28 text-right font-bold ${m.change < 0 ? "text-[#FF9B7A]" : "text-[#8FD19E]"}`}>
                {m.change > 0 ? "+" : ""}{fmt(m.change)} {m.unit}
              </span>
              <span className="w-40 text-sm text-[#C9BBA6]">
                {REASON[m.reason] ?? m.reason}{m.orderId ? ` #${m.orderId}` : ""}{m.note ? ` · ${m.note}` : ""}
              </span>
            </li>
          ))}
        </ul>
      </section>

      {open && <Manage item={open} onClose={() => setOpenId(null)} onSaved={() => { reload(); reloadMoves(); }} />}
    </main>
  );
}

function Stat({ n, label, tone }: { n: number; label: string; tone: "bad" | "warn" | "" }) {
  const cls = tone === "bad" ? "bg-[#7A2E1B]" : tone === "warn" ? "bg-[#FBBF24] text-[#2B1600]" : "bg-[#2B2621]";
  return (
    <div className={`rounded-2xl p-4 ${cls}`}>
      <div className="font-display text-4xl font-bold">{n}</div>
      <div className="font-bold">{label}</div>
    </div>
  );
}

function Manage({ item, onClose, onSaved }: { item: StockItem; onClose: () => void; onSaved: () => void }) {
  const [amount, setAmount] = useState("");
  const [note, setNote] = useState("");
  const [unit, setUnit] = useState(item.unit);
  const [lowAt, setLowAt] = useState(item.lowAt === null ? "" : String(item.lowAt));
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState<{ ok: boolean; text: string } | null>(null);
  const st = stockStatus(item);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => { if (e.key === "Escape") onClose(); };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [onClose]);

  const run = async (what: () => Promise<unknown>, done: string) => {
    setBusy(true); setMsg(null);
    try { await what(); setMsg({ ok: true, text: done }); onSaved(); }
    catch (e) { setMsg({ ok: false, text: e instanceof ApiError ? e.message : "Can't reach the server. Try again." }); }
    finally { setBusy(false); }
  };

  const num = Number(amount);
  const validAmount = amount.trim() !== "" && Number.isFinite(num) && num >= 0;
  const stockAction = (kind: StockKind, done: string) => {
    if (!validAmount) { setMsg({ ok: false, text: "Enter an amount first." }); return; }
    run(async () => { await changeStock(item.id, kind, num, note.trim()); setAmount(""); setNote(""); }, done);
  };

  const lowNum = lowAt.trim() === "" ? null : Number(lowAt);
  const saveSettings = (isAvailable = item.available) => {
    if (lowNum !== null && (!Number.isFinite(lowNum) || lowNum < 0)) { setMsg({ ok: false, text: "The low-stock alert must be 0 or more." }); return; }
    run(() => saveStockSettings(item.id, { unit: unit.trim(), lowAt: lowNum, isAvailable }),
      isAvailable === item.available ? "Settings saved." : isAvailable ? "Available again on the menu." : "Marked sold out on every tablet.");
  };

  const input = "h-12 rounded-xl border border-[#4A423A] bg-[#1E1A16] px-3 text-lg text-[#F5EDE0] placeholder:text-[#8F806C]";
  const btn = "h-12 cursor-pointer rounded-xl px-4 font-bold disabled:cursor-not-allowed disabled:opacity-40";

  return (
    <div className="fixed inset-0 z-10 flex items-center justify-center bg-black/70 p-4" onClick={onClose}>
      <div role="dialog" aria-modal="true" aria-labelledby="inv-title" onClick={(e) => e.stopPropagation()}
        className="flex max-h-full w-full max-w-lg flex-col gap-5 overflow-y-auto rounded-2xl bg-[#2B2621] p-6">
        <div className="flex items-start justify-between gap-3">
          <div>
            <h2 id="inv-title" className="font-display text-3xl font-bold">{item.name}</h2>
            <p className="text-sm text-[#C9BBA6]">Used in: {item.usedIn.join(", ") || "no dishes"}</p>
          </div>
          <button aria-label="Close" onClick={onClose} className="flex h-11 w-11 shrink-0 cursor-pointer items-center justify-center rounded-full bg-[#3A342D]"><X aria-hidden="true" className="h-5 w-5" /></button>
        </div>

        <div className="flex items-center justify-between rounded-xl bg-[#1E1A16] p-4">
          <div>
            <div className="text-sm text-[#C9BBA6]">On hand</div>
            <div className="font-display text-3xl font-bold">{item.stock === null ? "Not counted" : `${fmt(item.stock)} ${item.unit}`}</div>
          </div>
          <span className={`rounded-full px-3 py-1 text-sm font-bold ${STATUS[st].cls}`}>{STATUS[st].label}</span>
        </div>

        {/* Sold out switch: works whether or not the item is counted */}
        {item.available ? (
          <button disabled={busy} onClick={() => saveSettings(false)} className={`${btn} bg-[#7A2E1B] text-white`}>
            Mark sold out now
          </button>
        ) : (
          <button disabled={busy} onClick={() => saveSettings(true)} className={`${btn} bg-[#2F5D3A] text-white`}>
            Available again
          </button>
        )}

        <fieldset className="flex flex-col gap-3">
          <legend className="mb-2 font-bold">Update stock ({item.unit})</legend>
          <div className="flex gap-2">
            <input type="number" inputMode="decimal" min={0} step="any" value={amount} onChange={(e) => setAmount(e.target.value)}
              placeholder="Amount" aria-label={`Amount in ${item.unit}`} className={`${input} w-32`} />
            <input value={note} onChange={(e) => setNote(e.target.value)} placeholder="Note (optional)" maxLength={200} className={`${input} flex-1`} />
          </div>
          <div className="grid grid-cols-3 gap-2">
            <button disabled={busy} onClick={() => stockAction("restock", "Delivery added.")} className={`${btn} bg-[#2F5D3A] text-white`}>+ Delivery</button>
            <button disabled={busy || item.stock === null} onClick={() => stockAction("waste", "Waste recorded.")} className={`${btn} bg-[#3A342D]`}>− Waste</button>
            <button disabled={busy} onClick={() => stockAction("count", "Count saved.")} className={`${btn} bg-[#F5EDE0] text-[#1E1A16]`}>Set count</button>
          </div>
          <p className="text-sm text-[#8F806C]">
            Each serving of a dish uses 1 {item.unit}. When the count reaches 0, the dishes that need it show as sold out.
          </p>
        </fieldset>

        <fieldset className="flex flex-col gap-3">
          <legend className="mb-2 font-bold">Settings</legend>
          <div className="grid grid-cols-2 gap-2">
            <label className="flex flex-col gap-1 text-sm text-[#C9BBA6]">Unit
              <input value={unit} maxLength={20} onChange={(e) => setUnit(e.target.value)} className={input} />
            </label>
            <label className="flex flex-col gap-1 text-sm text-[#C9BBA6]">Low-stock alert at
              <input type="number" inputMode="decimal" min={0} step="any" value={lowAt} onChange={(e) => setLowAt(e.target.value)}
                placeholder="None" className={input} />
            </label>
          </div>
          <div className="flex gap-2">
            <button disabled={busy || !unit.trim()} onClick={() => saveSettings()} className={`${btn} flex-1 bg-[#3A342D]`}>Save settings</button>
            {item.stock !== null && (
              <button disabled={busy} onClick={() => run(() => changeStock(item.id, "untrack", 0), "No longer counted.")} className={`${btn} bg-transparent text-[#C9BBA6] underline`}>
                Stop counting
              </button>
            )}
          </div>
        </fieldset>

        {msg && <p role={msg.ok ? "status" : "alert"} className={`rounded-xl p-3 font-bold ${msg.ok ? "bg-[#2F5D3A]" : "bg-[#7A2E1B]"}`}>{msg.text}</p>}
      </div>
    </div>
  );
}
