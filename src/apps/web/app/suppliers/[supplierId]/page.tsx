"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useState } from "react";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import SupplierForm from "@/components/suppliers/SupplierForm";
import SupplierProducts from "@/components/suppliers/SupplierProducts";
import Button from "@/components/ui/Button";
import Dialog from "@/components/ui/Dialog";
import PageHeader from "@/components/ui/PageHeader";
import { ErrorState, LoadingState } from "@/components/ui/StateMessages";
import StatusBadge from "@/components/ui/StatusBadge";
import { getCurrentAccount } from "@/lib/auth";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { getSupplier, setSupplierStatus, updateSupplier, type SupplierStatus } from "@/lib/suppliers";

export default function SupplierDetailsPage(): JSX.Element {
  const { supplierId } = useParams<{ supplierId: string }>();
  const queryClient = useQueryClient();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [showInactivationDialog, setShowInactivationDialog] = useState(false);
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
      setShowInactivationDialog(false);
      await queryClient.invalidateQueries({ queryKey: ["supplier", supplierId] });
      await queryClient.invalidateQueries({ queryKey: ["suppliers"] });
    },
    onError: () => setErrorMessage("Não foi possível alterar a situação. Verifique se há produtos ativos vinculados."),
  });

  const supplier = supplierQuery.data;
  const isAdministrator = accountQuery.data?.role === "Administrador";
  return (
    <>
      <AuthenticatedNavigation />
      <main className="min-h-screen bg-surface px-space-md py-space-xl text-on-surface md:ml-64 md:px-space-xl">
        <div className="mx-auto max-w-4xl">
          <Link href="/suppliers" className="mb-space-md inline-flex text-label-md text-primary hover:underline">
            Voltar aos fornecedores
          </Link>
          <PageHeader title="Detalhes do fornecedor" description="Dados cadastrais e produtos fornecidos." />
          {(supplierQuery.isLoading || accountQuery.isLoading) && <LoadingState message="Carregando fornecedor..." />}
          {(supplierQuery.isError || accountQuery.isError) && <ErrorState message="Não foi possível carregar o fornecedor." />}
          {supplier && accountQuery.data && (
            <section className="space-y-space-lg rounded-xl bg-surface-container-lowest p-space-lg shadow-sm ring-1 ring-outline-variant/50 md:p-space-xl">
              <div className="flex flex-wrap items-center justify-between gap-space-md border-b border-outline-variant/50 pb-space-md">
                <div>
                  <h2 className="font-headline text-headline-sm">{supplier.name}</h2>
                  <p className="mt-1 text-body-sm text-on-surface-variant">{supplier.taxIdentifier}</p>
                </div>
                <StatusBadge active={supplier.status === "Active"} />
              </div>
              {isAdministrator
                ? <SupplierForm key={`${supplier.id}-${supplier.updatedAt}`} supplier={supplier}
                  submitting={updateMutation.isPending} errorMessage={errorMessage}
                  onSubmit={async (input) => {
                    setErrorMessage(null);
                    updateMutation.mutate(input);
                  }} />
                : <dl className="grid gap-space-md sm:grid-cols-2">
                  <div><dt className="text-label-sm text-on-surface-variant">Nome</dt><dd className="mt-1 text-body-md">{supplier.name}</dd></div>
                  <div><dt className="text-label-sm text-on-surface-variant">CPF/CNPJ</dt><dd className="mt-1 text-body-md">{supplier.taxIdentifier}</dd></div>
                  <div><dt className="text-label-sm text-on-surface-variant">Telefone</dt><dd className="mt-1 text-body-md">{supplier.phone ?? "Não informado"}</dd></div>
                  <div><dt className="text-label-sm text-on-surface-variant">E-mail</dt><dd className="mt-1 text-body-md">{supplier.email ?? "Não informado"}</dd></div>
                  <div><dt className="text-label-sm text-on-surface-variant">Contato</dt><dd className="mt-1 text-body-md">{supplier.contactName ?? "Não informado"}</dd></div>
                  <div className="sm:col-span-2"><dt className="text-label-sm text-on-surface-variant">Observações</dt><dd className="mt-1 whitespace-pre-wrap text-body-md">{supplier.notes ?? "Não informado"}</dd></div>
                </dl>}
              {isAdministrator && (
                <div className="flex justify-end border-t border-outline-variant/50 pt-space-md">
                  <Button type="button" variant={supplier.status === "Active" ? "danger" : "secondary"}
                    disabled={statusMutation.isPending}
                    onClick={() => {
                      setErrorMessage(null);
                      if (supplier.status === "Active") setShowInactivationDialog(true);
                      else statusMutation.mutate("Active");
                    }}>
                    {supplier.status === "Active" ? "Inativar fornecedor" : "Ativar fornecedor"}
                  </Button>
                </div>
              )}
              <SupplierProducts supplierId={supplier.id} active={supplier.status === "Active"} />
            </section>
          )}
        </div>
      </main>
      <Dialog open={showInactivationDialog} title="Alterar Situação" onClose={() => setShowInactivationDialog(false)}>
        <div className="space-y-space-md">
          <p className="text-body-md text-on-surface-variant">
            Confirma a inativação deste fornecedor? Produtos ativos vinculados podem impedir a alteração.
          </p>
          <div className="flex justify-end gap-space-sm">
            <Button type="button" variant="secondary" onClick={() => setShowInactivationDialog(false)}>Cancelar</Button>
            <Button type="button" variant="danger" disabled={statusMutation.isPending}
              onClick={() => statusMutation.mutate("Inactive")}>
              Confirmar Inativação
            </Button>
          </div>
        </div>
      </Dialog>
    </>
  );
}
