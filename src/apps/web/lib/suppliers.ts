import type { SupabaseClient } from "@supabase/supabase-js";
import { requestWithSession } from "@/lib/api";

export type SupplierStatus = "Active" | "Inactive";

export interface Supplier {
  id: string;
  name: string;
  taxIdentifier: string;
  phone: string | null;
  email: string | null;
  contactName: string | null;
  notes: string | null;
  status: SupplierStatus;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  updatedBy: string;
}

export interface PaginatedSuppliers {
  items: Supplier[];
  page: number;
  pageSize: number;
  total: number;
}

export interface SupplierInput {
  name: string;
  taxIdentifier: string;
  phone: string | null;
  email: string | null;
  contactName: string | null;
  notes: string | null;
}

export interface SupplierLink {
  supplierId: string;
  supplierName: string;
  taxIdentifier: string;
  supplierStatus: SupplierStatus;
  linkStatus: SupplierStatus;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  updatedBy: string;
}

export interface ProductSupplierLink {
  productId: string;
  productName: string;
  productStatus: SupplierStatus;
  linkStatus: SupplierStatus;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  updatedBy: string;
}

export interface PaginatedSupplierLinks {
  items: SupplierLink[];
  page: number;
  pageSize: number;
  total: number;
}

export interface PaginatedProductSupplierLinks {
  items: ProductSupplierLink[];
  page: number;
  pageSize: number;
  total: number;
}

export async function listSuppliers(
  client: SupabaseClient,
  filters: { search: string; status: string; page: number; pageSize: number },
): Promise<PaginatedSuppliers> {
  const query = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  });
  if (filters.search) query.set("search", filters.search);
  if (filters.status) query.set("status", filters.status);
  return requestWithSession<PaginatedSuppliers>(client, "GET", `/api/suppliers?${query.toString()}`);
}

export function getSupplier(client: SupabaseClient, supplierId: string): Promise<Supplier> {
  return requestWithSession<Supplier>(client, "GET", `/api/suppliers/${supplierId}`);
}

export function createSupplier(client: SupabaseClient, input: SupplierInput): Promise<Supplier> {
  return requestWithSession<Supplier>(client, "POST", "/api/suppliers", input);
}

export function updateSupplier(
  client: SupabaseClient,
  supplierId: string,
  input: SupplierInput,
): Promise<Supplier> {
  return requestWithSession<Supplier>(client, "PATCH", `/api/suppliers/${supplierId}`, input);
}

export function setSupplierStatus(
  client: SupabaseClient,
  supplierId: string,
  status: SupplierStatus,
): Promise<void> {
  return requestWithSession<void>(client, "PUT", `/api/suppliers/${supplierId}/status`, { status });
}

export function listProductsForSupplier(
  client: SupabaseClient,
  supplierId: string,
  filters: { status?: SupplierStatus; linkStatus?: SupplierStatus; page: number; pageSize: number },
): Promise<PaginatedProductSupplierLinks> {
  const query = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  });
  if (filters.status) query.set("status", filters.status);
  if (filters.linkStatus) query.set("linkStatus", filters.linkStatus);
  return requestWithSession<PaginatedProductSupplierLinks>(
    client,
    "GET",
    `/api/suppliers/${supplierId}/products?${query.toString()}`,
  );
}

export function listSuppliersForProduct(
  client: SupabaseClient,
  productId: string,
  filters: { linkStatus?: SupplierStatus; page: number; pageSize: number },
): Promise<PaginatedSupplierLinks> {
  const query = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  });
  if (filters.linkStatus) query.set("linkStatus", filters.linkStatus);
  return requestWithSession<PaginatedSupplierLinks>(
    client,
    "GET",
    `/api/products/${productId}/suppliers?${query.toString()}`,
  );
}

export function associateSupplier(
  client: SupabaseClient,
  productId: string,
  supplierId: string,
): Promise<void> {
  return requestWithSession<void>(
    client,
    "PUT",
    `/api/products/${productId}/suppliers/${supplierId}`,
  );
}

export function disassociateSupplier(
  client: SupabaseClient,
  productId: string,
  supplierId: string,
): Promise<void> {
  return requestWithSession<void>(
    client,
    "DELETE",
    `/api/products/${productId}/suppliers/${supplierId}`,
  );
}
