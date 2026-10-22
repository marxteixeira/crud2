import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import HealthStatus from "@/components/HealthStatus";
import { getHealth } from "@/lib/api";

vi.mock("@/lib/api", () => ({ getHealth: vi.fn() }));

describe("HealthStatus", () => {
  it("mostra Healthy quando a API responde", async () => {
    vi.mocked(getHealth).mockResolvedValue({ status: "Healthy", timestamp: "2026-01-01T00:00:00Z" });
    render(<HealthStatus />);
    expect(await screen.findByText("API: Healthy")).toBeInTheDocument();
  });

  it("mostra indisponível quando a API falha", async () => {
    vi.mocked(getHealth).mockRejectedValue(new Error("network"));
    render(<HealthStatus />);
    expect(await screen.findByText("API: indisponível")).toBeInTheDocument();
  });
});
