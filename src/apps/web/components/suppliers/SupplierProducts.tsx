"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { listProducts } from "@/lib/products";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import {
  associateSupplier,
  disassociateSupplier,
  listProductsForSupplier,
  type SupplierStatus,
} from "@/lib/suppliers";

const pageSize = 100;

export default function SupplierProducts({
  supplierId,
  active,
}: {
  supplierId: string;
  active: boolean;
}): JSX.Element {
  const client = getSupabaseBrowserClient();
  const queryClient = useQueryClient();
  const [productId, setProductId] = useState("");
  const [linkStatus, setLinkStatus] = useState<SupplierStatus>("Active");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const productsQuery = useQuery({
    queryKey: ["products", "Active", "supplier-links"],
    queryFn: () => listProducts(client, { status: "Active", page: 1, pageSize }),
  });
  const linksQuery = useQuery({
    queryKey: ["supplier-products", supplierId, linkStatus],
    queryFn: () => listProductsForSupplier(client, supplierId, { linkStatus, page: 1, pageSize }),
  });

  async function refreshLinks(): Promise<void> {
    setErrorMessage(null);
    await queryClient.invalidateQueries({ queryKey: ["supplier-products", supplierId] });
    await queryClient.invalidateQueries({ queryKey: ["product-suppliers"] });
  }

  const associateMutation = useMutation({
    mutationFn: (selectedProductId: string) => associateSupplier(client, selectedProductId, supplierId),
    onSuccess: refreshLinks,
    onError: () => setErrorMessage("Não foi possível associar o produto. Verifique a situação dos cadastros."),
  });
  const disassociateMutation = useMutation({
    mutationFn: (selectedProductId: string) => disassociateSupplier(client, selectedProductId, supplierId),
    onSuccess: refreshLinks,
    onError: () => setErrorMessage("Não foi possível desassociar o produto."),
  });

  return (
    <section aria-labelledby="supplier-products-heading" className="space-y-3 border-t pt-4">
      <h2 id="supplier-products-heading" className="text-lg font-semibold">Produtos fornecidos</h2>
      {active && (
        <div className="flex flex-wrap items-end gap-2">
          <div>
            <label htmlFor="product-select" className="mb-1 block text-sm font-medium">Adicionar produto</label>
            <select id="product-select" value={productId}
              onChange={(event) => setProductId(event.target.value)} className="rounded border p-2">
              <option value="">Selecione</option>
              {productsQuery.data?.items.map((product) => (
                <option key={product.id} value={product.id}>{product.name}</option>
              ))}
            </select>
          </div>
          <button type="button" disabled={!productId || associateMutation.isPending}
            onClick={() => {
              if (!productId) return;
              associateMutation.mutate(productId);
            }} className="rounded border px-3 py-2 disabled:opacity-50">
            Associar
          </button>
        </div>
      )}
      <div>
        <label htmlFor="supplier-link-status" className="mr-2 text-sm font-medium">Relações</label>
        <select id="supplier-link-status" value={linkStatus}
          onChange={(event) => setLinkStatus(event.target.value as SupplierStatus)} className="rounded border p-2">
          <option value="Active">Ativas</option>
          <option value="Inactive">Históricas</option>
        </select>
      </div>
      {errorMessage && <p role="alert" className="text-sm text-red-700">{errorMessage}</p>}
      {(productsQuery.isError || linksQuery.isError)
        && <p role="alert">Não foi possível carregar os produtos associados.</p>}
      {linksQuery.isLoading && <p role="status">Carregando produtos...</p>}
      {linksQuery.data && linksQuery.data.items.length === 0 && <p>Nenhum produto nesta relação.</p>}
      {linksQuery.data && linksQuery.data.items.length > 0 && (
        <ul className="divide-y rounded border">
          {linksQuery.data.items.map((link) => (
            <li key={link.productId} className="flex flex-wrap items-center justify-between gap-2 p-3">
              <span>{link.productName} — {link.productStatus === "Active" ? "Ativo" : "Inativo"}</span>
              {link.linkStatus === "Active" && (
                <button type="button" disabled={disassociateMutation.isPending}
                  onClick={() => disassociateMutation.mutate(link.productId)}
                  className="rounded border px-3 py-1 disabled:opacity-50">
                  Desassociar
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
