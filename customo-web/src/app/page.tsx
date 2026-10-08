"use client";
import { useEffect, useState } from "react";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { claimTable, releaseTable } from "@/lib/api";
import { useTables } from "@/lib/tables";
import { useStore } from "@/lib/store";

const KEY = "customo-table-token";

export default function Welcome() {
  const { s, d } = useStore();
  const router = useRouter();
  const [busy, setBusy] = useState(false);
  const [note, setNote] = useState("");
  // The claim token lives in this browser, so a refresh doesn't lock the customer out of their own table
  const saved = typeof window === "undefined" ? null : localStorage.getItem(KEY);
  const { tables } = useTables(saved);

  // After a refresh, take back the table this browser already holds
  useEffect(() => {
    const mine = tables.find((t) => t.mine);
    if (mine && saved && !s.tableToken) d({ t: "table", n: mine.number, token: saved });
  }, [tables, saved, s.tableToken, d]);

  const pickTable = async (n: number) => {
    setBusy(true); setNote("");
    try {
      const token = await claimTable(n, saved);
      if (!token) { setNote(`Table ${n} was just taken. Please pick another.`); return; }
      if (s.tableToken && s.table !== n) releaseTable(s.table);   // give back the table held before
      localStorage.setItem(KEY, token);
      d({ t: "table", n, token });
    } catch {
      setNote("Can't reach the server. Please try again.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <main className="mx-auto flex min-h-screen max-w-2xl flex-col items-center justify-center gap-6 p-6 text-center">
      <Image src="/logo.png" alt="Customo" width={240} height={240} priority className="rounded-full" />
      <h1 className="text-2xl text-mute">Welcome! Which table are you at?</h1>
      {tables.length === 0 && <p className="text-mute">Loading tables… make sure the API is running.</p>}
      <div className="grid grid-cols-4 gap-3 sm:grid-cols-6">
        {tables.map((t) => {
          const mine = s.tableToken !== null && s.table === t.number;
          return (
            <button key={t.number} disabled={t.taken || busy} aria-pressed={mine} onClick={() => pickTable(t.number)}
              className={`flex h-[72px] w-24 flex-col items-center justify-center rounded-2xl border text-2xl font-bold disabled:cursor-not-allowed ${
                mine ? "border-ink bg-ink text-cream" : t.taken ? "border-line bg-[#EFE7DA] text-mute" : "cursor-pointer border-[#CDBFAE] bg-white"}`}>
              {t.number}
              {t.taken && <span className="text-xs font-normal">Taken</span>}
            </button>
          );
        })}
      </div>
      {note && <p role="alert" className="font-bold text-tomato">{note}</p>}
      <button className="btn btn-primary w-full max-w-sm" disabled={!s.tableToken} onClick={() => router.push("/allergies")}>Start ordering</button>
      <p className="text-mute">Taken tables are greyed out. Next, you can tell us about any food allergies.</p>
    </main>
  );
}
