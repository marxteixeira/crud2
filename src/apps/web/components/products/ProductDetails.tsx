"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useState } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import ProductForm from "@/components/products/ProductForm";
import ProductSuppliers from "@/components/products/ProductSuppliers";
import { getCurrentAccount } from "@/lib/auth";
import { listRacks } from "@/lib/locations";
import { getProduct, setProductStatus, updateProduct, type ProductStatus } from "@/lib/products";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";

export default function ProductDetails(): JSX.Element {
  const { productId } = useParams<{ productId: string }>();
  const queryClient = useQueryClient();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const client = getSupabaseBrowserClient();
  const productQuery = useQuery({
    queryKey: ["product", productId],
    queryFn: () => getProduct(client, productId),
  });
  const accountQuery = useQuery({
    queryKey: ["current-account"],
    queryFn: () => getCurrentAccount(client),
  });
  const racksQuery = useQuery({
    queryKey: ["locations", "racks"],
    queryFn: () => listRacks(client),
  });
  const updateMutation = useMutation({
    mutationFn: (input: Parameters<typeof updateProduct>[2]) => updateProduct(client, productId, input),
    onSuccess: async () => {
      setErrorMessage(null);
      await queryClient.invalidateQueries({ queryKey: ["product", productId] });
      await queryClient.invalidateQueries({ queryKey: ["products"] });
    },
    onError: () => setErrorMessage("Não foi possível atualizar o produto."),
  });
  const statusMutation = useMutation({
    mutationFn: (status: ProductStatus) => setProductStatus(client, productId, status),
    onSuccess: async () => {
      setErrorMessage(null);
      await queryClient.invalidateQueries({ queryKey: ["product", productId] });
      await queryClient.invalidateQueries({ queryKey: ["products"] });
    },
    onError: () => setErrorMessage("Não foi possível alterar a situação do produto."),
  });

  const isAdministrator = accountQuery.data?.role === "Administrador";
  const product = productQuery.data;
  return (
    <>
      <AuthenticatedNavigation />
      <main className="mx-auto max-w-3xl space-y-space-lg p-space-md text-on-surface md:p-space-xl">
        <Link href="/products" className="text-label-md text-primary underline">Voltar ao catálogo</Link>
        <h1 className="font-headline text-headline-lg-mobile md:text-headline-lg">Detalhes do produto</h1>
        {(productQuery.isLoading || accountQuery.isLoading || racksQuery.isLoading)
          && <p role="status" className="rounded-xl bg-surface-container-low p-space-lg text-body-md text-on-surface-variant">Carregando produto...</p>}
        {(productQuery.isError || accountQuery.isError || racksQuery.isError)
          && <p role="alert" className="rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">Não foi possível carregar os dados do produto.</p>}
        {product && accountQuery.data && racksQuery.data && (
          <section className="space-y-space-lg rounded-xl bg-surface-container-lowest p-space-lg shadow-sm ring-1 ring-outline-variant/50">
            <p className="rounded-lg bg-surface-container-low p-space-md text-body-md">Situação: <span className="font-title text-title-md">{product.status === "Active" ? "Ativo" : "Inativo"}</span></p>
            <p className="rounded-lg bg-surface-container-low p-space-md text-body-md">Localização: {product.rackCode && product.shelfCode
              ? `Estante ${product.rackCode} / Prateleira ${product.shelfCode}` : "Sem localização"}</p>
            <ProductForm
              key={`${product.id}-${product.updatedAt}`}
              racks={racksQuery.data}
              product={product}
              isAdministrator={isAdministrator}
              submitting={updateMutation.isPending}
              errorMessage={errorMessage}
              onSubmit={async (input) => {
                setErrorMessage(null);
                await updateMutation.mutateAsync(input);
              }}
            />
            <ProductSuppliers product={product} />
            {isAdministrator && (
              <button type="button" disabled={statusMutation.isPending}
                onClick={() => statusMutation.mutate(product.status === "Active" ? "Inactive" : "Active")}
                className="rounded-lg bg-secondary-container px-space-md py-2.5 text-label-md text-on-secondary-fixed disabled:opacity-50">
                {product.status === "Active" ? "Inativar produto" : "Ativar produto"}
              </button>
            )}
          </section>
        )}
      </main>
    </>
  );
}
