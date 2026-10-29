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
      <main className="mx-auto max-w-7xl space-y-space-lg p-space-md text-on-surface md:p-space-xl">
        <Link href="/locations" className="text-label-md text-primary underline">Voltar às localizações</Link>
        <h1 className="font-headline text-headline-lg-mobile md:text-headline-lg">Produtos na prateleira</h1>
        <div>
          <label htmlFor="shelf-product-status" className="mb-space-xs block text-label-sm text-on-surface-variant">Situação</label>
          <select id="shelf-product-status" value={status}
            onChange={(event) => changeFilters(event.target.value, 1)} className="rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md">
            <option value="">Todas</option>
            <option value="Active">Ativos</option>
            <option value="Inactive">Inativos</option>
          </select>
        </div>
        {productsQuery.isLoading && <p role="status" className="rounded-xl bg-surface-container-low p-space-lg text-body-md text-on-surface-variant">Carregando produtos...</p>}
        {productsQuery.isError && <p role="alert" className="rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">Não foi possível carregar os produtos desta prateleira.</p>}
        {productsQuery.data && (
          <>
            <ProductList products={productsQuery.data.items} />
            <div className="flex flex-wrap items-center justify-between gap-space-md">
              <p>{productsQuery.data.total} produto(s)</p>
              <div className="flex items-center gap-space-sm">
                <button type="button" disabled={page <= 1} onClick={() => changeFilters(status, page - 1)}
                  className="rounded-lg px-space-md py-1.5 text-label-md ring-1 ring-outline-variant hover:bg-surface-container-low disabled:opacity-50">Anterior</button>
                <span aria-live="polite">Página {page} de {totalPages}</span>
                <button type="button" disabled={page >= totalPages} onClick={() => changeFilters(status, page + 1)}
                  className="rounded-lg px-space-md py-1.5 text-label-md ring-1 ring-outline-variant hover:bg-surface-container-low disabled:opacity-50">Próxima</button>
              </div>
            </div>
          </>
        )}
      </main>
    </>
  );
}
