"use client";
import Link from "next/link";
import { ALLERGENS } from "@/lib/menu";
import { useStore } from "@/lib/store";

export default function Allergies() {
  const { s, d } = useStore();
  const toggle = (a: string) =>
    d({
      t: "allergies",
      a: s.allergies.includes(a)
        ? s.allergies.filter((x) => x !== a)
        : [...s.allergies, a],
    });
  return (
    <main className="mx-auto flex min-h-screen max-w-4xl flex-col items-center justify-center gap-6 p-6 text-center">
      <h1 className="font-display text-4xl font-bold">Any food allergies?</h1>
      <p className="text-lg text-mute">
        Tap all that apply. We will warn you on dishes and tell the kitchen.
      </p>
      <div className="grid w-full grid-cols-2 gap-3 sm:grid-cols-5">
        {ALLERGENS.map((a) => {
          const on = s.allergies.includes(a);
          return (
            <button
              key={a}
              aria-pressed={on}
              onClick={() => toggle(a)}
              className={`relative h-[68px] w-full cursor-pointer rounded-2xl border-2 text-lg font-medium ${
                on
                  ? "border-honeyedge bg-honey text-honeyink"
                  : "border-[#CDBFAE] bg-white"
              }`}
            >
              {on && (
                <span
                  aria-hidden="true"
                  className="absolute left-4 top-1/2 -translate-y-1/2"
                >
                  ✓
                </span>
              )}
              {a}
            </button>
          );
        })}
      </div>
      <p className="font-bold">
        {s.allergies.length
          ? `Selected: ${s.allergies.join(", ")}`
          : "No allergies selected"}
      </p>
      <div className="flex flex-wrap justify-center gap-4">
        {s.allergies.length === 0 ? (
          <Link href="/menu" className="btn h-16 px-8 text-lg">
            I have no allergies
          </Link>
        ) : (
          <Link href="/menu" className="btn btn-primary h-16">
            Continue to menu
          </Link>
        )}
      </div>
      <p className="text-mute">You can change this anytime from the menu.</p>
    </main>
  );
}
