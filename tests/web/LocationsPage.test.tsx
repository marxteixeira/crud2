import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen, within } from "@testing-library/react";
import { AxiosError, AxiosHeaders } from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";
import LocationsPageClient from "@/app/locations/LocationsPageClient";
import { listRacks } from "@/lib/locations";

const mocks = vi.hoisted(() => ({
  createRack: vi.fn(),
  listRacks: vi.fn(),
  setRackStatus: vi.fn(),
  setShelfStatus: vi.fn(),
  updateRack: vi.fn(),
  updateShelf: vi.fn(),
}));

vi.mock("@/lib/locations", () => ({
  createRack: mocks.createRack,
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

  it("loads code and description of the selected rack into the edit fields", async () => {
    renderPage();

    const code = await screen.findByLabelText("Código", { selector: "#edit-rack-code" });
    const description = screen.getByLabelText("Descrição", { selector: "#edit-rack-description" });
    expect(code).toHaveValue("A");
    expect(description).toHaveValue("Parede");

    fireEvent.change(screen.getByLabelText("Estante selecionada"), { target: { value: "rack-b" } });
    expect(await screen.findByRole("region", { name: "Prateleiras da estante B" })).toBeInTheDocument();
    expect(screen.getByLabelText("Código", { selector: "#edit-rack-code" })).toHaveValue("B");
    expect(screen.getByLabelText("Descrição", { selector: "#edit-rack-description" })).toHaveValue("");

    fireEvent.change(screen.getByLabelText("Estante selecionada"), { target: { value: "rack-a" } });
    expect(await screen.findByRole("region", { name: "Prateleiras da estante A" })).toBeInTheDocument();
    expect(screen.getByLabelText("Código", { selector: "#edit-rack-code" })).toHaveValue("A");
    expect(screen.getByLabelText("Descrição", { selector: "#edit-rack-description" })).toHaveValue("Parede");
  });

  it("discards unsaved edit-field changes when another rack is selected", async () => {
    renderPage();

    const code = await screen.findByLabelText("Código", { selector: "#edit-rack-code" });
    fireEvent.change(code, { target: { value: "ZZ" } });
    expect(code).toHaveValue("ZZ");

    fireEvent.change(screen.getByLabelText("Estante selecionada"), { target: { value: "rack-b" } });
    expect(await screen.findByRole("region", { name: "Prateleiras da estante B" })).toBeInTheDocument();
    expect(screen.getByLabelText("Código", { selector: "#edit-rack-code" })).toHaveValue("B");
  });

  it("opens and cancels rack creation from the page action", async () => {
    renderPage();
    await screen.findByRole("region", { name: "Prateleiras da estante A" });

    expect(screen.queryByRole("form", { name: "Cadastrar estante" })).not.toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "+ Cadastrar Nova Estante" }));
    expect(screen.getByRole("dialog", { name: "Cadastrar Nova Estante" })).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: "Cancelar" }));

    expect(screen.queryByRole("dialog", { name: "Cadastrar Nova Estante" })).not.toBeInTheDocument();
    expect(mocks.createRack).not.toHaveBeenCalled();
  });

  it("creates a rack in the dialog and selects it after refreshing the list", async () => {
    const newRack = {
      id: "rack-c",
      code: "C",
      description: "Nova localização",
      status: "Active" as const,
      shelves: [],
    };
    mocks.createRack.mockResolvedValue(newRack);
    mocks.listRacks.mockResolvedValueOnce(racks).mockResolvedValueOnce([...racks, newRack]);
    renderPage();
    await screen.findByRole("region", { name: "Prateleiras da estante A" });

    fireEvent.click(screen.getByRole("button", { name: "+ Cadastrar Nova Estante" }));
    const dialog = screen.getByRole("dialog", { name: "Cadastrar Nova Estante" });
    fireEvent.change(within(dialog).getByLabelText("Código da estante"), { target: { value: "C" } });
    fireEvent.change(within(dialog).getByLabelText("Descrição"), { target: { value: "Nova localização" } });
    fireEvent.click(within(dialog).getByRole("button", { name: "Cadastrar estante" }));

    expect(await screen.findByRole("region", { name: "Prateleiras da estante C" })).toBeInTheDocument();
    expect(screen.queryByRole("dialog", { name: "Cadastrar Nova Estante" })).not.toBeInTheDocument();
    expect(screen.getByLabelText("Estante selecionada")).toHaveValue("rack-c");
    expect(mocks.createRack).toHaveBeenCalledWith(
      expect.anything(),
      { code: "C", description: "Nova localização" },
    );
  });

  it("keeps the new rack selected while the refreshed list is still loading", async () => {
    const newRack = { id: "rack-c", code: "C", description: null, status: "Active" as const, shelves: [] };
    let resolveRefresh: (value: typeof racks) => void = () => undefined;
    const delayedRefresh = new Promise<typeof racks>((resolve) => {
      resolveRefresh = resolve;
    });
    mocks.createRack.mockResolvedValue(newRack);
    mocks.listRacks.mockResolvedValueOnce(racks).mockReturnValueOnce(delayedRefresh);
    renderPage();
    await screen.findByRole("region", { name: "Prateleiras da estante A" });

    fireEvent.click(screen.getByRole("button", { name: "+ Cadastrar Nova Estante" }));
    const dialog = screen.getByRole("dialog", { name: "Cadastrar Nova Estante" });
    fireEvent.change(within(dialog).getByLabelText("Código da estante"), { target: { value: "C" } });
    fireEvent.click(within(dialog).getByRole("button", { name: "Cadastrar estante" }));

    expect(await screen.findByRole("region", { name: "Prateleiras da estante C" })).toBeInTheDocument();
    expect(screen.getByLabelText("Estante selecionada")).toHaveValue("rack-c");

    resolveRefresh([...racks, newRack] as typeof racks);
    await screen.findByRole("option", { name: /C/ });
    expect(screen.getByLabelText("Estante selecionada")).toHaveValue("rack-c");
    expect(screen.getByRole("region", { name: "Prateleiras da estante C" })).toBeInTheDocument();
  });

  it("keeps the current rack selected when rack creation fails", async () => {
    mocks.createRack.mockRejectedValue(new Error("falha"));
    renderPage();
    await screen.findByRole("region", { name: "Prateleiras da estante A" });
    fireEvent.change(screen.getByLabelText("Estante selecionada"), { target: { value: "rack-b" } });

    fireEvent.click(screen.getByRole("button", { name: "+ Cadastrar Nova Estante" }));
    const dialog = screen.getByRole("dialog", { name: "Cadastrar Nova Estante" });
    fireEvent.change(within(dialog).getByLabelText("Código da estante"), { target: { value: "C" } });
    fireEvent.click(within(dialog).getByRole("button", { name: "Cadastrar estante" }));

    expect(await within(dialog).findByRole("alert")).toBeInTheDocument();
    expect(screen.getByRole("dialog", { name: "Cadastrar Nova Estante" })).toBeInTheDocument();
    expect(screen.getByLabelText("Estante selecionada")).toHaveValue("rack-b");
  });

  it("keeps existing shelf management and removes shelf creation controls", async () => {
    renderPage();
    const shelfRegion = await screen.findByRole("region", { name: "Prateleiras da estante A" });

    expect(screen.queryByLabelText("Código da prateleira")).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Cadastrar prateleira" })).not.toBeInTheDocument();
    expect(within(shelfRegion).getByLabelText("Prateleira")).toHaveValue("1");
    expect(within(shelfRegion).getByRole("button", { name: "Salvar" })).toBeInTheDocument();
    expect(within(shelfRegion).getByRole("link", { name: "Ver produtos" })).toHaveAttribute(
      "href",
      "/locations/rack-a/shelves/shelf-a1",
    );
    expect(within(shelfRegion).getByRole("button", { name: "Inativar" })).toBeInTheDocument();
  });

  it("starts rack fields empty without examples", async () => {
    renderPage();
    await screen.findByRole("region", { name: "Prateleiras da estante A" });
    fireEvent.click(screen.getByRole("button", { name: "+ Cadastrar Nova Estante" }));

    const dialog = screen.getByRole("dialog", { name: "Cadastrar Nova Estante" });
    const code = within(dialog).getByLabelText("Código da estante");
    const description = within(dialog).getByLabelText("Descrição");
    expect(code).toHaveValue("");
    expect(code).not.toHaveAttribute("placeholder");
    expect(description).toHaveValue("");
    expect(description).not.toHaveAttribute("placeholder");
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

    fireEvent.click(screen.getByRole("button", { name: "+ Cadastrar Nova Estante" }));
    const dialog = screen.getByRole("dialog", { name: "Cadastrar Nova Estante" });
    fireEvent.change(within(dialog).getByLabelText("Código da estante"), { target: { value: "C" } });
    fireEvent.change(within(dialog).getByLabelText("Descrição"), { target: { value: "Nova localização" } });
    fireEvent.click(within(dialog).getByRole("button", { name: "Cadastrar estante" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("trace-123");
    expect(screen.getByRole("dialog", { name: "Cadastrar Nova Estante" })).toBeInTheDocument();
    const openDialog = within(screen.getByRole("dialog"));
    expect(openDialog.getByLabelText("Código da estante")).toHaveValue("C");
    expect(openDialog.getByLabelText("Descrição")).toHaveValue("Nova localização");
  });
});
