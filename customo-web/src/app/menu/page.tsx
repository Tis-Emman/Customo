"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import Header from "@/components/Header";
import { Customize, Warning } from "@/components/Dialogs";
import { CATS, Dish } from "@/lib/menu";
import { fetchMenu } from "@/lib/api";
import { conflicts, peso, total, unit, useStore } from "@/lib/store";

export default function Menu() {
  const { s, d } = useStore();
  const router = useRouter();
  const [cat, setCat] = useState("All");
  const [dish, setDish] = useState<Dish | null>(null);
  const [warn, setWarn] = useState(false);
  const [dishes, setDishes] = useState<Dish[] | null>(null);
  const [failed, setFailed] = useState(false);
  const load = () => { setFailed(false); fetchMenu().then(setDishes).catch(() => setFailed(true)); };
  useEffect(() => { fetchMenu().then(setDishes).catch(() => setFailed(true)); }, []);
  const pick = (x: Dish) => { setDish(x); setWarn(conflicts(x, s.allergies).length > 0); };

  return (
    <>
      <Header />
      <main className="mx-auto grid max-w-7xl gap-6 p-6 lg:grid-cols-[1fr_320px]">
        <section className="flex flex-col gap-4">
          <div className="flex flex-wrap gap-2" role="tablist">
            {CATS.map((c) => (
              <button key={c} role="tab" aria-selected={cat === c} onClick={() => setCat(c)} className={`pill ${cat === c ? "!border-0 !bg-tomato font-bold !text-white" : "bg-transparent"}`}>{c}</button>
            ))}
          </div>
          <div className="flex flex-wrap items-center gap-3 rounded-2xl border border-line bg-white p-4">
            <span className="text-sm text-mute">My allergies</span>
            {s.allergies.length ? s.allergies.map((a) => <span key={a} className="rounded-full bg-honey px-4 py-2 font-bold text-honeyink">{a}</span>) : <span>None</span>}
            <Link href="/allergies" className="btn ml-auto">Change</Link>
          </div>
          {failed && <p role="alert" className="rounded-2xl bg-honey p-4 font-bold text-honeyink">We can&apos;t reach the menu. Make sure the API is running. <button className="btn ml-2" onClick={load}>Try again</button></p>}
          {!dishes && !failed && <p className="text-mute">Loading the menu…</p>}
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {(dishes ?? []).filter((x) => cat === "All" || x.cat === cat).map((x) => {
              const hit = conflicts(x, s.allergies);
              return (
                <button key={x.id} onClick={() => pick(x)}
                  className={`flex min-h-44 cursor-pointer flex-col gap-1.5 rounded-2xl p-4 text-left ${hit.length ? "border-2 border-honeyedge bg-[#FFF7E0]" : "border border-line bg-white"}`}>
                  <span className="flex w-full justify-between"><span className="font-display text-xl font-semibold">{x.name}</span><b>{peso(x.price)}</b></span>
                  <span className="text-sm text-mute">{x.allergens.length ? `Contains: ${x.allergens.join(", ").toLowerCase()}` : "No common allergens"}</span>
                  <span className={`text-sm font-bold ${hit.length ? "text-honeyink" : "text-leaf"}`}>
                    {hit.length ? `Contains ${hit.join(" and ").toLowerCase()}. Matches your ${hit.length > 1 ? "allergies" : "allergy"}.` : "No flagged allergens"}
                  </span>
                  <span className="mt-auto font-bold text-tomato">Customize and add</span>
                </button>
              );
            })}
          </div>
        </section>

        <aside className="flex h-fit flex-col gap-3 rounded-2xl border border-line bg-white p-5 lg:sticky lg:top-6">
          <h2 className="font-display text-2xl font-bold">Your order</h2>
          {s.cart.length === 0 && <p className="text-mute">Pick a dish to start your order.</p>}
          {s.cart.map((l) => (
            <div key={l.key} className="border-b border-line pb-3">
              <div className="flex justify-between font-bold"><span>{l.qty}× {l.dish.name}</span><span>{peso(unit(l) * l.qty)}</span></div>
              {l.opts.filter((o) => o.type === "Extra").map((o) => <div key={o.id} className="text-sm text-mute">+ {o.name}</div>)}
            </div>
          ))}
          <div className="flex items-baseline justify-between"><span className="text-mute">Total</span><span className="font-display text-3xl font-bold">{peso(total(s.cart))}</span></div>
          <button className="btn btn-primary" disabled={!s.cart.length} onClick={() => router.push("/review")}>Review order</button>
        </aside>
      </main>

      {dish && warn && <Warning dish={dish} hit={conflicts(dish, s.allergies)} onCancel={() => setDish(null)} onContinue={() => setWarn(false)} />}
      {dish && !warn && (
        <Customize dish={dish} onClose={() => setDish(null)}
          onAdd={(opts, qty) => { d({ t: "add", l: { key: crypto.randomUUID(), dish, qty, opts } }); setDish(null); }} />
      )}
    </>
  );
}
