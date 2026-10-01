"use client";
import { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { API, fetchOrders, Order } from "./api";

// Loads recent orders, then listens for the SignalR "OrderSubmitted" event from the C# API.
export function useOrders() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [live, setLive] = useState(false);

  useEffect(() => {
    let dead = false;
    let timer: ReturnType<typeof setTimeout>;
    const load = () => fetchOrders().then((o) => { if (!dead) setOrders(o); }).catch(() => {});

    // LogLevel.None: failures are shown by the Live/Offline pill, so the dev overlay stays quiet
    const conn = new HubConnectionBuilder()
      .withUrl(`${API}/hubs/orders`).withAutomaticReconnect().configureLogging(LogLevel.None).build();

    conn.on("OrderSubmitted", (o: Order) => setOrders((prev) => (prev.some((x) => x.id === o.id) ? prev : [...prev, o])));
    conn.onreconnecting(() => setLive(false));
    conn.onreconnected(() => { setLive(true); load(); });
    conn.onclose(() => setLive(false));

    const start = () => conn.start()
      .then(() => { if (!dead) { setLive(true); load(); } })
      .catch(() => { if (!dead) timer = setTimeout(start, 3000); });

    load();
    // Start on the next tick: React dev mode mounts effects twice, and this skips the throwaway first connection
    timer = setTimeout(start, 0);
    return () => { dead = true; clearTimeout(timer); conn.stop(); };
  }, []);

  return { orders, live };
}