"use client";
import { useCallback } from "react";
import { fetchTables, Table } from "./api";
import { useLive } from "./live";

// Table list that refreshes itself when the API announces "TablesChanged" (and every 10 s as a fallback).
// `forToken` says which token the list was loaded with, so callers never act on a list from before a new claim.
export function useTables(token: string | null) {
  const fetcher = useCallback(() => fetchTables(token).then((tables) => ({ tables, forToken: token })), [token]);
  const { data, failed } = useLive<{ tables: Table[]; forToken: string | null }>(fetcher, "TablesChanged", 10000);
  return { tables: data?.tables ?? [], forToken: data?.forToken ?? null, failed };
}
