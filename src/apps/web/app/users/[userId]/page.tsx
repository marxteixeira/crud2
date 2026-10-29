"use client";

import Link from "next/link";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import UserForm from "@/components/users/UserForm";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { getUser, updateUser } from "@/lib/users";
import PageHeader from "@/components/ui/PageHeader";
import { ErrorState, LoadingState } from "@/components/ui/StateMessages";

export default function UserDetailsPage(): JSX.Element {
  const { userId } = useParams<{ userId: string }>();
  const router = useRouter();
  const queryClient = useQueryClient();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const accountQuery = useQuery({
    queryKey: ["user", userId],
    queryFn: () => getUser(getSupabaseBrowserClient(), userId),
  });
  const updateMutation = useMutation({
    mutationFn: async (values: { email: string; displayName: string; role: "Administrador" | "Operador" }) => {
      const account = accountQuery.data;
      if (!account) throw new Error("A conta não está carregada.");
      return updateUser(getSupabaseBrowserClient(), userId, {
        email: values.email === account.email ? undefined : values.email,
        displayName: values.displayName === account.displayName ? undefined : values.displayName,
        role: values.role === account.role ? undefined : values.role,
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["users"] });
      await queryClient.invalidateQueries({ queryKey: ["user", userId] });
      await queryClient.invalidateQueries({ queryKey: ["current-account"] });
      router.push("/users");
    },
    onError: () => setErrorMessage("Não foi possível salvar as alterações. Verifique os dados e tente novamente."),
  });

  return (
    <>
      <AuthenticatedNavigation />
      <main className="min-h-screen bg-surface px-space-md py-space-xl text-on-surface md:ml-64 md:px-space-xl lg:px-12">
        <div className="mx-auto max-w-3xl">
          <Link href="/users" className="mb-space-md inline-flex text-label-md text-on-surface-variant underline-offset-4 hover:text-primary hover:underline">Voltar para usuários</Link>
          <PageHeader title="Editar usuário" description="Atualize os dados e o perfil de acesso da conta." />
          {accountQuery.isLoading && <LoadingState message="Carregando conta..." />}
          {accountQuery.isError && <ErrorState message="Não foi possível carregar a conta." />}
          {accountQuery.data && (
            <section className="rounded-xl bg-surface-container-lowest p-space-lg shadow-sm ring-1 ring-outline-variant/40 sm:p-space-xl" aria-label="Dados do usuário">
              <p className="mb-space-lg flex items-center gap-space-sm text-body-sm text-on-surface-variant">
                Situação:
                <span className="rounded-full bg-secondary-container px-space-md py-1 text-label-sm text-on-secondary-fixed">{accountQuery.data.status === "ConvitePendente" ? "Convite pendente" : accountQuery.data.status}</span>
              </p>
              {errorMessage && <div className="mb-space-md"><ErrorState message={errorMessage} /></div>}
              <UserForm
                account={accountQuery.data}
                submitting={updateMutation.isPending}
                onSubmit={async (input) => {
                  setErrorMessage(null);
                  await updateMutation.mutateAsync(input);
                }}
              />
            </section>
          )}
        </div>
      </main>
    </>
  );
}
