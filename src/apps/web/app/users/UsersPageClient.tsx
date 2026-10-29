"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import UserForm from "@/components/users/UserForm";
import UserTable from "@/components/users/UserTable";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { inviteUser, listUsers, resendInvitation, setUserStatus, type UserAccount, type UserStatus } from "@/lib/users";
import Button from "@/components/ui/Button";
import Dialog from "@/components/ui/Dialog";
import { InputField, SelectField } from "@/components/ui/Field";
import PageHeader from "@/components/ui/PageHeader";
import { ErrorState, LoadingState, SuccessState } from "@/components/ui/StateMessages";

const pageSize = 25;

export default function UsersPageClient(): JSX.Element {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const queryClient = useQueryClient();
  const [searchDraft, setSearchDraft] = useState(searchParams.get("search") ?? "");
  const [mutationError, setMutationError] = useState<string | null>(null);
  const [inviteOpen, setInviteOpen] = useState(false);
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
  const [notice, setNotice] = useState<string | null>(null);
  const resendMutation = useMutation({
    mutationFn: (account: UserAccount) => resendInvitation(getSupabaseBrowserClient(), account.id),
    onSuccess: () => {
      setMutationError(null);
      setNotice("Convite reenviado. Os convites anteriores deixaram de valer.");
    },
    onError: () => {
      setNotice(null);
      setMutationError("Não foi possível reenviar o convite. Tente novamente.");
    },
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
    if (!("page" in values)) params.set("page", "1");
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
      <main className="min-h-screen bg-surface px-space-md py-space-xl text-on-surface md:ml-64 md:px-space-xl lg:px-12">
        <div className="mx-auto max-w-7xl">
          <PageHeader
            title="Gerenciar usuários"
            description="Convide pessoas e gerencie seus perfis de acesso."
            action={<Button onClick={() => { setMutationError(null); setInviteOpen(true); }}>Convidar usuário</Button>}
          />
          <section aria-label="Filtros de usuários" className="mb-space-lg flex flex-wrap items-end gap-space-md rounded-xl bg-surface-container-lowest p-space-md shadow-sm ring-1 ring-outline-variant/40 sm:p-space-lg">
            <form onSubmit={submitSearch} className="flex min-w-[min(100%,18rem)] flex-1 items-end gap-space-sm">
              <InputField label="Buscar" value={searchDraft} onChange={(event) => setSearchDraft(event.target.value)} />
              <Button type="submit" variant="secondary">Pesquisar</Button>
            </form>
            <div className="w-full sm:w-56">
              <SelectField label="Situação" value={status} onChange={(event) => updateQuery({ status: event.target.value })}>
                <option value="">Todas</option>
                <option value="ConvitePendente">Convite pendente</option>
                <option value="Ativa">Ativa</option>
                <option value="Inativa">Inativa</option>
              </SelectField>
            </div>
          </section>

          {mutationError && <div className="mb-space-md"><ErrorState message={mutationError} /></div>}
          {notice && <div className="mb-space-md"><SuccessState message={notice} /></div>}
          {usersQuery.isLoading && <LoadingState message="Carregando contas..." />}
          {usersQuery.isError && <ErrorState message="Não foi possível carregar as contas." />}
          {usersQuery.data && (
            <>
              <UserTable
                accounts={usersQuery.data.items}
                changingId={statusMutation.isPending ? statusMutation.variables?.account.id : undefined}
                resendingId={resendMutation.isPending ? resendMutation.variables?.id : undefined}
                onResendInvitation={(account) => {
                  setMutationError(null);
                  setNotice(null);
                  resendMutation.mutate(account);
                }}
                onStatusChange={(account, nextStatus) => {
                  setMutationError(null);
                  statusMutation.mutate({ account, nextStatus });
                }}
              />
              <div className="mt-space-md flex flex-wrap items-center justify-between gap-space-md rounded-xl bg-surface-container-lowest px-space-lg py-space-md text-body-sm text-on-surface-variant shadow-sm ring-1 ring-outline-variant/40">
                <p><span className="font-title text-on-surface">{usersQuery.data.total}</span> conta(s)</p>
                <div className="flex items-center gap-space-md">
                  <Button variant="secondary" disabled={page <= 1} onClick={() => updateQuery({ page: String(page - 1) })}>Anterior</Button>
                  <span aria-live="polite">Página {page} de {totalPages}</span>
                  <Button variant="secondary" disabled={page >= totalPages} onClick={() => updateQuery({ page: String(page + 1) })}>Próxima</Button>
                </div>
              </div>
            </>
          )}
        </div>
      </main>
      <Dialog open={inviteOpen} title="Convidar novo usuário" onClose={() => setInviteOpen(false)}>
        <p className="mb-space-lg text-body-md text-on-surface-variant">Informe os dados da pessoa e selecione o perfil de acesso.</p>
        <UserForm
          submitting={inviteMutation.isPending}
          errorMessage={mutationError}
          onSubmit={async (input) => {
            await inviteMutation.mutateAsync(input);
            setInviteOpen(false);
          }}
        />
        <div className="mt-space-md flex justify-end">
          <Button variant="secondary" onClick={() => setInviteOpen(false)}>Cancelar</Button>
        </div>
      </Dialog>
    </>
  );
}
