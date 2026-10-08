"use client";
import { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { API, fetchTables, Table } from "./api";

// Table list that refreshes itself when the API announces "TablesChanged"
export function useTables(token: string | null) {
  const [tables, setTables] = useState<Table[]>([]);

  useEffect(() => {
    let dead = false;
    let timer: ReturnType<typeof setTimeout>;
    const load = () => fetchTables(token).then((t) => { if (!dead) setTables(t); }).catch(() => {});
    const conn = new HubConnectionBuilder().withUrl(`${API}/hubs/orders`).withAutomaticReconnect().configureLogging(LogLevel.None).build();

    conn.on("TablesChanged", load);
    conn.onreconnected(load);
    const start = () => conn.start().then(load).catch(() => { if (!dead) timer = setTimeout(start, 3000); });

    load();
    timer = setTimeout(start, 0);
    return () => { dead = true; clearTimeout(timer); conn.stop(); };
  }, [token]);

  return { tables };
}
