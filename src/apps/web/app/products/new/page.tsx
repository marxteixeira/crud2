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
      <main className="mx-auto max-w-2xl space-y-5 p-6">
        <h1 className="text-2xl font-bold">Cadastrar produto</h1>
        {(accountQuery.isLoading || racksQuery.isLoading) && <p role="status">Carregando formulário...</p>}
        {(accountQuery.isError || racksQuery.isError) && <p role="alert">Não foi possível carregar os dados do formulário.</p>}
        {accountQuery.data && racksQuery.data && (
          <section className="rounded border p-5">
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
