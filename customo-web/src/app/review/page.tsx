"use client";
import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import Header, { NeedsSession } from "@/components/Header";
import { conflicts, lineIssue, peso, total, unit, useStore, where } from "@/lib/store";
import { ApiError, fetchMenu, postOrder } from "@/lib/api";
import { useLive } from "@/lib/live";
import type { Dish } from "@/lib/menu";

export default function ReviewPage() {
  return <NeedsSession><Review /></NeedsSession>;
}

function Review() {
  const { s, d } = useStore();
  const router = useRouter();
  const { data: dishes, reload } = useLive<Dish[]>(fetchMenu, "MenuChanged", 30000);
  const flagged = s.cart.filter((l) => conflicts(l.dish, s.allergies, l.removed).length).length;
  const issues = s.cart.filter((l) => lineIssue(l, dishes)).length;

  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [lostTable, setLostTable] = useState(false);

  const confirm = async () => {
    setBusy(true); setError(""); setLostTable(false);
    try {
      const takeOut = s.mode === "TakeOut";
      const order = await postOrder({
        type: takeOut ? "TakeOut" : "DineIn",
        table: takeOut ? null : s.table, tableToken: takeOut ? null : s.tableToken, allergies: s.allergies,
        items: s.cart.map((l) => ({ menuItemId: l.dish.id, quantity: l.qty, optionIds: l.opts.map((o) => o.id), removedIngredientIds: l.removed.map((i) => i.id) })),
      });
      d({ t: "done", order });
      router.push("/confirmed");
    } catch (e) {
      if (e instanceof ApiError && e.status === 409) {
        // Sold out, or the table is no longer held: the API says exactly what happened
        setError(e.message);
        setLostTable(e.message.includes("no longer reserved"));
        reload();
      } else {
        setError("We couldn't send your order. Check the connection and try again.");
      }
      setBusy(false);
    }
  };

  return (
    <>
      <Header back="/menu" />
      <main className="mx-auto grid max-w-5xl gap-6 p-6 lg:grid-cols-[1fr_340px]">
        <h1 className="font-display text-4xl font-bold lg:col-span-2">Review your order <span className="text-2xl font-normal text-mute">· {where(s)}</span></h1>
        <section className="flex flex-col gap-3">
          {s.cart.length === 0 && <p className="text-mute">Your order is empty. <Link href="/menu" className="font-bold text-tomato underline">Back to the menu</Link></p>}
          {s.cart.map((l) => {
            const hit = conflicts(l.dish, s.allergies, l.removed);
            const issue = lineIssue(l, dishes);
            return (
              <div key={l.key} className={`flex justify-between gap-4 rounded-2xl p-4 ${issue ? "border-2 border-tomato bg-[#FFF1EC]" : hit.length ? "border-2 border-honeyedge bg-[#FFF7E0]" : "border border-line bg-white"}`}>
                <div>
                  <div className="font-display text-xl font-semibold">{l.qty}× {l.dish.name}</div>
                  {l.removed.map((i) => <div key={i.id} className="text-sm font-bold text-honeyink">No {i.name.toLowerCase()}</div>)}
                  {l.opts.map((o) => <div key={o.id} className="text-sm text-mute">{o.type === "Extra" ? "+ " : ""}{o.name}</div>)}
                  {hit.length > 0 && <div className="mt-1 font-bold text-honeyink">Contains {hit.join(" and ").toLowerCase()}. Matches your allergy.</div>}
                  {issue && <div className="mt-1 font-bold text-tomato">{issue}</div>}
                </div>
                <div className="flex flex-col items-end gap-2">
                  <b className="text-xl">{peso(unit(l) * l.qty)}</b>
                  <button className="btn" onClick={() => d({ t: "remove", key: l.key })}>Remove</button>
                </div>
              </div>
            );
          })}
        </section>
        <aside className="flex h-fit flex-col gap-4 rounded-2xl border border-line bg-white p-6">
          <div>
            <div className="text-sm text-mute">My allergies</div>
            <div className="flex items-center justify-between"><b>{s.allergies.join(", ") || "None"}</b><Link href="/allergies" className="btn">Change</Link></div>
          </div>
          {flagged > 0 && <div className="rounded-xl bg-honey p-3 font-bold text-honeyink">{flagged} {flagged > 1 ? "items match" : "item matches"} your allergies. Please check before confirming.</div>}
          <div className="flex items-baseline justify-between border-t border-line pt-3"><span className="text-mute">Total</span><span className="font-display text-4xl font-bold">{peso(total(s.cart))}</span></div>
          {error && <p role="alert" className="font-bold text-tomato">{error}</p>}
          {lostTable && <Link href="/" className="btn h-14 text-lg font-bold">Pick my table again</Link>}
          <button className="btn btn-primary" disabled={!s.cart.length || busy || issues > 0} onClick={confirm}>{busy ? "Sending…" : "Confirm order"}</button>
          <Link href="/menu" className="btn h-14 text-lg">Back to menu</Link>
        </aside>
      </main>
    </>
  );
}
