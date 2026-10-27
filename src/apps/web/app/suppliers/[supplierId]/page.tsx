"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useState } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import SupplierForm from "@/components/suppliers/SupplierForm";
import SupplierProducts from "@/components/suppliers/SupplierProducts";
import { getCurrentAccount } from "@/lib/auth";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { getSupplier, setSupplierStatus, updateSupplier, type SupplierStatus } from "@/lib/suppliers";

export default function SupplierDetailsPage(): JSX.Element {
  const { supplierId } = useParams<{ supplierId: string }>();
  const queryClient = useQueryClient();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const client = getSupabaseBrowserClient();
  const supplierQuery = useQuery({
    queryKey: ["supplier", supplierId],
    queryFn: () => getSupplier(client, supplierId),
  });
  const accountQuery = useQuery({
    queryKey: ["current-account"],
    queryFn: () => getCurrentAccount(client),
  });
  const updateMutation = useMutation({
    mutationFn: (input: Parameters<typeof updateSupplier>[2]) => updateSupplier(client, supplierId, input),
    onSuccess: async () => {
      setErrorMessage(null);
      await queryClient.invalidateQueries({ queryKey: ["supplier", supplierId] });
      await queryClient.invalidateQueries({ queryKey: ["suppliers"] });
    },
    onError: () => setErrorMessage("Não foi possível atualizar o fornecedor."),
  });
  const statusMutation = useMutation({
    mutationFn: (status: SupplierStatus) => setSupplierStatus(client, supplierId, status),
    onSuccess: async () => {
      setErrorMessage(null);
      await queryClient.invalidateQueries({ queryKey: ["supplier", supplierId] });
      await queryClient.invalidateQueries({ queryKey: ["suppliers"] });
    },
    onError: () => setErrorMessage(
      "Não foi possível alterar a situação. Verifique se há produtos ativos vinculados.",
    ),
  });

  const supplier = supplierQuery.data;
  const isAdministrator = accountQuery.data?.role === "Administrador";
  return (
    <>
      <AuthenticatedNavigation />
      <main className="mx-auto max-w-2xl space-y-5 p-6">
        <Link href="/suppliers" className="underline">Voltar aos fornecedores</Link>
        <h1 className="text-2xl font-bold">Detalhes do fornecedor</h1>
        {(supplierQuery.isLoading || accountQuery.isLoading) && <p role="status">Carregando fornecedor...</p>}
        {(supplierQuery.isError || accountQuery.isError) && <p role="alert">Não foi possível carregar o fornecedor.</p>}
        {supplier && accountQuery.data && (
          <section className="space-y-5 rounded border p-5">
            <p>Situação: {supplier.status === "Active" ? "Ativo" : "Inativo"}</p>
            {isAdministrator
              ? <SupplierForm key={`${supplier.id}-${supplier.updatedAt}`} supplier={supplier}
                submitting={updateMutation.isPending} errorMessage={errorMessage}
                onSubmit={async (input) => {
                  setErrorMessage(null);
                  updateMutation.mutate(input);
                }} />
              : <dl className="space-y-2">
                <div><dt className="font-medium">Nome</dt><dd>{supplier.name}</dd></div>
                <div><dt className="font-medium">CPF/CNPJ</dt><dd>{supplier.taxIdentifier}</dd></div>
                <div><dt className="font-medium">Telefone</dt><dd>{supplier.phone ?? "Não informado"}</dd></div>
                <div><dt className="font-medium">E-mail</dt><dd>{supplier.email ?? "Não informado"}</dd></div>
                <div><dt className="font-medium">Contato</dt><dd>{supplier.contactName ?? "Não informado"}</dd></div>
                <div><dt className="font-medium">Observações</dt><dd>{supplier.notes ?? "Não informado"}</dd></div>
              </dl>}
            {isAdministrator && (
              <button type="button" disabled={statusMutation.isPending}
                onClick={() => statusMutation.mutate(supplier.status === "Active" ? "Inactive" : "Active")}
                className="rounded border px-4 py-2 disabled:opacity-50">
                {supplier.status === "Active" ? "Inativar fornecedor" : "Ativar fornecedor"}
              </button>
            )}
            <SupplierProducts supplierId={supplier.id} active={supplier.status === "Active"} />
          </section>
        )}
      </main>
    </>
  );
}
