"use client";

import { useMutation, useQuery } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useState } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import ProductForm from "@/components/products/ProductForm";
import { getCurrentAccount } from "@/lib/auth";
import { listRacks } from "@/lib/locations";
import { createProduct } from "@/lib/products";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";

export default function NewProductPage(): JSX.Element {
  const router = useRouter();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const accountQuery = useQuery({
    queryKey: ["current-account"],
    queryFn: () => getCurrentAccount(getSupabaseBrowserClient()),
  });
  const racksQuery = useQuery({
    queryKey: ["locations", "racks"],
    queryFn: () => listRacks(getSupabaseBrowserClient()),
  });
  const createMutation = useMutation({
    mutationFn: (input: Parameters<typeof createProduct>[1]) =>
      createProduct(getSupabaseBrowserClient(), input),
    onSuccess: (product) => router.push(`/products/${product.id}`),
    onError: () => setErrorMessage("Não foi possível cadastrar o produto. Revise os dados e tente novamente."),
  });

  return (
    <>
      <AuthenticatedNavigation />
      <main className="mx-auto max-w-3xl space-y-space-lg p-space-md text-on-surface md:p-space-xl">
        <header><p className="text-label-sm uppercase tracking-wider text-on-surface-variant">Catálogo de produtos</p><h1 className="font-headline text-headline-lg-mobile md:text-headline-lg">Cadastrar produto</h1><p className="mt-1 text-body-md text-on-surface-variant">Preencha os dados cadastrais, alocação física e parâmetros de precificação.</p></header>
        {(accountQuery.isLoading || racksQuery.isLoading) && <p role="status">Carregando formulário...</p>}
        {(accountQuery.isError || racksQuery.isError) && <p role="alert">Não foi possível carregar os dados do formulário.</p>}
        {accountQuery.data && racksQuery.data && (
          <section className="rounded-xl bg-surface-container-lowest p-space-lg shadow-sm ring-1 ring-outline-variant/50">
            <ProductForm
              racks={racksQuery.data}
              isAdministrator={accountQuery.data.role === "Administrador"}
              submitting={createMutation.isPending}
              errorMessage={errorMessage}
              onSubmit={async (input) => {
                setErrorMessage(null);
                await createMutation.mutateAsync(input);
              }}
            />
          </section>
        )}
      </main>
    </>
  );
}
