import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import UsersPageClient from "@/app/users/UsersPageClient";
import { listUsers } from "@/lib/users";

const mocks = vi.hoisted(() => ({
  replace: vi.fn(),
  listUsers: vi.fn(),
  search: "search=ana&status=Ativa&page=2",
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: mocks.replace }),
  usePathname: () => "/users",
  useSearchParams: () => new URLSearchParams(mocks.search),
}));
vi.mock("@/lib/supabase/client", () => ({ getSupabaseBrowserClient: () => ({}) }));
vi.mock("@/lib/users", () => ({
  listUsers: mocks.listUsers,
  inviteUser: vi.fn(),
  setUserStatus: vi.fn(),
}));
vi.mock("@/components/AuthenticatedNavigation", () => ({
  default: () => <nav aria-label="Navegação principal">Navegação</nav>,
}));

const pageData = {
  items: [{
    id: "user-1",
    email: "ana@example.com",
    displayName: "Ana",
    pendingEmail: null,
    role: "Operador" as const,
    status: "Ativa" as const,
  }],
  page: 2,
  pageSize: 25,
  total: 60,
};

function renderPage(): void {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <UsersPageClient />
    </QueryClientProvider>,
  );
}

describe("UsersPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mocks.search = "search=ana&status=Ativa&page=2";
    mocks.listUsers.mockResolvedValue(pageData);
  });

  it("loads paginated users with filters represented in the URL", async () => {
    renderPage();

    expect(await screen.findByText("ana@example.com")).toBeInTheDocument();
    expect(vi.mocked(listUsers)).toHaveBeenCalledWith(
      expect.anything(),
      { search: "ana", status: "Ativa", page: 2, pageSize: 25 },
    );
    expect(screen.getByLabelText("Buscar")).toHaveValue("ana");
    expect(screen.getByLabelText("Situação")).toHaveValue("Ativa");
  });

  it("writes search and pagination changes back to the URL", async () => {
    renderPage();
    await screen.findByText("ana@example.com");

    fireEvent.change(screen.getByLabelText("Buscar"), { target: { value: "bia" } });
    fireEvent.click(screen.getByRole("button", { name: "Pesquisar" }));
    expect(mocks.replace).toHaveBeenCalledWith("/users?search=bia&status=Ativa&page=1", { scroll: false });

    fireEvent.click(screen.getByRole("button", { name: "Próxima" }));
    expect(mocks.replace).toHaveBeenCalledWith("/users?search=ana&status=Ativa&page=3", { scroll: false });
  });

  it("shows loading, empty, and error states accessibly", async () => {
    mocks.listUsers.mockImplementation(() => new Promise(() => {}));
    renderPage();
    expect(screen.getByRole("status")).toHaveTextContent("Carregando contas...");

    mocks.listUsers.mockReset();
    mocks.listUsers.mockResolvedValue({ ...pageData, items: [], total: 0 });
    const emptyQueryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    const emptyView = render(
      <QueryClientProvider client={emptyQueryClient}>
        <UsersPageClient />
      </QueryClientProvider>,
    );
    expect(await screen.findByText("Nenhuma conta encontrada.")).toBeInTheDocument();
    emptyView.unmount();

    mocks.listUsers.mockRejectedValue(new Error("request failed"));
    const errorQueryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    render(
      <QueryClientProvider client={errorQueryClient}>
        <UsersPageClient />
      </QueryClientProvider>,
    );
    expect(await screen.findByRole("alert")).toHaveTextContent("Não foi possível carregar as contas.");
  });
});
