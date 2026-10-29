"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { listProducts } from "@/lib/products";
import Button from "@/components/ui/Button";
import DataTable, { DataCell, DataRow } from "@/components/ui/DataTable";
import { SelectField } from "@/components/ui/Field";
import { EmptyState, ErrorState, LoadingState } from "@/components/ui/StateMessages";
import StatusBadge from "@/components/ui/StatusBadge";
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
    <section aria-labelledby="supplier-products-heading" className="space-y-space-md border-t border-outline-variant/50 pt-space-lg">
      <h2 id="supplier-products-heading" className="font-headline text-headline-sm">Produtos fornecidos</h2>
      {active && (
        <div className="flex flex-wrap items-end gap-space-sm">
          <SelectField label="Adicionar produto" value={productId}
            onChange={(event) => setProductId(event.target.value)} className="min-w-60">
            <option value="">Selecione</option>
            {productsQuery.data?.items.map((product) => (
              <option key={product.id} value={product.id}>{product.name}</option>
            ))}
          </SelectField>
          <Button type="button" variant="secondary" disabled={!productId || associateMutation.isPending}
            onClick={() => {
              if (!productId) return;
              associateMutation.mutate(productId);
            }}>
            Associar
          </Button>
        </div>
      )}
      <div className="max-w-xs">
        <SelectField label="Relações" value={linkStatus}
          onChange={(event) => setLinkStatus(event.target.value as SupplierStatus)}>
          <option value="Active">Ativas</option>
          <option value="Inactive">Históricas</option>
        </SelectField>
      </div>
      {errorMessage && <ErrorState message={errorMessage} />}
      {(productsQuery.isError || linksQuery.isError)
        && <ErrorState message="Não foi possível carregar os produtos associados." />}
      {linksQuery.isLoading && <LoadingState message="Carregando produtos..." />}
      {linksQuery.data && linksQuery.data.items.length === 0 && <EmptyState message="Nenhum produto nesta relação." />}
      {linksQuery.data && linksQuery.data.items.length > 0 && (
        <DataTable caption="Produtos relacionados ao fornecedor" headers={["Produto", "Relação", "Ações"]}>
          {linksQuery.data.items.map((link) => (
            <DataRow key={link.productId}>
              <DataCell className="font-title text-title-md">{link.productName} — {link.productStatus === "Active" ? "Ativo" : "Inativo"}</DataCell>
              <DataCell><StatusBadge active={link.linkStatus === "Active"} activeLabel="Ativa" inactiveLabel="Histórica" /></DataCell>
              <DataCell>
                {link.linkStatus === "Active" && (
                  <Button type="button" variant="secondary" disabled={disassociateMutation.isPending}
                    onClick={() => disassociateMutation.mutate(link.productId)}>
                    Desassociar
                  </Button>
                )}
              </DataCell>
            </DataRow>
          ))}
        </DataTable>
      )}
    </section>
  );
}
