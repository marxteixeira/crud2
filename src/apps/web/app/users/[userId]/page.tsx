"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import UserForm from "@/components/users/UserForm";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { getUser, updateUser } from "@/lib/users";

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
      <main className="mx-auto max-w-2xl space-y-5 p-6">
        <Link href="/users" className="underline">Voltar para usuários</Link>
        <h1 className="text-2xl font-bold">Editar usuário</h1>
        {accountQuery.isLoading && <p role="status">Carregando conta...</p>}
        {accountQuery.isError && <p role="alert" className="text-sm text-red-700">Não foi possível carregar a conta.</p>}
        {accountQuery.data && (
          <section className="rounded border p-5">
            <p className="mb-4 text-sm">Situação: {accountQuery.data.status}</p>
            <UserForm
              account={accountQuery.data}
              submitting={updateMutation.isPending}
              errorMessage={errorMessage}
              onSubmit={async (input) => {
                await updateMutation.mutateAsync(input);
              }}
            />
          </section>
        )}
      </main>
    </>
  );
}
