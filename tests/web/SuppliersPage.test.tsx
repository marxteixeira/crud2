import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import SuppliersPageClient from "@/app/suppliers/SuppliersPageClient";
import NewSupplierPage from "@/app/suppliers/new/page";
import SupplierForm from "@/components/suppliers/SupplierForm";
import { listSuppliers } from "@/lib/suppliers";

const mocks = vi.hoisted(() => ({
  replace: vi.fn(),
  listSuppliers: vi.fn(),
  createSupplier: vi.fn(),
  getCurrentAccount: vi.fn(),
  search: "search=distribuidora&status=Active&page=2",
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: mocks.replace }),
  usePathname: () => "/suppliers",
  useSearchParams: () => new URLSearchParams(mocks.search),
}));
vi.mock("@/lib/supabase/client", () => ({ getSupabaseBrowserClient: () => ({}) }));
vi.mock("@/lib/suppliers", () => ({
  listSuppliers: mocks.listSuppliers,
  getSupplier: vi.fn(),
  createSupplier: mocks.createSupplier,
  updateSupplier: vi.fn(),
  setSupplierStatus: vi.fn(),
}));
vi.mock("@/lib/auth", () => ({ getCurrentAccount: mocks.getCurrentAccount }));
vi.mock("@/components/AuthenticatedNavigation", () => ({
  default: () => <nav aria-label="Navegação principal">Navegação</nav>,
}));

const supplier = {
  id: "supplier-1",
  name: "Distribuidora Central",
  taxIdentifier: "52998224725",
  phone: null,
  email: null,
  contactName: null,
  notes: null,
  status: "Active" as const,
  createdAt: "2026-10-26T12:00:00Z",
  updatedAt: "2026-10-26T12:00:00Z",
  createdBy: "actor-1",
  updatedBy: "actor-1",
};

function renderPage(): void {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <SuppliersPageClient />
    </QueryClientProvider>,
  );
}

function renderNewSupplierPage(): void {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <NewSupplierPage />
    </QueryClientProvider>,
  );
}

