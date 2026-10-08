"use client";
import Image from "next/image";
import { useStore } from "@/lib/store";

export default function Header() {
  const { s } = useStore();
  return (
    <header className="mx-auto flex max-w-7xl items-center justify-between px-6 pt-4">
      <div className="flex items-center gap-3">
        <Image src="/logo.png" alt="" width={56} height={56} className="rounded-full" />
        <span className="font-display text-3xl font-bold text-tomato">Customo</span>
      </div>
      <span className="rounded-full bg-ink px-5 py-2 font-bold text-cream">{s.table ? `Table ${s.table}` : "No table"}</span>
    </header>
  );
}
