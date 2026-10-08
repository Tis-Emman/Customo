"use client";
import { useState } from "react";
import type { Dish, Ingredient, Opt } from "@/lib/menu";
import { conflicts, peso } from "@/lib/store";

function Modal({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="fixed inset-0 z-10 flex items-center justify-center bg-ink/60 p-4">
      <div role="dialog" aria-modal="true" aria-label={label} className="flex max-h-full w-full max-w-xl flex-col gap-5 overflow-y-auto rounded-2xl bg-white p-7">
        {children}
      </div>
    </div>
  );
}

type WarningProps = { dish: Dish; hit: string[]; fix: Ingredient[] | null; onCancel: () => void; onFix: () => void; onContinue: () => void };
export function Warning({ dish, hit, fix, onCancel, onFix, onContinue }: WarningProps) {
  return (
    <Modal label="Allergy warning">
      <h2 className="font-display text-2xl font-bold">This dish matches your allergy</h2>
      <div className="rounded-xl border-2 border-honeyedge bg-[#FFF7E0] p-4">
        <div className="font-display text-xl font-semibold">{dish.name}</div>
        <div className="font-bold text-honeyink">Contains {hit.join(", ").toLowerCase()}, which is on your allergy list.</div>
      </div>
      <p className="text-mute">The kitchen will be told about your allergies either way.</p>
      {fix ? (
        <>
          <button className="btn btn-primary" onClick={onFix}>Remove {fix.map((i) => i.name.toLowerCase()).join(" and ")} and add</button>
          <button className="btn h-14 text-lg" onClick={onCancel}>Choose another dish</button>
        </>
      ) : (
        <button className="btn btn-primary" onClick={onCancel}>Choose another dish</button>
      )}
      <button className="btn h-14 text-lg" onClick={onContinue}>Add anyway</button>
    </Modal>
  );
}

type CustomizeProps = {
  dish: Dish; allergies: string[]; initialRemoved: Ingredient[];
  onClose: () => void; onAdd: (opts: Opt[], qty: number, removed: Ingredient[]) => void;
};
export function Customize({ dish, allergies, initialRemoved, onClose, onAdd }: CustomizeProps) {
  const [pick, setPick] = useState<Opt[]>(() => (["Size", "Swap"] as const).flatMap((t) => dish.options.filter((o) => o.type === t).slice(0, 1)));
  const [gone, setGone] = useState<Ingredient[]>(initialRemoved);
  const [qty, setQty] = useState(1);
  const toggle = (o: Opt) =>
    setPick((p) => o.type === "Extra" ? (p.includes(o) ? p.filter((x) => x !== o) : [...p, o]) : [...p.filter((x) => x.type !== o.type), o]);
  const toggleIng = (i: Ingredient) => setGone((g) => (g.some((x) => x.id === i.id) ? g.filter((x) => x.id !== i.id) : [...g, i]));
  const sum = (dish.price + pick.reduce((t, o) => t + o.delta, 0)) * qty;
  const hit = conflicts(dish, allergies, gone);

  return (
    <Modal label={dish.name}>
      <div className="flex items-center justify-between">
        <h2 className="font-display text-3xl font-bold">{dish.name}</h2>
        <button className="btn w-11 !rounded-full text-2xl" aria-label="Close" onClick={onClose}>×</button>
      </div>
      <p className={`font-bold ${hit.length ? "text-honeyink" : "text-leaf"}`}>
        {hit.length ? `Still contains ${hit.join(" and ").toLowerCase()}, which matches your allergies.` : "No flagged allergens for you"}
      </p>

      {dish.ingredients.length > 0 && (
        <div>
          <div className="font-bold">Ingredients</div>
          <p className="mb-2 text-sm text-mute">Tap an ingredient to leave it out.</p>
          <div className="flex flex-wrap gap-2">
            {dish.ingredients.map((i) => {
              const off = gone.some((x) => x.id === i.id);
              const flagged = !off && i.allergen !== null && allergies.includes(i.allergen);
              return (
                <button key={i.id} disabled={!i.removable} aria-pressed={!off} onClick={() => toggleIng(i)}
                  className={`btn h-10 text-sm disabled:opacity-70 ${off ? "line-through opacity-60" : ""} ${flagged ? "!border-2 !border-honeyedge !bg-honey font-bold text-honeyink" : ""}`}>
                  {off ? `No ${i.name.toLowerCase()}` : i.name}{flagged ? " ⚠" : ""}{!i.removable ? " (required)" : ""}
                </button>
              );
            })}
          </div>
          {gone.some((i) => i.allergen) && <p className="mt-2 text-sm text-mute">For severe allergies, please also tell your server.</p>}
        </div>
      )}

      {(["Size", "Extra", "Swap"] as const).map((t) => {
        const opts = dish.options.filter((o) => o.type === t);
        if (!opts.length) return null;
        return (
          <div key={t}>
            <div className="mb-2 font-bold">{t === "Extra" ? "Extras" : t}</div>
            <div className="flex flex-wrap gap-2">
              {opts.map((o) => (
                <button key={o.id} aria-pressed={pick.includes(o)} onClick={() => toggle(o)} className={`btn h-12 ${pick.includes(o) ? "on !border-2" : ""}`}>
                  {o.name}{o.delta ? ` +${peso(o.delta)}` : ""}
                </button>
              ))}
            </div>
          </div>
        );
      })}
      <div className="flex items-center gap-4">
        <div className="flex items-center gap-1">
          <button className="btn h-12 w-12 text-2xl" aria-label="Less" disabled={qty < 2} onClick={() => setQty(qty - 1)}>−</button>
          <span className="w-10 text-center text-xl font-bold">{qty}</span>
          <button className="btn h-12 w-12 text-2xl" aria-label="More" onClick={() => setQty(qty + 1)}>+</button>
        </div>
        <button className="btn btn-primary flex-1" onClick={() => onAdd(pick, qty, gone)}>Add to order · {peso(sum)}</button>
      </div>
    </Modal>
  );
}
