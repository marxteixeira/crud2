"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useParams, useSearchParams } from "next/navigation";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import ProductList from "@/components/products/ProductList";
import { listProductsAtShelf, type ProductStatus } from "@/lib/products";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";

const pageSize = 25;

export default function ShelfProductsPage(): JSX.Element {
  const { rackId, shelfId } = useParams<{ rackId: string; shelfId: string }>();
  const searchParams = useSearchParams();
  const page = Math.max(1, Number(searchParams.get("page") ?? "1") || 1);
  const status = searchParams.get("status") ?? "";
  const productsQuery = useQuery({
    queryKey: ["products", "shelf", rackId, shelfId, status, page],
    queryFn: () => listProductsAtShelf(getSupabaseBrowserClient(), rackId, shelfId, {
      page, pageSize, status: status as ProductStatus | undefined,
    }),
  });
  const totalPages = Math.max(1, Math.ceil((productsQuery.data?.total ?? 0) / pageSize));

  function changeFilters(nextStatus: string, nextPage: number): void {
    const query = new URLSearchParams();
    if (nextStatus) query.set("status", nextStatus);
    if (nextPage > 1) query.set("page", String(nextPage));
    window.history.replaceState(null, "", `?${query.toString()}`);
  }

  return (
    <>
      <AuthenticatedNavigation />
      <main className="mx-auto max-w-5xl space-y-5 p-6">
        <Link href="/locations" className="underline">Voltar às localizações</Link>
        <h1 className="text-2xl font-bold">Produtos na prateleira</h1>
        <div>
          <label htmlFor="shelf-product-status" className="mb-1 block text-sm font-medium">Situação</label>
          <select id="shelf-product-status" value={status}
            onChange={(event) => changeFilters(event.target.value, 1)} className="rounded border p-2">
            <option value="">Todas</option>
            <option value="Active">Ativos</option>
            <option value="Inactive">Inativos</option>
          </select>
        </div>
        {productsQuery.isLoading && <p role="status">Carregando produtos...</p>}
        {productsQuery.isError && <p role="alert">Não foi possível carregar os produtos desta prateleira.</p>}
        {productsQuery.data && (
          <>
            <ProductList products={productsQuery.data.items} />
            <div className="flex items-center justify-between">
              <p>{productsQuery.data.total} produto(s)</p>
              <div className="flex items-center gap-3">
                <button type="button" disabled={page <= 1} onClick={() => changeFilters(status, page - 1)}
                  className="rounded border px-3 py-1 disabled:opacity-50">Anterior</button>
                <span aria-live="polite">Página {page} de {totalPages}</span>
                <button type="button" disabled={page >= totalPages} onClick={() => changeFilters(status, page + 1)}
                  className="rounded border px-3 py-1 disabled:opacity-50">Próxima</button>
              </div>
            </div>
          </>
        )}
      </main>
    </>
  );
}
