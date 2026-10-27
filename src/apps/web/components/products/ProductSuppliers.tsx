"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { listSuppliers } from "@/lib/suppliers";
import {
  associateSupplier,
  disassociateSupplier,
  listSuppliersForProduct,
} from "@/lib/suppliers";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import type { Product } from "@/lib/products";
import { type SupplierStatus } from "@/lib/suppliers";

const pageSize = 100;

export default function ProductSuppliers({ product }: { product: Product }): JSX.Element {
  const client = getSupabaseBrowserClient();
  const queryClient = useQueryClient();
  const [selectedSupplierId, setSelectedSupplierId] = useState("");
  const [linkStatus, setLinkStatus] = useState<SupplierStatus>("Active");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const suppliersQuery = useQuery({
    queryKey: ["suppliers", "Active", "product-links"],
    queryFn: () => listSuppliers(client, { search: "", status: "Active", page: 1, pageSize }),
  });
  const linksQuery = useQuery({
    queryKey: ["product-suppliers", product.id, linkStatus],
    queryFn: () => listSuppliersForProduct(client, product.id, { linkStatus, page: 1, pageSize }),
  });

  async function refreshLinks(): Promise<void> {
    setErrorMessage(null);
    await queryClient.invalidateQueries({ queryKey: ["product-suppliers", product.id] });
    await queryClient.invalidateQueries({ queryKey: ["supplier-products"] });
  }

  const associateMutation = useMutation({
    mutationFn: (supplierId: string) => associateSupplier(client, product.id, supplierId),
    onSuccess: refreshLinks,
    onError: () => setErrorMessage("Não foi possível associar o fornecedor. Verifique a situação dos cadastros."),
  });
  const disassociateMutation = useMutation({
    mutationFn: (supplierId: string) => disassociateSupplier(client, product.id, supplierId),
    onSuccess: refreshLinks,
    onError: () => setErrorMessage("Não foi possível desassociar o fornecedor."),
  });

  return (
    <section aria-labelledby="product-suppliers-heading" className="space-y-3 border-t pt-4">
      <h2 id="product-suppliers-heading" className="text-lg font-semibold">Fornecedores</h2>
      {product.status === "Active" && (
        <div className="flex flex-wrap items-end gap-2">
          <div>
            <label htmlFor="supplier-select" className="mb-1 block text-sm font-medium">Adicionar fornecedor</label>
            <select id="supplier-select" value={selectedSupplierId}
              onChange={(event) => setSelectedSupplierId(event.target.value)} className="rounded border p-2">
              <option value="">Selecione</option>
              {suppliersQuery.data?.items.map((supplier) => (
                <option key={supplier.id} value={supplier.id}>{supplier.name}</option>
              ))}
            </select>
          </div>
          <button type="button" disabled={!selectedSupplierId || associateMutation.isPending}
            onClick={() => {
              if (!selectedSupplierId) return;
              associateMutation.mutate(selectedSupplierId);
            }} className="rounded border px-3 py-2 disabled:opacity-50">
            Associar
          </button>
        </div>
      )}
      <div>
        <label htmlFor="product-link-status" className="mr-2 text-sm font-medium">Relações</label>
        <select id="product-link-status" value={linkStatus}
          onChange={(event) => setLinkStatus(event.target.value as SupplierStatus)} className="rounded border p-2">
          <option value="Active">Ativas</option>
          <option value="Inactive">Históricas</option>
        </select>
      </div>
      {errorMessage && <p role="alert" className="text-sm text-red-700">{errorMessage}</p>}
      {suppliersQuery.isError || linksQuery.isError
        ? <p role="alert">Não foi possível carregar os vínculos de fornecedores.</p>
        : null}
      {linksQuery.isLoading && <p role="status">Carregando fornecedores...</p>}
      {linksQuery.data && linksQuery.data.items.length === 0 && <p>Nenhum fornecedor nesta relação.</p>}
      {linksQuery.data && linksQuery.data.items.length > 0 && (
        <ul className="divide-y rounded border">
          {linksQuery.data.items.map((link) => (
            <li key={link.supplierId} className="flex flex-wrap items-center justify-between gap-2 p-3">
              <span>
                {link.supplierName} — {link.taxIdentifier}
                {link.supplierStatus === "Inactive" && " (fornecedor inativo)"}
              </span>
              {link.linkStatus === "Active" && (
                <button type="button" disabled={disassociateMutation.isPending}
                  onClick={() => disassociateMutation.mutate(link.supplierId)}
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
