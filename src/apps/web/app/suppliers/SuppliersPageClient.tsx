"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import { getCurrentAccount } from "@/lib/auth";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { listSuppliers } from "@/lib/suppliers";

const pageSize = 25;

export default function SuppliersPageClient(): JSX.Element {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [searchDraft, setSearchDraft] = useState(searchParams.get("search") ?? "");
  const search = searchParams.get("search") ?? "";
  const status = searchParams.get("status") ?? "";
  const page = Math.max(1, Number(searchParams.get("page") ?? "1") || 1);
  useEffect(() => setSearchDraft(search), [search]);

  const client = getSupabaseBrowserClient();
  const accountQuery = useQuery({
    queryKey: ["current-account"],
    queryFn: () => getCurrentAccount(client),
  });
  const suppliersQuery = useQuery({
    queryKey: ["suppliers", search, status, page],
    queryFn: () => listSuppliers(client, { search, status, page, pageSize }),
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

  const totalPages = Math.max(1, Math.ceil((suppliersQuery.data?.total ?? 0) / pageSize));

  return (
    <>
      <AuthenticatedNavigation />
      <main className="mx-auto max-w-6xl space-y-6 p-6">
        <header className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold">Fornecedores</h1>
            <p className="text-sm text-slate-600">Consulte e mantenha os fornecedores dos produtos.</p>
          </div>
          {accountQuery.data?.role === "Administrador" && (
            <Link href="/suppliers/new" className="rounded bg-blue-700 px-4 py-2 text-white">
              Cadastrar fornecedor
            </Link>
          )}
        </header>
        <section aria-label="Filtros de fornecedores" className="flex flex-wrap items-end gap-3">
          <form onSubmit={submitSearch} className="flex flex-wrap items-end gap-2">
            <div>
              <label htmlFor="supplier-search" className="mb-1 block text-sm font-medium">Buscar fornecedor</label>
              <input id="supplier-search" value={searchDraft}
                onChange={(event) => setSearchDraft(event.target.value)} className="rounded border p-2" />
            </div>
            <button type="submit" className="rounded border px-4 py-2">Pesquisar</button>
          </form>
          <div>
            <label htmlFor="supplier-status" className="mb-1 block text-sm font-medium">Situação</label>
            <select id="supplier-status" value={status}
              onChange={(event) => updateQuery({ status: event.target.value })} className="rounded border p-2">
              <option value="">Todas</option>
              <option value="Active">Ativos</option>
              <option value="Inactive">Inativos</option>
            </select>
          </div>
        </section>
        {accountQuery.isError && <p role="alert">Não foi possível carregar as permissões da conta.</p>}
        {suppliersQuery.isLoading && <p role="status">Carregando fornecedores...</p>}
        {suppliersQuery.isError && <p role="alert">Não foi possível carregar os fornecedores.</p>}
        {suppliersQuery.data && (
          <>
            {suppliersQuery.data.items.length === 0
              ? <p>Nenhum fornecedor encontrado.</p>
              : <ul className="divide-y rounded border">
                {suppliersQuery.data.items.map((supplier) => (
                  <li key={supplier.id} className="flex flex-wrap justify-between gap-2 p-4">
                    <div>
                      <Link href={`/suppliers/${supplier.id}`} className="font-medium underline">
                        {supplier.name}
                      </Link>
                      <p className="text-sm text-slate-600">{supplier.taxIdentifier}</p>
                    </div>
                    <span>{supplier.status === "Active" ? "Ativo" : "Inativo"}</span>
                  </li>
                ))}
              </ul>}
            <div className="flex items-center justify-between">
              <p className="text-sm">{suppliersQuery.data.total} fornecedor(es)</p>
              <div className="flex items-center gap-3">
                <button type="button" disabled={page <= 1}
                  onClick={() => updateQuery({ page: String(page - 1) })}
                  className="rounded border px-3 py-1 disabled:opacity-50">Anterior</button>
                <span aria-live="polite">Página {page} de {totalPages}</span>
                <button type="button" disabled={page >= totalPages}
                  onClick={() => updateQuery({ page: String(page + 1) })}
                  className="rounded border px-3 py-1 disabled:opacity-50">Próxima</button>
              </div>
            </div>
          </>
        )}
      </main>
    </>
  );
}
