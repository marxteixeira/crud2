"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import ProductList from "@/components/products/ProductList";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { listRacks } from "@/lib/locations";
import { listProducts, type ProductStatus } from "@/lib/products";
import { listSuppliers } from "@/lib/suppliers";

const pageSize = 25;

export default function ProductsPageClient(): JSX.Element {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [searchDraft, setSearchDraft] = useState(searchParams.get("search") ?? "");
  const search = searchParams.get("search") ?? "";
  const rackId = searchParams.get("rackId") ?? "";
  const shelfId = searchParams.get("shelfId") ?? "";
  const status = searchParams.get("status") ?? "";
  const supplierId = searchParams.get("supplierId") ?? "";
  const page = Math.max(1, Number(searchParams.get("page") ?? "1") || 1);
  useEffect(() => setSearchDraft(search), [search]);

  const racksQuery = useQuery({
    queryKey: ["locations", "racks"],
    queryFn: () => listRacks(getSupabaseBrowserClient()),
  });
  const productsQuery = useQuery({
    queryKey: ["products", search, rackId, shelfId, status, supplierId, page],
    queryFn: () => listProducts(getSupabaseBrowserClient(), {
      search, rackId, shelfId, status: status as ProductStatus | undefined, supplierId, page, pageSize,
    }),
  });
  const suppliersQuery = useQuery({
    queryKey: ["suppliers", "catalog-filter"],
    queryFn: () => listSuppliers(getSupabaseBrowserClient(), { search: "", status: "", page: 1, pageSize: 100 }),
  });

  function updateQuery(values: Record<string, string>): void {
    const params = new URLSearchParams(searchParams.toString());
    for (const [key, value] of Object.entries(values)) {
      if (value) params.set(key, value);
      else params.delete(key);
    }
    if (!("page" in values)) params.set("page", "1");
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  }

  function submitSearch(event: FormEvent<HTMLFormElement>): void {
    event.preventDefault();
    updateQuery({ search: searchDraft.trim() });
  }

  const selectedRack = racksQuery.data?.find((rack) => rack.id === rackId);
  const totalPages = Math.max(1, Math.ceil((productsQuery.data?.total ?? 0) / pageSize));

  return (
    <>
      <AuthenticatedNavigation />
      <main className="mx-auto max-w-7xl space-y-space-xl p-space-md text-on-surface md:p-space-xl">
        <header className="flex flex-wrap items-center justify-between gap-space-md">
          <div>
            <h1 className="font-headline text-headline-lg-mobile md:text-headline-lg">Catálogo de produtos</h1>
            <p className="mt-1 text-body-md text-on-surface-variant">Pesquise e consulte o estoque por localização.</p>
          </div>
          <Link href="/products/new" className="rounded-lg bg-primary-container px-space-md py-2.5 text-label-md text-on-primary hover:bg-primary">Cadastrar novo produto</Link>
        </header>
        <section aria-label="Filtros de produtos" className="flex flex-wrap items-end gap-space-md rounded-xl bg-surface-container-lowest p-space-md shadow-sm ring-1 ring-outline-variant/50">
          <form onSubmit={submitSearch} className="flex flex-wrap items-end gap-space-sm">
            <div>
              <label htmlFor="product-search" className="mb-space-xs block text-label-sm text-on-surface-variant">Buscar produto</label>
              <input id="product-search" value={searchDraft} onChange={(event) => setSearchDraft(event.target.value)}
                className="rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md" />
            </div>
            <button type="submit" className="rounded-lg bg-primary-container px-space-md py-2.5 text-label-md text-on-primary">Pesquisar</button>
          </form>
          <div>
            <label htmlFor="product-rack-filter" className="mb-space-xs block text-label-sm text-on-surface-variant">Estante</label>
            <select id="product-rack-filter" value={rackId}
              onChange={(event) => updateQuery({ rackId: event.target.value, shelfId: "" })}
              className="rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md">
              <option value="">Todas</option>
              {racksQuery.data?.map((rack) => <option key={rack.id} value={rack.id}>{rack.code}</option>)}
            </select>
          </div>
          <div>
            <label htmlFor="product-shelf-filter" className="mb-space-xs block text-label-sm text-on-surface-variant">Prateleira</label>
            <select id="product-shelf-filter" value={shelfId}
              onChange={(event) => updateQuery({ shelfId: event.target.value })}
              disabled={!selectedRack} className="rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md disabled:opacity-50">
              <option value="">Todas</option>
              {selectedRack?.shelves.map((shelf) => <option key={shelf.id} value={shelf.id}>{shelf.code}</option>)}
            </select>
          </div>
          <div>
            <label htmlFor="product-status-filter" className="mb-space-xs block text-label-sm text-on-surface-variant">Situação</label>
            <select id="product-status-filter" value={status}
              onChange={(event) => updateQuery({ status: event.target.value })} className="rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md">
              <option value="">Todas</option>
              <option value="Active">Ativos</option>
              <option value="Inactive">Inativos</option>
            </select>
          </div>
          <div>
            <label htmlFor="product-supplier-filter" className="mb-space-xs block text-label-sm text-on-surface-variant">Fornecedor</label>
            <select id="product-supplier-filter" value={supplierId}
              onChange={(event) => updateQuery({ supplierId: event.target.value })} className="rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md">
              <option value="">Todos</option>
              {suppliersQuery.data?.items.map((supplier) => (
                <option key={supplier.id} value={supplier.id}>
                  {supplier.name}{supplier.status === "Inactive" ? " (inativo)" : ""}
                </option>
              ))}
            </select>
          </div>
        <button type="button" onClick={() => { setSearchDraft(""); updateQuery({ search: "", rackId: "", shelfId: "", status: "", supplierId: "", page: "" }); }} className="rounded-lg bg-secondary-container px-space-md py-2.5 text-label-md text-on-secondary-fixed">Limpar</button>
        </section>
        {racksQuery.isError && <p role="alert">Não foi possível carregar as localizações.</p>}
        {suppliersQuery.isError && <p role="alert">Não foi possível carregar os fornecedores para o filtro.</p>}
        {productsQuery.isLoading && <p role="status">Carregando produtos...</p>}
        {productsQuery.isError && <p role="alert">Não foi possível carregar o catálogo.</p>}
        {productsQuery.data && (
          <>
            <ProductList products={productsQuery.data.items} />
            <div className="flex items-center justify-between">
              <p className="text-body-sm text-on-surface-variant">{productsQuery.data.total} produto(s)</p>
              <div className="flex items-center gap-3">
                <button type="button" disabled={page <= 1} onClick={() => updateQuery({ page: String(page - 1) })}
                  className="rounded-lg px-space-md py-1.5 text-label-md ring-1 ring-outline-variant hover:bg-surface-container-low disabled:opacity-50">Anterior</button>
                <span aria-live="polite">Página {page} de {totalPages}</span>
                <button type="button" disabled={page >= totalPages} onClick={() => updateQuery({ page: String(page + 1) })}
                  className="rounded-lg px-space-md py-1.5 text-label-md ring-1 ring-outline-variant hover:bg-surface-container-low disabled:opacity-50">Próxima</button>
              </div>
            </div>
          </>
        )}
      </main>
    </>
  );
}
