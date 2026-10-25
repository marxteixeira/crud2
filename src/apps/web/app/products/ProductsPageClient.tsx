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
  const page = Math.max(1, Number(searchParams.get("page") ?? "1") || 1);
  useEffect(() => setSearchDraft(search), [search]);

  const racksQuery = useQuery({
    queryKey: ["locations", "racks"],
    queryFn: () => listRacks(getSupabaseBrowserClient()),
  });
  const productsQuery = useQuery({
    queryKey: ["products", search, rackId, shelfId, status, page],
    queryFn: () => listProducts(getSupabaseBrowserClient(), {
      search, rackId, shelfId, status: status as ProductStatus | undefined, page, pageSize,
    }),
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
      <main className="mx-auto max-w-6xl space-y-6 p-6">
        <header className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h1 className="text-2xl font-bold">Catálogo de produtos</h1>
            <p className="text-sm text-slate-600">Pesquise e consulte o estoque por localização.</p>
          </div>
          <Link href="/products/new" className="rounded bg-blue-700 px-4 py-2 text-white">Cadastrar produto</Link>
        </header>
        <section aria-label="Filtros de produtos" className="flex flex-wrap items-end gap-3">
          <form onSubmit={submitSearch} className="flex flex-wrap items-end gap-2">
            <div>
              <label htmlFor="product-search" className="mb-1 block text-sm font-medium">Buscar produto</label>
              <input id="product-search" value={searchDraft} onChange={(event) => setSearchDraft(event.target.value)}
                className="rounded border p-2" />
            </div>
            <button type="submit" className="rounded border px-4 py-2">Pesquisar</button>
          </form>
          <div>
            <label htmlFor="product-rack-filter" className="mb-1 block text-sm font-medium">Estante</label>
            <select id="product-rack-filter" value={rackId}
              onChange={(event) => updateQuery({ rackId: event.target.value, shelfId: "" })}
              className="rounded border p-2">
              <option value="">Todas</option>
              {racksQuery.data?.map((rack) => <option key={rack.id} value={rack.id}>{rack.code}</option>)}
            </select>
          </div>
          <div>
            <label htmlFor="product-shelf-filter" className="mb-1 block text-sm font-medium">Prateleira</label>
            <select id="product-shelf-filter" value={shelfId}
              onChange={(event) => updateQuery({ shelfId: event.target.value })}
              disabled={!selectedRack} className="rounded border p-2 disabled:opacity-50">
              <option value="">Todas</option>
              {selectedRack?.shelves.map((shelf) => <option key={shelf.id} value={shelf.id}>{shelf.code}</option>)}
            </select>
          </div>
          <div>
            <label htmlFor="product-status-filter" className="mb-1 block text-sm font-medium">Situação</label>
            <select id="product-status-filter" value={status}
              onChange={(event) => updateQuery({ status: event.target.value })} className="rounded border p-2">
              <option value="">Todas</option>
              <option value="Active">Ativos</option>
              <option value="Inactive">Inativos</option>
            </select>
          </div>
        </section>
        {racksQuery.isError && <p role="alert">Não foi possível carregar as localizações.</p>}
        {productsQuery.isLoading && <p role="status">Carregando produtos...</p>}
        {productsQuery.isError && <p role="alert">Não foi possível carregar o catálogo.</p>}
        {productsQuery.data && (
          <>
            <ProductList products={productsQuery.data.items} />
            <div className="flex items-center justify-between">
              <p className="text-sm">{productsQuery.data.total} produto(s)</p>
              <div className="flex items-center gap-3">
                <button type="button" disabled={page <= 1} onClick={() => updateQuery({ page: String(page - 1) })}
                  className="rounded border px-3 py-1 disabled:opacity-50">Anterior</button>
                <span aria-live="polite">Página {page} de {totalPages}</span>
                <button type="button" disabled={page >= totalPages} onClick={() => updateQuery({ page: String(page + 1) })}
                  className="rounded border px-3 py-1 disabled:opacity-50">Próxima</button>
              </div>
            </div>
          </>
        )}
      </main>
    </>
  );
}
