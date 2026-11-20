"use client";
import { useEffect, useState } from "react";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { API } from "./api";

export function connectHub(): HubConnection {
  // LogLevel.None: connection problems are shown in the UI, so the dev overlay stays quiet
  return new HubConnectionBuilder().withUrl(`${API}/hubs/orders`).withAutomaticReconnect().configureLogging(LogLevel.None).build();
}

/**
 * Loads data and keeps it fresh: reloads when the API announces one of `events`,
 * after a reconnect, when the tab becomes visible again, and every `pollMs` as a safety net
 * (so a screen never stays wrong just because one live message was missed).
 * `fetcher` must be stable (a module-level function or a useCallback).
 */
export function useLive<T>(fetcher: () => Promise<T>, events: string, pollMs = 15000) {
  const [data, setData] = useState<T | null>(null);
  const [failed, setFailed] = useState(false);
  const [live, setLive] = useState(false);
  const [tick, setTick] = useState(0);

  useEffect(() => {
    let dead = false;
    let timer: ReturnType<typeof setTimeout>;
    const load = () => fetcher()
      .then((d) => { if (!dead) { setData(d); setFailed(false); } })
      .catch(() => { if (!dead) setFailed(true); });

    const conn = connectHub();
    for (const e of events.split(",").filter(Boolean)) conn.on(e, load);
    conn.onreconnecting(() => setLive(false));
    conn.onreconnected(() => { setLive(true); load(); });
    conn.onclose(() => setLive(false));
    const start = () => conn.start()
      .then(() => { if (!dead) { setLive(true); load(); } })
      .catch(() => { if (!dead) timer = setTimeout(start, 3000); });

    const poll = setInterval(load, pollMs);
    const onVisible = () => { if (document.visibilityState === "visible") load(); };
    document.addEventListener("visibilitychange", onVisible);

    load();
    // Start on the next tick: React dev mode mounts effects twice, and this skips the throwaway first connection
    timer = setTimeout(start, 0);
    return () => {
      dead = true; clearTimeout(timer); clearInterval(poll);
      document.removeEventListener("visibilitychange", onVisible);
      conn.stop();
    };
  }, [fetcher, events, pollMs, tick]);

  return { data, failed, live, reload: () => setTick((t) => t + 1) };
}
