"use client";

import Link from "next/link";
import type { UserAccount, UserStatus } from "@/lib/users";
import Button from "@/components/ui/Button";
import StatusBadge from "@/components/ui/StatusBadge";
import { DataCell, DataRow, DataTable } from "@/components/ui/DataTable";
import { EmptyState } from "@/components/ui/StateMessages";

interface UserTableProps {
  accounts: UserAccount[];
  changingId?: string;
  resendingId?: string;
  onStatusChange: (account: UserAccount, status: Exclude<UserStatus, "ConvitePendente">) => void;
  onResendInvitation?: (account: UserAccount) => void;
}

export default function UserTable({ accounts, changingId, resendingId, onStatusChange, onResendInvitation }: UserTableProps): JSX.Element {
  if (accounts.length === 0) return <EmptyState message="Nenhuma conta encontrada." />;

  return (
    <DataTable caption="Usuários cadastrados" headers={["Usuário", "Perfil de Acesso", "Situação", "Ações"]}>
      {accounts.map((account) => (
        <DataRow key={account.id}>
          <DataCell>
            <div className="font-title text-title-md text-on-surface">{account.displayName}</div>
            <div className="text-body-sm text-on-surface-variant">{account.email}</div>
            {account.pendingEmail && <div className="mt-1 text-body-sm text-on-surface-variant">Novo e-mail aguardando confirmação: {account.pendingEmail}</div>}
          </DataCell>
          <DataCell>
            <span className="inline-flex rounded-full bg-secondary-container px-space-md py-1 text-label-sm text-on-secondary-fixed">{account.role}</span>
          </DataCell>
          <DataCell>
            {account.status === "ConvitePendente" ? (
              <span className="inline-flex rounded-full bg-primary-fixed px-space-md py-1 text-label-sm text-on-primary-fixed-variant">Convite pendente</span>
            ) : (
              <StatusBadge active={account.status === "Ativa"} activeLabel="Ativa" inactiveLabel="Inativa" />
            )}
          </DataCell>
          <DataCell className="min-w-52">
            <div className="flex flex-wrap items-center gap-x-space-md gap-y-space-xs">
              <Link className="text-label-md text-on-surface-variant underline-offset-4 hover:text-primary hover:underline" href={`/users/${account.id}`}>
                Editar
              </Link>
              {account.status !== "ConvitePendente" && (
                <Button
                  variant={account.status === "Ativa" ? "danger" : "secondary"}
                  disabled={changingId === account.id}
                  onClick={() => onStatusChange(account, account.status === "Ativa" ? "Inativa" : "Ativa")}
                  className="px-2 py-1 text-body-sm"
                  aria-label={`${account.status === "Ativa" ? "Inativar" : "Reativar"} ${account.displayName}`}
                >
                  {account.status === "Ativa" ? "Inativar" : "Reativar"}
                </Button>
              )}
              {account.status === "ConvitePendente" && onResendInvitation && (
                <Button
                  variant="secondary"
                  disabled={resendingId === account.id}
                  onClick={() => onResendInvitation(account)}
                  className="px-2 py-1 text-body-sm"
                  aria-label={`Reenviar convite para ${account.displayName}`}
                >
                  Reenviar convite
                </Button>
              )}
            </div>
          </DataCell>
        </DataRow>
      ))}
    </DataTable>
  );
}
