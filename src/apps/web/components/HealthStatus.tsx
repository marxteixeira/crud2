"use client";

import { useEffect, useState } from "react";
import { getHealth } from "@/lib/api";

type State = "loading" | "healthy" | "unavailable";

export default function HealthStatus(): JSX.Element {
  const [state, setState] = useState<State>("loading");

  useEffect(() => {
    let active = true;
    getHealth()
      .then((health) => {
        if (active) setState(health.status === "Healthy" ? "healthy" : "unavailable");
      })
      .catch(() => {
        if (active) setState("unavailable");
      });
    return () => {
      active = false;
    };
  }, []);

  const label: string =
    state === "loading" ? "API: verificando..." : state === "healthy" ? "API: Healthy" : "API: indisponível";

  return (
    <p role="status" aria-label="Status da API" data-state={state}>
      {label}
    </p>
  );
}
