"use client";
import Image from "next/image";
import Link from "next/link";
import { canOrder, useStore, where } from "@/lib/store";

// back: where the "Back" button goes (leave it out to hide the button)
export default function Header({ back }: { back?: string }) {
  const { s } = useStore();
  return (
    <header className="mx-auto flex max-w-7xl items-center justify-between gap-3 px-6 pt-4">
      <div className="flex items-center gap-3">
        {back && <BackButton href={back} />}
        <Image src="/logo.png" alt="" width={56} height={56} className="rounded-full" />
        <span className="font-display text-3xl font-bold text-tomato">Customo</span>
      </div>
      <span className="rounded-full bg-ink px-5 py-2 font-bold text-cream">{where(s)}</span>
    </header>
  );
}

export function BackButton({ href, label = "Back" }: { href: string; label?: string }) {
  return (
    <Link href={href} className="btn h-12 gap-1 pr-5 text-lg font-bold" aria-label={label}>
      <span aria-hidden="true" className="text-2xl leading-none">‹</span> {label}
    </Link>
  );
}

// Pages after the start screen need a dine-in table or take-out. Without one, send the customer to the start.
export function NeedsSession({ children }: { children: React.ReactNode }) {
  const { s, ready } = useStore();
  if (!ready) return null;
  if (!canOrder(s)) {
    return (
      <main className="flex min-h-screen flex-col items-center justify-center gap-4 p-6 text-center">
        <p className="text-lg text-mute">Choose dine-in or take-out first.</p>
        <Link href="/" className="btn btn-primary inline-flex">Start an order</Link>
      </main>
    );
  }
  return <>{children}</>;
}
