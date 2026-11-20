"use client";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { peso, useStore } from "@/lib/store";

export default function Confirmed() {
  const { s, d, ready } = useStore();
  const router = useRouter();
  const o = s.last;
  if (!ready) return null;
  if (!o) return <main className="p-10 text-center"><Link href="/" className="btn btn-primary inline-flex">Start an order</Link></main>;
  const takeOut = o.type === "TakeOut";

  // New customer on this device: clear this order's details and go back to the start
  const startOver = () => { d({ t: "reset" }); router.push("/"); };

  return (
    <main className="flex min-h-screen items-center justify-center p-6">
      <div className="flex w-full max-w-lg flex-col items-center gap-4 rounded-2xl border border-line bg-white p-9">
        <svg width="72" height="72" viewBox="0 0 72 72" aria-hidden="true"><circle cx="36" cy="36" r="34" fill="#1F6B3A" /><path d="M21 37l10 10 20-22" fill="none" stroke="#fff" strokeWidth="6" strokeLinecap="round" strokeLinejoin="round" /></svg>
        <h1 className="text-center font-display text-4xl font-bold">Order sent to the kitchen</h1>
        {takeOut ? (
          <div className="flex flex-col items-center">
            <span className="text-mute">Take-out · your order number</span>
            <span className="font-display text-6xl font-bold text-tomato">#{o.id}</span>
            <span className="text-mute">We&apos;ll call this number when it&apos;s packed.</span>
          </div>
        ) : (
          <p className="text-mute">Order #{o.id} · Table {o.table}</p>
        )}
        <ul className="w-full border-y border-line py-3">
          {o.items.map((i, k) => <li key={k} className="py-1"><b>{i.qty}× {i.name}</b>{i.removed.length > 0 && <span className="text-sm text-mute"> · no {i.removed.join(", no ").toLowerCase()}</span>}{i.opts.length > 0 && <span className="text-sm text-mute"> · {i.opts.join(", ")}</span>}</li>)}
        </ul>
        <div className="flex w-full justify-between text-2xl font-bold"><span>Total</span><span>{peso(o.total)}</span></div>
        {o.allergies.length > 0 && <div className="w-full rounded-xl bg-honey p-3 text-honeyink">Allergies sent to the kitchen: <b>{o.allergies.join(", ")}</b></div>}
        <Link href="/menu" className="btn btn-primary w-full">{takeOut ? "Order more (gets a new number)" : `Order more for Table ${o.table}`}</Link>
        <button className="btn h-14 w-full text-lg" onClick={startOver}>
          <span aria-hidden="true" className="mr-1 text-2xl leading-none">‹</span> Back to start (new order)
        </button>
      </div>
    </main>
  );
}
