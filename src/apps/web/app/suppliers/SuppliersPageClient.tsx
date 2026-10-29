"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import Button from "@/components/ui/Button";
import DataTable, { DataCell, DataRow } from "@/components/ui/DataTable";
import { InputField, SelectField } from "@/components/ui/Field";
import PageHeader from "@/components/ui/PageHeader";
import { EmptyState, ErrorState, LoadingState } from "@/components/ui/StateMessages";
import StatusBadge from "@/components/ui/StatusBadge";
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
      <main className="min-h-screen bg-surface px-space-md py-space-xl text-on-surface md:ml-64 md:px-space-xl">
        <div className="mx-auto max-w-7xl">
          <PageHeader
            title="Fornecedores"
            description="Consulte e mantenha os fornecedores dos produtos."
            action={accountQuery.data?.role === "Administrador" ? (
              <Link href="/suppliers/new" className="inline-flex items-center justify-center rounded-lg bg-primary-container px-space-md py-2 text-label-md text-on-primary transition-colors hover:bg-primary">Cadastrar fornecedor</Link>
            ) : undefined}
          />
          <section aria-label="Filtros de fornecedores" className="mb-space-md rounded-xl bg-surface-container-lowest p-space-md shadow-sm ring-1 ring-outline-variant/50">
            <div className="flex flex-wrap items-end gap-space-md">
              <form onSubmit={submitSearch} className="flex min-w-0 flex-1 flex-wrap items-end gap-space-sm">
                <InputField
                  label="Buscar fornecedor"
                  value={searchDraft}
                  onChange={(event) => setSearchDraft(event.target.value)}
                  className="min-w-64"
                />
                <Button type="submit" variant="secondary">Pesquisar</Button>
              </form>
              <SelectField
                label="Situação"
                value={status}
                onChange={(event) => updateQuery({ status: event.target.value })}
                className="min-w-36"
              >
                <option value="">Todas</option>
                <option value="Active">Ativos</option>
                <option value="Inactive">Inativos</option>
              </SelectField>
            </div>
          </section>
          {accountQuery.isError && <ErrorState message="Não foi possível carregar as permissões da conta." />}
          {suppliersQuery.isLoading && <LoadingState message="Carregando fornecedores..." />}
          {suppliersQuery.isError && <ErrorState message="Não foi possível carregar os fornecedores." />}
          {suppliersQuery.data && (
            <>
              {suppliersQuery.data.items.length === 0 ? (
                <EmptyState message="Nenhum fornecedor encontrado." />
              ) : (
                <DataTable
                  caption="Fornecedores cadastrados"
                  headers={["Fornecedor / Razão Social", "Documento", "Contato & Canal", "Itens Associados", "Situação", "Ações"]}
                >
                  {suppliersQuery.data.items.map((supplier) => (
                    <DataRow key={supplier.id}>
                      <DataCell>
                        <Link href={`/suppliers/${supplier.id}`} className="font-title text-title-md text-primary hover:underline">
                          {supplier.name}
                        </Link>
                      </DataCell>
                      <DataCell className="whitespace-nowrap">{supplier.taxIdentifier}</DataCell>
                      <DataCell>
                        <div className="space-y-1 text-body-sm">
                          {supplier.contactName && <p>{supplier.contactName}</p>}
                          {supplier.email && <p>{supplier.email}</p>}
                          {supplier.phone && <p>{supplier.phone}</p>}
                          {!supplier.contactName && !supplier.email && !supplier.phone && (
                            <span className="text-on-surface-variant">Não informado</span>
                          )}
                        </div>
                      </DataCell>
                      <DataCell>
                        <span className="text-on-surface-variant" title="Consulte os detalhes para ver os produtos associados.">—</span>
                      </DataCell>
                      <DataCell><StatusBadge active={supplier.status === "Active"} /></DataCell>
                      <DataCell>
                        <Link href={`/suppliers/${supplier.id}`} className="text-label-md text-primary hover:underline">
                          Ver detalhes
                        </Link>
                      </DataCell>
                    </DataRow>
                  ))}
                </DataTable>
              )}
              <div className="mt-space-md flex flex-wrap items-center justify-between gap-space-md text-body-sm text-on-surface-variant">
                <p>{suppliersQuery.data.total} fornecedor(es)</p>
                <div className="flex items-center gap-space-sm">
                  <Button type="button" variant="secondary" disabled={page <= 1}
                    onClick={() => updateQuery({ page: String(page - 1) })}>Anterior</Button>
                  <span aria-live="polite">Página {page} de {totalPages}</span>
                  <Button type="button" variant="secondary" disabled={page >= totalPages}
                    onClick={() => updateQuery({ page: String(page + 1) })}>Próxima</Button>
                </div>
              </div>
            </>
          )}
        </div>
      </main>
    </>
  );
}
