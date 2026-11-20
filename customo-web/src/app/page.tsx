"use client";
import { useEffect, useState } from "react";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { ChevronLeft, ShoppingBag, UtensilsCrossed } from "lucide-react";
import { claimTable, leaveTable } from "@/lib/api";
import { useTables } from "@/lib/tables";
import { useStore } from "@/lib/store";

// Older versions kept only the table token under this key; still read it so those tablets keep their table
const KEY = "customo-table-token";

export default function Welcome() {
  const { s, d, ready } = useStore();
  const router = useRouter();
  const [busy, setBusy] = useState(false);
  const [note, setNote] = useState("");
  // Only used for loading tables, never shown, so reading it during the first render is safe
  const [legacy] = useState<string | null>(() => {
    try { return typeof window === "undefined" ? null : localStorage.getItem(KEY); } catch { return null; }
  });

  const token = s.tableToken ?? legacy;
  const { tables, forToken, failed } = useTables(ready ? token : null);

  useEffect(() => {
    if (!tables.length || forToken !== token || !token) return;
    const mine = tables.find((t) => t.mine);
    // After a refresh, take back the table this browser already holds
    if (mine && !s.tableToken) d({ t: "table", n: mine.number, token });
    // Staff freed our table (or the unused hold ran out): stop showing it as ours
    if (!mine && s.tableToken === token) d({ t: "clearTable" });
  }, [tables, forToken, token, s.tableToken, d]);

  const pickTable = async (n: number) => {
    setBusy(true); setNote("");
    try {
      const fresh = await claimTable(n, token);
      if (!fresh) { setNote(`Table ${n} was just taken. Please pick another.`); return; }
      // Give back a table picked by mistake. The API refuses if that party already ordered,
      // so handing this tablet to a new group can never free a table where people are eating.
      if (s.tableToken && s.table !== n) leaveTable(s.table, s.tableToken);
      try { localStorage.setItem(KEY, fresh); } catch { /* storage blocked */ }
      d({ t: "table", n, token: fresh });
    } catch {
      setNote("Can't reach the server. Please try again.");
    } finally {
      setBusy(false);
    }
  };

  const takeOut = () => {
    if (s.tableToken) leaveTable(s.table, s.tableToken);   // only frees it if nothing was ordered there
    d({ t: "clearTable" });
    d({ t: "mode", m: "TakeOut" });
    router.push("/allergies");
  };

  // Step 1: dine in or take out
  if (s.mode !== "DineIn") {
    return (
      <main className="mx-auto flex min-h-screen max-w-2xl flex-col items-center justify-center gap-8 p-6 text-center">
        <Image src="/logo.png" alt="Customo" width={220} height={220} priority className="rounded-full" />
        <h1 className="font-display text-4xl font-bold">Welcome! How are you eating today?</h1>
        <div className="grid w-full grid-cols-1 gap-4 sm:grid-cols-2">
          <button onClick={() => d({ t: "mode", m: "DineIn" })}
            className="flex h-40 cursor-pointer flex-col items-center justify-center gap-2 rounded-2xl border-2 border-[#CDBFAE] bg-white hover:border-tomato">
            <UtensilsCrossed aria-hidden="true" className="h-12 w-12 text-tomato" strokeWidth={1.75} />
            <span className="font-display text-3xl font-bold">Dine in</span>
            <span className="text-mute">Eat here at a table</span>
          </button>
          <button onClick={takeOut}
            className="flex h-40 cursor-pointer flex-col items-center justify-center gap-2 rounded-2xl border-2 border-[#CDBFAE] bg-white hover:border-tomato">
            <ShoppingBag aria-hidden="true" className="h-12 w-12 text-tomato" strokeWidth={1.75} />
            <span className="font-display text-3xl font-bold">Take out</span>
            <span className="text-mute">Pack it to go</span>
          </button>
        </div>
      </main>
    );
  }

  // Step 2 (dine in): which table
  return (
    <main className="mx-auto flex min-h-screen max-w-2xl flex-col items-center justify-center gap-6 p-6 text-center">
      <div className="flex w-full justify-start">
        <button className="btn h-12 gap-1 pr-5 text-lg font-bold" onClick={() => { setNote(""); d({ t: "mode", m: null }); }}>
          <ChevronLeft aria-hidden="true" className="h-5 w-5" /> Back
        </button>
      </div>
      <Image src="/logo.png" alt="Customo" width={180} height={180} priority className="rounded-full" />
      <h1 className="text-2xl text-mute">Dine in. Which table are you at?</h1>
      {tables.length === 0 && <p className="text-mute">{failed ? "Can't reach the server. Retrying…" : "Loading tables…"}</p>}
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
