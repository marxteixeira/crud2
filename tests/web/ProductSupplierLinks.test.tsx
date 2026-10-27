import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ProductSuppliers from "@/components/products/ProductSuppliers";
import SupplierProducts from "@/components/suppliers/SupplierProducts";

const mocks = vi.hoisted(() => ({
  listSuppliers: vi.fn(),
  listSuppliersForProduct: vi.fn(),
  listProductsForSupplier: vi.fn(),
  listProducts: vi.fn(),
  associateSupplier: vi.fn(),
  disassociateSupplier: vi.fn(),
}));

vi.mock("@/lib/suppliers", () => ({
  listSuppliers: mocks.listSuppliers,
  listSuppliersForProduct: mocks.listSuppliersForProduct,
  listProductsForSupplier: mocks.listProductsForSupplier,
  associateSupplier: mocks.associateSupplier,
  disassociateSupplier: mocks.disassociateSupplier,
}));
vi.mock("@/lib/products", () => ({ listProducts: mocks.listProducts }));
vi.mock("@/lib/supabase/client", () => ({ getSupabaseBrowserClient: () => ({}) }));

const supplier = {
  id: "supplier-1",
  name: "Fornecedor Central",
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

const product = {
  id: "product-1",
  name: "Café",
  quantity: 3,
  price: 12.5,
  shelfId: "shelf-1",
  rackCode: "A",
  shelfCode: "1",
  status: "Active" as const,
  createdAt: "2026-10-26T12:00:00Z",
  updatedAt: "2026-10-26T12:00:00Z",
  createdBy: "actor-1",
  updatedBy: "actor-1",
};

function renderWithQuery(component: JSX.Element): void {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(<QueryClientProvider client={queryClient}>{component}</QueryClientProvider>);
}

describe("product-supplier links", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mocks.listSuppliers.mockResolvedValue({ items: [supplier], page: 1, pageSize: 100, total: 1 });
    mocks.listSuppliersForProduct.mockResolvedValue({
      items: [{
        supplierId: supplier.id,
        supplierName: supplier.name,
        taxIdentifier: supplier.taxIdentifier,
        supplierStatus: "Active",
        linkStatus: "Active",
        createdAt: supplier.createdAt,
        updatedAt: supplier.updatedAt,
        createdBy: supplier.createdBy,
        updatedBy: supplier.updatedBy,
      }],
      page: 1,
      pageSize: 100,
      total: 1,
    });
    mocks.listProducts.mockResolvedValue({ items: [product], page: 1, pageSize: 100, total: 1 });
    mocks.listProductsForSupplier.mockResolvedValue({
      items: [{
        productId: product.id,
        productName: product.name,
        productStatus: "Active",
        linkStatus: "Active",
        createdAt: product.createdAt,
        updatedAt: product.updatedAt,
        createdBy: product.createdBy,
        updatedBy: product.updatedBy,
      }],
      page: 1,
      pageSize: 100,
      total: 1,
    });
    mocks.associateSupplier.mockResolvedValue(undefined);
    mocks.disassociateSupplier.mockResolvedValue(undefined);
  });

  it("shows product supplier links and associates a selected active supplier", async () => {
    renderWithQuery(<ProductSuppliers product={product} />);

    expect(await screen.findByText(/Fornecedor Central —/)).toBeInTheDocument();
    fireEvent.change(screen.getByLabelText("Adicionar fornecedor"), { target: { value: supplier.id } });
    fireEvent.click(screen.getByRole("button", { name: "Associar" }));

    await waitFor(() => {
      expect(mocks.associateSupplier).toHaveBeenCalledWith(expect.anything(), product.id, supplier.id);
    });
  });

  it("shows products and associates a product from the supplier view", async () => {
    renderWithQuery(<SupplierProducts supplierId={supplier.id} active />);

    expect(await screen.findByText(/Café —/)).toBeInTheDocument();
    fireEvent.change(screen.getByLabelText("Adicionar produto"), { target: { value: product.id } });
    fireEvent.click(screen.getByRole("button", { name: "Associar" }));

    await waitFor(() => {
      expect(mocks.associateSupplier).toHaveBeenCalledWith(expect.anything(), product.id, supplier.id);
    });
  });

  it("does not offer new product associations to an inactive supplier", async () => {
    renderWithQuery(<SupplierProducts supplierId={supplier.id} active={false} />);

    expect(await screen.findByText(/Café —/)).toBeInTheDocument();
    expect(screen.queryByLabelText("Adicionar produto")).not.toBeInTheDocument();
  });

  it("disassociates a supplier from the product view", async () => {
    renderWithQuery(<ProductSuppliers product={product} />);

    fireEvent.click(await screen.findByRole("button", { name: "Desassociar" }));

    await waitFor(() => {
      expect(mocks.disassociateSupplier).toHaveBeenCalledWith(expect.anything(), product.id, supplier.id);
    });
  });

  it("keeps historical product links visible without offering another removal", async () => {
    mocks.listProductsForSupplier.mockImplementation(async (_client, _supplierId, filters) => ({
      items: [{
        productId: product.id,
        productName: product.name,
        productStatus: "Inactive",
        linkStatus: filters.linkStatus === "Inactive" ? "Inactive" : "Active",
        createdAt: product.createdAt,
        updatedAt: product.updatedAt,
        createdBy: product.createdBy,
        updatedBy: product.updatedBy,
      }],
      page: 1,
      pageSize: 100,
      total: 1,
    }));
    renderWithQuery(<SupplierProducts supplierId={supplier.id} active />);

    fireEvent.change(await screen.findByLabelText("Relações"), { target: { value: "Inactive" } });

    expect(await screen.findByText("Café — Inativo")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Desassociar" })).not.toBeInTheDocument();
  });

  it("shows all suppliers associated with a product", async () => {
    const anotherSupplier = { ...supplier, id: "supplier-2", name: "Fornecedor Norte" };
    mocks.listSuppliers.mockResolvedValue({
      items: [supplier, anotherSupplier],
      page: 1,
      pageSize: 100,
      total: 2,
    });
    mocks.listSuppliersForProduct.mockResolvedValue({
      items: [supplier, anotherSupplier].map((item) => ({
        supplierId: item.id,
        supplierName: item.name,
        taxIdentifier: item.taxIdentifier,
        supplierStatus: "Active" as const,
        linkStatus: "Active" as const,
        createdAt: item.createdAt,
        updatedAt: item.updatedAt,
        createdBy: item.createdBy,
        updatedBy: item.updatedBy,
      })),
      page: 1,
      pageSize: 100,
      total: 2,
    });
    renderWithQuery(<ProductSuppliers product={product} />);

    expect(await screen.findByText(/Fornecedor Norte —/)).toBeInTheDocument();
    expect(screen.getAllByRole("button", { name: "Desassociar" })).toHaveLength(2);
  });

  it("shows a clear empty state when a supplier has no linked products", async () => {
    mocks.listProductsForSupplier.mockResolvedValue({ items: [], page: 1, pageSize: 100, total: 0 });
    renderWithQuery(<SupplierProducts supplierId={supplier.id} active />);

    expect(await screen.findByText("Nenhum produto nesta relação.")).toBeInTheDocument();
  });
});
