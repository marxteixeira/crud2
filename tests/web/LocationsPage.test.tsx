import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen } from "@testing-library/react";
import { AxiosError, AxiosHeaders } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";
import LocationsPageClient from "@/app/locations/LocationsPageClient";
import { createShelf, listRacks } from "@/lib/locations";

const mocks = vi.hoisted(() => ({
  createRack: vi.fn(),
  createShelf: vi.fn(),
  listRacks: vi.fn(),
  setRackStatus: vi.fn(),
  setShelfStatus: vi.fn(),
  updateRack: vi.fn(),
  updateShelf: vi.fn(),
}));

vi.mock("@/lib/locations", () => ({
  createRack: mocks.createRack,
  createShelf: mocks.createShelf,
  listRacks: mocks.listRacks,
  setRackStatus: mocks.setRackStatus,
  setShelfStatus: mocks.setShelfStatus,
  updateRack: mocks.updateRack,
  updateShelf: mocks.updateShelf,
}));
vi.mock("@/lib/supabase/client", () => ({ getSupabaseBrowserClient: () => ({}) }));
vi.mock("@/components/AuthenticatedNavigation", () => ({
  default: () => <nav aria-label="Navegação principal">Navegação</nav>,
}));

const racks = [
  {
    id: "rack-a",
    code: "A",
    description: "Parede",
    status: "Active" as const,
    shelves: [{ id: "shelf-a1", rackId: "rack-a", code: "1", status: "Active" as const }],
  },
  {
    id: "rack-b",
    code: "B",
    description: null,
    status: "Active" as const,
    shelves: [{ id: "shelf-b1", rackId: "rack-b", code: "1", status: "Active" as const }],
  },
];

function renderPage(): void {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <LocationsPageClient />
    </QueryClientProvider>,
  );
}

describe("LocationsPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mocks.listRacks.mockResolvedValue(racks);
    mocks.createRack.mockResolvedValue(racks[0]);
    mocks.createShelf.mockResolvedValue(racks[0].shelves[0]);
    mocks.setRackStatus.mockResolvedValue(undefined);
    mocks.setShelfStatus.mockResolvedValue(undefined);
    mocks.updateRack.mockResolvedValue(racks[0]);
    mocks.updateShelf.mockResolvedValue(racks[0].shelves[0]);
  });

  it("shows racks and only the selected rack's shelves", async () => {
    renderPage();

    expect(await screen.findByRole("region", { name: "Prateleiras da estante A" })).toBeInTheDocument();
    expect(screen.getByLabelText("Estante selecionada")).toHaveValue("rack-a");
    fireEvent.change(screen.getByLabelText("Estante selecionada"), { target: { value: "rack-b" } });
    expect(await screen.findByRole("region", { name: "Prateleiras da estante B" })).toBeInTheDocument();
  });

  it("creates a shelf under the selected rack", async () => {
    renderPage();
    await screen.findByRole("region", { name: "Prateleiras da estante A" });

    fireEvent.change(screen.getByLabelText("Estante selecionada"), { target: { value: "rack-b" } });
    await screen.findByRole("region", { name: "Prateleiras da estante B" });
    fireEvent.change(screen.getByLabelText("Código da prateleira"), { target: { value: "2" } });
    fireEvent.click(screen.getByRole("button", { name: "Cadastrar prateleira" }));

    expect(await vi.mocked(createShelf)).toHaveBeenCalledWith(
      expect.anything(),
      "rack-b",
      { code: "2" },
    );
  });

  it("shows accessible loading and error states", async () => {
    mocks.listRacks.mockImplementation(() => new Promise(() => {}));
    renderPage();
    expect(screen.getByRole("status")).toHaveTextContent("Carregando estantes...");

    mocks.listRacks.mockRejectedValue(new Error("request failed"));
    const errorClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    render(
      <QueryClientProvider client={errorClient}>
        <LocationsPageClient />
      </QueryClientProvider>,
    );
    expect(await screen.findByRole("alert")).toHaveTextContent("Não foi possível carregar as localizações.");
  });

  it("shows the trace id when creating a rack fails with an internal error", async () => {
    mocks.createRack.mockRejectedValue(
      new AxiosError("fail", "ERR_BAD_RESPONSE", undefined, undefined, {
        status: 500,
        data: { traceId: "trace-123" },
        statusText: "",
        headers: {},
        config: { headers: new AxiosHeaders() },
      }),
    );
    renderPage();
    await screen.findByRole("region", { name: "Prateleiras da estante A" });

    fireEvent.change(screen.getByLabelText("Código da estante"), { target: { value: "C" } });
    fireEvent.click(screen.getByRole("button", { name: "Cadastrar estante" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("trace-123");
  });
});