describe("SuppliersPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mocks.search = "search=distribuidora&status=Active&page=2";
    mocks.listSuppliers.mockResolvedValue({ items: [supplier], page: 2, pageSize: 25, total: 60 });
    mocks.createSupplier.mockResolvedValue(supplier);
    mocks.getCurrentAccount.mockResolvedValue({
      id: "account-1", email: "admin@example.com", displayName: "Admin",
      role: "Administrador", status: "Ativa",
    });
  });

  it("loads the paginated supplier list using URL filters", async () => {
    renderPage();

    expect(await screen.findByText("Distribuidora Central")).toBeInTheDocument();
    expect(vi.mocked(listSuppliers)).toHaveBeenCalledWith(
      expect.anything(),
      { search: "distribuidora", status: "Active", page: 2, pageSize: 25 },
    );
    expect(screen.getByLabelText("Buscar fornecedor")).toHaveValue("distribuidora");
    expect(screen.getByLabelText("Situação")).toHaveValue("Active");
    expect(screen.getByRole("link", { name: "Cadastrar fornecedor" })).toBeInTheDocument();
  });

  it("writes search and pagination changes back to the URL", async () => {
    renderPage();
    await screen.findByText("Distribuidora Central");

    fireEvent.change(screen.getByLabelText("Buscar fornecedor"), { target: { value: "mercado" } });
    fireEvent.click(screen.getByRole("button", { name: "Pesquisar" }));
    expect(mocks.replace).toHaveBeenCalledWith(
      "/suppliers?search=mercado&status=Active&page=1",
      { scroll: false },
    );

    fireEvent.click(screen.getByRole("button", { name: "Próxima" }));
    expect(mocks.replace).toHaveBeenCalledWith(
      "/suppliers?search=distribuidora&status=Active&page=3",
      { scroll: false },
    );
  });

  it("does not offer supplier creation to an Operator", async () => {
    mocks.getCurrentAccount.mockResolvedValue({
      id: "account-2", email: "operator@example.com", displayName: "Operador",
      role: "Operador", status: "Ativa",
    });
    renderPage();

    await screen.findByText("Distribuidora Central");
    expect(screen.queryByRole("link", { name: "Cadastrar fornecedor" })).not.toBeInTheDocument();
  });

  it("validates the required identifier before submitting", async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(<SupplierForm submitting={false} onSubmit={onSubmit} />);

    fireEvent.change(screen.getByLabelText("Nome ou razão social"), { target: { value: "Fornecedor" } });
    fireEvent.change(screen.getByLabelText("CPF ou CNPJ"), { target: { value: "123" } });
    fireEvent.click(screen.getByRole("button", { name: "Cadastrar fornecedor" }));

    expect(await screen.findByText("CPF deve ter 11 dígitos e CNPJ deve ter 14 dígitos.")).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("submits optional empty fields as null", async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(<SupplierForm submitting={false} onSubmit={onSubmit} />);

    fireEvent.change(screen.getByLabelText("Nome ou razão social"), { target: { value: "Fornecedor" } });
    fireEvent.change(screen.getByLabelText("CPF ou CNPJ"), { target: { value: "529.982.247-25" } });
    fireEvent.click(screen.getByRole("button", { name: "Cadastrar fornecedor" }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledWith({
        name: "Fornecedor",
        taxIdentifier: "529.982.247-25",
        phone: null,
        email: null,
        contactName: null,
        notes: null,
      }));
  });

  it("shows a clear empty state when no suppliers match", async () => {
    mocks.listSuppliers.mockResolvedValue({ items: [], page: 1, pageSize: 25, total: 0 });
    renderPage();

    expect(await screen.findByText("Nenhum fornecedor encontrado.")).toBeInTheDocument();
  });

  it("reports supplier list failures instead of showing an empty result", async () => {
    mocks.listSuppliers.mockRejectedValue(new Error("network unavailable"));
    renderPage();

    expect(await screen.findByRole("alert")).toHaveTextContent("Não foi possível carregar os fornecedores.");
  });

  it("shows API conflict feedback on the supplier form", () => {
    render(
      <SupplierForm
        submitting={false}
        errorMessage="Já existe um fornecedor com esse CPF ou CNPJ."
        onSubmit={vi.fn().mockResolvedValue(undefined)}
      />,
    );

    expect(screen.getByRole("alert")).toHaveTextContent("Já existe um fornecedor com esse CPF ou CNPJ.");
  });

  it("does not allow an Operator to open supplier creation directly", async () => {
    mocks.getCurrentAccount.mockResolvedValue({
      id: "account-2", email: "operator@example.com", displayName: "Operador",
      role: "Operador", status: "Ativa",
    });
    renderNewSupplierPage();

    expect(await screen.findByText("Somente Administradores podem cadastrar fornecedores.")).toBeInTheDocument();
    expect(screen.queryByLabelText("Nome ou razão social")).not.toBeInTheDocument();
  });

  it("creates a supplier and navigates to its details", async () => {
    renderNewSupplierPage();
    fireEvent.change(await screen.findByLabelText("Nome ou razão social"), { target: { value: "Fornecedor" } });
    fireEvent.change(screen.getByLabelText("CPF ou CNPJ"), { target: { value: "529.982.247-25" } });
    fireEvent.click(screen.getByRole("button", { name: "Cadastrar fornecedor" }));

    await waitFor(() => {
      expect(mocks.createSupplier).toHaveBeenCalled();
      expect(mocks.replace).toHaveBeenCalledWith("/suppliers/supplier-1");
    });
  });

  it("shows a clear error when supplier creation fails", async () => {
    mocks.createSupplier.mockRejectedValue(new Error("conflict"));
    renderNewSupplierPage();
    fireEvent.change(await screen.findByLabelText("Nome ou razão social"), { target: { value: "Fornecedor" } });
    fireEvent.change(screen.getByLabelText("CPF ou CNPJ"), { target: { value: "529.982.247-25" } });
    fireEvent.click(screen.getByRole("button", { name: "Cadastrar fornecedor" }));

    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Não foi possível cadastrar o fornecedor. Verifique os dados e tente novamente.",
    );
  });
});
