"use client";
import Image from "next/image";
import Link from "next/link";
import { useStore } from "@/lib/store";

export default function Welcome() {
  const { s, d } = useStore();
  return (
    <main className="mx-auto flex min-h-screen max-w-2xl flex-col items-center justify-center gap-6 p-6 text-center">
      <Image src="/logo.png" alt="Customo" width={240} height={240} priority className="rounded-full" />
      <h1 className="text-2xl text-mute">Welcome! Which table are you at?</h1>
      <div className="grid grid-cols-4 gap-3 sm:grid-cols-6">
        {Array.from({ length: 12 }, (_, i) => i + 1).map((n) => (
          <button key={n} aria-pressed={s.table === n} onClick={() => d({ t: "table", n })}
            className={`h-[72px] w-24 cursor-pointer rounded-2xl border text-2xl font-bold ${s.table === n ? "border-ink bg-ink text-cream" : "border-[#CDBFAE] bg-white"}`}>{n}</button>
        ))}
      </div>
      <Link href="/allergies" className="btn btn-primary w-full max-w-sm">Start ordering</Link>
      <p className="text-mute">Next, you can tell us about any food allergies.</p>
    </main>
  );
}
