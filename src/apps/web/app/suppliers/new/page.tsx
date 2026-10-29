"use client";

import { useMutation, useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import SupplierForm from "@/components/suppliers/SupplierForm";
import PageHeader from "@/components/ui/PageHeader";
import { ErrorState, LoadingState } from "@/components/ui/StateMessages";
import { getCurrentAccount } from "@/lib/auth";
import { createSupplier } from "@/lib/suppliers";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";

export default function NewSupplierPage(): JSX.Element {
  const router = useRouter();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const client = getSupabaseBrowserClient();
  const accountQuery = useQuery({
    queryKey: ["current-account"],
    queryFn: () => getCurrentAccount(client),
  });
  const mutation = useMutation({
    mutationFn: (input: Parameters<typeof createSupplier>[1]) => createSupplier(client, input),
    onSuccess: (supplier) => router.replace(`/suppliers/${supplier.id}`),
    onError: () => setErrorMessage("Não foi possível cadastrar o fornecedor. Verifique os dados e tente novamente."),
  });

  return (
    <>
      <AuthenticatedNavigation />
      <main className="min-h-screen bg-surface px-space-md py-space-xl text-on-surface md:ml-64 md:px-space-xl">
        <div className="mx-auto max-w-3xl">
          <Link href="/suppliers" className="mb-space-md inline-flex text-label-md text-primary hover:underline">
            Voltar aos fornecedores
          </Link>
          <section className="rounded-xl bg-surface-container-lowest p-space-lg shadow-sm ring-1 ring-outline-variant/50 md:p-space-xl">
            <PageHeader title="Cadastrar fornecedor" description="Insira as informações cadastrais e fiscais da empresa parceira." />
            {accountQuery.isLoading && <LoadingState message="Carregando permissões da conta..." />}
            {accountQuery.isError && <ErrorState message="Não foi possível carregar as permissões da conta." />}
            {accountQuery.data?.role === "Administrador"
              ? <SupplierForm submitting={mutation.isPending} errorMessage={errorMessage}
                onSubmit={async (input) => {
                  setErrorMessage(null);
                  mutation.mutate(input);
                }} />
              : accountQuery.data && <ErrorState message="Somente Administradores podem cadastrar fornecedores." />}
          </section>
        </div>
      </main>
    </>
  );
}
