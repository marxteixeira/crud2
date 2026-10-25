import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ProductsPageClient from "@/app/products/ProductsPageClient";

const mocks = vi.hoisted(() => ({
  listProducts: vi.fn(),
  listRacks: vi.fn(),
  replace: vi.fn(),
  queryString: "",
}));

vi.mock("@/lib/products", () => ({ listProducts: mocks.listProducts }));
vi.mock("@/lib/locations", () => ({ listRacks: mocks.listRacks }));
vi.mock("@/lib/supabase/client", () => ({ getSupabaseBrowserClient: () => ({}) }));
vi.mock("next/navigation", () => ({
  usePathname: () => "/products",
  useRouter: () => ({ replace: mocks.replace }),
  useSearchParams: () => new URLSearchParams(mocks.queryString),
}));
vi.mock("@/components/AuthenticatedNavigation", () => ({
  default: () => <nav>Navegação</nav>,
}));

function renderPage(): { rerenderPage: () => void } {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  const renderTree = (): JSX.Element =>
    <QueryClientProvider client={queryClient}><ProductsPageClient /></QueryClientProvider>;
  const rendered = render(renderTree());
  return { rerenderPage: () => rendered.rerender(renderTree()) };
}

describe("ProductsPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mocks.queryString = "";
    mocks.replace.mockImplementation((url: string) => {
      mocks.queryString = url.split("?")[1] ?? "";
    });
    mocks.listRacks.mockResolvedValue([{
      id: "rack-1",
      code: "A",
      description: null,
      status: "Active",
      shelves: [{ id: "shelf-1", rackId: "rack-1", code: "1", status: "Active" }],
    }]);
    mocks.listProducts.mockResolvedValue({
      items: [{
        id: "product-1",
        name: "Café",
        quantity: 3,
        price: 5.5,
        shelfId: "shelf-1",
        rackCode: "A",
        shelfCode: "1",
        status: "Active",
      }],
      page: 1,
      pageSize: 25,
      total: 1,
    });
  });

  it("lists products and writes combined filters into the URL", async () => {
    const { rerenderPage } = renderPage();
    expect(await screen.findByRole("link", { name: "Café" })).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText("Buscar produto"), { target: { value: "cafe" } });
    fireEvent.click(screen.getByRole("button", { name: "Pesquisar" }));
    rerenderPage();
    fireEvent.change(screen.getByLabelText("Estante"), { target: { value: "rack-1" } });
    rerenderPage();
    fireEvent.change(screen.getByLabelText("Prateleira"), { target: { value: "shelf-1" } });
    rerenderPage();
    fireEvent.change(screen.getByLabelText("Situação"), { target: { value: "Active" } });
    rerenderPage();

    await waitFor(() => expect(mocks.replace).toHaveBeenCalledWith(
      expect.stringContaining("status=Active"),
      { scroll: false },
    ));
    expect(mocks.replace).toHaveBeenCalledWith(
      expect.stringContaining("rackId=rack-1"),
      { scroll: false },
    );
    expect(mocks.replace).toHaveBeenCalledWith(
      expect.stringContaining("shelfId=shelf-1"),
      { scroll: false },
    );
  });

  it("shows the empty catalog state", async () => {
    mocks.listProducts.mockResolvedValue({ items: [], page: 1, pageSize: 25, total: 0 });
    renderPage();
    expect(await screen.findByText("Nenhum produto encontrado.")).toBeInTheDocument();
  });
});
