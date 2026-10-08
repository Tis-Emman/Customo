"use client";
import { useEffect, useState } from "react";
import { completeOrder, releaseTable } from "@/lib/api";
import { useTables } from "@/lib/tables";
import { useOrders } from "@/lib/orders";

export default function Kitchen() {
  const { orders, live, dismiss } = useOrders();
  const { tables } = useTables(null);
  const occupied = tables.filter((t) => t.taken);
  const [arm, setArm] = useState<number | null>(null); // order waiting for a second tap
  const [failed, setFailed] = useState(false);
  const flagged = orders.filter((o) => o.allergies.length).length;
  const [freeing, setFreeing] = useState<number | null>(null); // table waiting for confirmation
  const pending =
    freeing === null ? 0 : orders.filter((o) => o.table === freeing).length;

  useEffect(() => {
    if (freeing === null) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") setFreeing(null);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [freeing]);

  const confirmFree = async () => {
    const n = freeing;
    setFreeing(null);
    if (n === null) return;
    setFailed(false);
    try {
      const r = await releaseTable(n);
      if (!r.ok) throw new Error();
    } catch {
      setFailed(true);
    }
  };

  // The confirm state clears itself after 3 seconds
  useEffect(() => {
    if (arm === null) return;
    const t = setTimeout(() => setArm(null), 3000);
    return () => clearTimeout(t);
  }, [arm]);

  const finish = async (id: number) => {
    if (arm !== id) {
      setArm(id);
      return;
    }
    setArm(null);
    setFailed(false);
    try {
      await completeOrder(id);
      dismiss(id);
    } catch {
      setFailed(true);
    }
  };

  return (
    <main className="min-h-screen bg-[#1E1A16] p-8 text-[#F5EDE0]">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="font-display text-4xl font-bold">Kitchen</h1>
        <div className="flex items-center gap-4">
          <span className="text-lg text-[#C9BBA6]">
            {orders.length} active orders · {flagged} with allergy flags
          </span>
          <span
            className={`rounded-full px-4 py-2 font-bold ${live ? "bg-[#2F5D3A]" : "bg-[#7A2E1B]"}`}
          >
            {live ? "Live" : "Offline"}
          </span>
        </div>
      </div>
      <div className="mb-6 flex flex-wrap items-center gap-2">
        <span className="text-[#C9BBA6]">Occupied tables:</span>
        {occupied.length === 0 && <span className="text-[#C9BBA6]">none</span>}
        {occupied.map((t) => (
          <button
            key={t.number}
            onClick={() => setFreeing(t.number)}
            className="h-11 cursor-pointer rounded-full bg-[#3A342D] px-4 font-bold"
          >
            Table {t.number} ✕
          </button>
        ))}
      </div>
      {failed && (
        <p role="alert" className="mb-4 rounded-xl bg-[#7A2E1B] p-3 font-bold">
          Could not mark the order as done. Check the connection and try again.
        </p>
      )}
      {orders.length === 0 && (
        <p className="text-xl text-[#C9BBA6]">
          {live
            ? "All caught up. New orders appear here as soon as customers confirm them."
            : "Can't reach the server. Start the API and this screen reconnects on its own."}
        </p>
      )}
      <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-4">
        {orders.map((o) => (
          <article
            key={o.id}
            className={`flex flex-col gap-3 rounded-2xl bg-[#2B2621] p-5 ${o.allergies.length ? "border-[3px] border-[#FBBF24]" : "border-[3px] border-transparent"}`}
          >
            <div className="flex items-baseline justify-between">
              <h2 className="font-display text-4xl font-bold">
                Table {o.table}
              </h2>
              <span className="text-[#C9BBA6]">#{o.id}</span>
            </div>
            <div className="text-[#C9BBA6]">
              {new Date(o.createdAt).toLocaleTimeString([], {
                hour: "2-digit",
                minute: "2-digit",
              })}
            </div>
            {o.allergies.length ? (
              <div className="rounded-xl bg-[#FBBF24] p-3 text-[#2B1600]">
                <div className="text-sm font-bold">
                  ALLERGY - CHECK BEFORE COOKING
                </div>
                <div className="text-2xl font-bold">
                  {o.allergies.join(" · ")}
                </div>
              </div>
            ) : (
              <div className="rounded-xl bg-[#3A342D] p-3 text-[#C9BBA6]">
                No allergies flagged
              </div>
            )}
            {o.items.map((i, k) => (
              <div key={k}>
                <div className="text-2xl font-bold">
                  {i.qty}× {i.name}
                  {i.conflict && (
                    <span className="ml-2 rounded bg-[#FBBF24] px-2 py-0.5 align-middle text-sm text-[#2B1600]">
                      matches allergy
                    </span>
                  )}
                </div>
                {i.removed.length > 0 && (
                  <div className="text-xl font-bold uppercase text-[#FF9B7A]">
                    No {i.removed.join(", no ")}
                  </div>
                )}
                {i.opts.length > 0 && (
                  <div className="text-lg text-[#C9BBA6]">
                    + {i.opts.join(", + ")}
                  </div>
                )}
              </div>
            ))}
            <button
              onClick={() => finish(o.id)}
              className={`mt-auto h-14 cursor-pointer rounded-xl text-lg font-bold ${arm === o.id ? "bg-[#FBBF24] text-[#2B1600]" : "bg-[#2F5D3A] text-white"}`}
            >
              {arm === o.id ? "Tap again to confirm" : "Done"}
            </button>
          </article>
        ))}
      </div>

      {freeing !== null && (
        <div
          className="fixed inset-0 z-10 flex items-center justify-center bg-black/70 p-4"
          onClick={() => setFreeing(null)}
        >
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="free-title"
            onClick={(e) => e.stopPropagation()}
            className="flex w-full max-w-md flex-col gap-4 rounded-2xl bg-[#2B2621] p-7"
          >
            <h2 id="free-title" className="font-display text-3xl font-bold">
              Free table {freeing}?
            </h2>
            <p className="text-[#C9BBA6]">
              The table becomes available right away, and new customers can pick
              it.
            </p>
            {pending > 0 && (
              <p className="rounded-xl bg-[#FBBF24] p-3 font-bold text-[#2B1600]">
                Table {freeing} still has {pending} active{" "}
                {pending === 1 ? "order" : "orders"} in the kitchen.
              </p>
            )}
            <div className="flex gap-3">
              <button
                autoFocus
                onClick={() => setFreeing(null)}
                className="h-14 flex-1 cursor-pointer rounded-xl bg-[#3A342D] text-lg font-bold"
              >
                Cancel
              </button>
              <button
                onClick={confirmFree}
                className="h-14 flex-1 cursor-pointer rounded-xl bg-[#B93A1B] text-lg font-bold text-white"
              >
                Free table
              </button>
            </div>
          </div>
        </div>
      )}
    </main>
  );
}
