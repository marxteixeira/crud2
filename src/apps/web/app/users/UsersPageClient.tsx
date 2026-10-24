"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import UserForm from "@/components/users/UserForm";
import UserTable from "@/components/users/UserTable";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { inviteUser, listUsers, setUserStatus, type UserAccount, type UserStatus } from "@/lib/users";

const pageSize = 25;

export default function UsersPageClient(): JSX.Element {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const queryClient = useQueryClient();
  const [searchDraft, setSearchDraft] = useState(searchParams.get("search") ?? "");
  const [mutationError, setMutationError] = useState<string | null>(null);
  const search = searchParams.get("search") ?? "";
  const status = searchParams.get("status") ?? "";
  const page = Math.max(1, Number(searchParams.get("page") ?? "1") || 1);
  useEffect(() => setSearchDraft(search), [search]);

  const usersQuery = useQuery({
    queryKey: ["users", search, status, page],
    queryFn: () => listUsers(getSupabaseBrowserClient(), { search, status, page, pageSize }),
  });
  const inviteMutation = useMutation({
    mutationFn: (input: Parameters<typeof inviteUser>[1]) => inviteUser(getSupabaseBrowserClient(), input),
    onSuccess: async () => {
      setMutationError(null);
      await queryClient.invalidateQueries({ queryKey: ["users"] });
      await queryClient.invalidateQueries({ queryKey: ["current-account"] });
    },
    onError: () => setMutationError("Não foi possível enviar o convite. Revise os dados ou tente novamente."),
  });
  const statusMutation = useMutation({
    mutationFn: ({ account, nextStatus }: { account: UserAccount; nextStatus: Exclude<UserStatus, "ConvitePendente"> }) =>
      setUserStatus(getSupabaseBrowserClient(), account.id, nextStatus),
    onSuccess: async () => {
      setMutationError(null);
      await queryClient.invalidateQueries({ queryKey: ["users"] });
      await queryClient.invalidateQueries({ queryKey: ["current-account"] });
    },
    onError: () => setMutationError("Não foi possível alterar a situação da conta."),
  });

  function updateQuery(values: Record<string, string>): void {
    const params = new URLSearchParams(searchParams.toString());
    for (const [key, value] of Object.entries(values)) {
      if (value) params.set(key, value);
      else params.delete(key);
    }
    if (!("page" in values)) {
      params.set("page", "1");
    }
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  }

  function submitSearch(event: FormEvent<HTMLFormElement>): void {
    event.preventDefault();
    updateQuery({ search: searchDraft.trim() });
  }

  const totalPages = Math.max(1, Math.ceil((usersQuery.data?.total ?? 0) / pageSize));

  return (
    <>
      <AuthenticatedNavigation />
      <main className="mx-auto max-w-6xl space-y-6 p-6">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold">Gerenciar usuários</h1>
            <p className="text-sm text-slate-600">Convide pessoas e gerencie seus perfis de acesso.</p>
          </div>
          <details className="rounded border p-4">
            <summary className="cursor-pointer font-medium">Convidar usuário</summary>
            <div className="mt-4 w-80 max-w-full">
              <UserForm
                submitting={inviteMutation.isPending}
                errorMessage={mutationError}
                onSubmit={async (input) => {
                  await inviteMutation.mutateAsync(input);
                }}
              />
            </div>
          </details>
        </div>

        <section aria-label="Filtros de usuários" className="flex flex-wrap items-end gap-3">
          <form onSubmit={submitSearch} className="flex flex-wrap items-end gap-2">
            <div>
              <label htmlFor="user-search" className="mb-1 block text-sm font-medium">Buscar</label>
              <input id="user-search" value={searchDraft} onChange={(event) => setSearchDraft(event.target.value)}
                className="rounded border p-2" />
            </div>
            <button type="submit" className="rounded border px-4 py-2">Pesquisar</button>
          </form>
          <div>
            <label htmlFor="user-status" className="mb-1 block text-sm font-medium">Situação</label>
            <select id="user-status" value={status} onChange={(event) => updateQuery({ status: event.target.value })}
              className="rounded border p-2">
              <option value="">Todas</option>
              <option value="ConvitePendente">Convite pendente</option>
              <option value="Ativa">Ativa</option>
              <option value="Inativa">Inativa</option>
            </select>
          </div>
        </section>

        {mutationError && <p role="alert" className="text-sm text-red-700">{mutationError}</p>}
        {usersQuery.isLoading && <p role="status">Carregando contas...</p>}
        {usersQuery.isError && <p role="alert" className="text-sm text-red-700">Não foi possível carregar as contas.</p>}
        {usersQuery.data && (
          <>
            <UserTable
              accounts={usersQuery.data.items}
              changingId={statusMutation.isPending ? statusMutation.variables?.account.id : undefined}
              onStatusChange={(account, nextStatus) => {
                setMutationError(null);
                statusMutation.mutate({ account, nextStatus });
              }}
            />
            <div className="flex items-center justify-between">
              <p className="text-sm">{usersQuery.data.total} conta(s)</p>
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
