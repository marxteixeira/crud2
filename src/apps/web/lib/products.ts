import type { SupabaseClient } from "@supabase/supabase-js";
import { requestWithSession } from "@/lib/api";

export type ProductStatus = "Active" | "Inactive";

export interface Product {
  id: string;
  name: string;
  quantity: number;
  price: number | null;
  shelfId: string | null;
  rackCode: string | null;
  shelfCode: string | null;
  status: ProductStatus;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  updatedBy: string;
}

export interface PaginatedProducts {
  items: Product[];
  page: number;
  pageSize: number;
  total: number;
}

export interface ProductInput {
  name: string;
  quantity: number;
  price?: number | null;
  shelfId?: string | null;
}

export interface ProductFilters {
  search?: string;
  rackId?: string;
  shelfId?: string;
  status?: ProductStatus;
  page: number;
  pageSize: number;
}

export async function listProducts(
  client: SupabaseClient,
  filters: ProductFilters,
): Promise<PaginatedProducts> {
  const query = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  });
  if (filters.search) query.set("search", filters.search);
  if (filters.rackId) query.set("rackId", filters.rackId);
  if (filters.shelfId) query.set("shelfId", filters.shelfId);
  if (filters.status) query.set("status", filters.status);
  return requestWithSession<PaginatedProducts>(client, "GET", `/api/products?${query.toString()}`);
}

export function getProduct(client: SupabaseClient, productId: string): Promise<Product> {
  return requestWithSession<Product>(client, "GET", `/api/products/${productId}`);
}

export function createProduct(client: SupabaseClient, input: ProductInput): Promise<Product> {
  return requestWithSession<Product>(client, "POST", "/api/products", input);
}

export function updateProduct(
  client: SupabaseClient,
  productId: string,
  input: ProductInput,
): Promise<Product> {
  return requestWithSession<Product>(client, "PATCH", `/api/products/${productId}`, input);
}

export function setProductStatus(
  client: SupabaseClient,
  productId: string,
  status: ProductStatus,
): Promise<void> {
  return requestWithSession<void>(client, "PUT", `/api/products/${productId}/status`, { status });
}

export function listProductsAtShelf(
  client: SupabaseClient,
  rackId: string,
  shelfId: string,
  filters: Pick<ProductFilters, "status" | "page" | "pageSize">,
): Promise<PaginatedProducts> {
  const query = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  });
  if (filters.status) query.set("status", filters.status);
  return requestWithSession<PaginatedProducts>(
    client,
    "GET",
    `/api/locations/racks/${rackId}/shelves/${shelfId}/products?${query.toString()}`,
  );
}
