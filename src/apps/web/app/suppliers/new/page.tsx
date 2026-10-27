"use client";

import { useMutation, useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import SupplierForm from "@/components/suppliers/SupplierForm";
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
      <main className="mx-auto max-w-2xl space-y-5 p-6">
        <Link href="/suppliers" className="underline">Voltar aos fornecedores</Link>
        <h1 className="text-2xl font-bold">Cadastrar fornecedor</h1>
        {accountQuery.isLoading && <p role="status">Carregando permissões da conta...</p>}
        {accountQuery.isError && <p role="alert">Não foi possível carregar as permissões da conta.</p>}
        {accountQuery.data?.role === "Administrador"
          ? <SupplierForm submitting={mutation.isPending} errorMessage={errorMessage}
            onSubmit={async (input) => {
              setErrorMessage(null);
              mutation.mutate(input);
            }} />
          : accountQuery.data && <p role="alert">Somente Administradores podem cadastrar fornecedores.</p>}
      </main>
    </>
  );
}
