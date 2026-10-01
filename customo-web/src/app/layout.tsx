import type { Metadata } from "next";
import { Fraunces, DM_Sans } from "next/font/google";
import "./globals.css";
import { Provider } from "@/lib/store";

const fr = Fraunces({ subsets: ["latin"], variable: "--font-fraunces" });
const dm = DM_Sans({ subsets: ["latin"], variable: "--font-dmsans" });
export const metadata: Metadata = { title: "Customo" };

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" className={`${fr.variable} ${dm.variable}`}>
      <body className="font-sans"><Provider>{children}</Provider></body>
    </html>
  );
}
