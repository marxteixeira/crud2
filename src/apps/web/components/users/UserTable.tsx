"use client";

import Link from "next/link";
import type { UserAccount, UserStatus } from "@/lib/users";

interface UserTableProps {
  accounts: UserAccount[];
  changingId?: string;
  onStatusChange: (account: UserAccount, status: Exclude<UserStatus, "ConvitePendente">) => void;
}

export default function UserTable({
  accounts,
  changingId,
  onStatusChange,
}: UserTableProps): JSX.Element {
  if (accounts.length === 0) {
    return <p className="rounded border p-6 text-center">Nenhuma conta encontrada.</p>;
  }

  return (
    <div className="overflow-x-auto rounded border">
      <table className="w-full border-collapse text-left">
        <thead className="bg-slate-100">
          <tr>
            <th scope="col" className="p-3">Nome</th>
            <th scope="col" className="p-3">E-mail</th>
            <th scope="col" className="p-3">Perfil</th>
            <th scope="col" className="p-3">Situação</th>
            <th scope="col" className="p-3">Ações</th>
          </tr>
        </thead>
        <tbody>
          {accounts.map((account) => (
            <tr key={account.id} className="border-t">
              <td className="p-3">{account.displayName}</td>
              <td className="p-3">
                {account.email}
                {account.pendingEmail && <span className="block text-sm text-slate-600">Pendente: {account.pendingEmail}</span>}
              </td>
              <td className="p-3">{account.role}</td>
              <td className="p-3">{account.status}</td>
              <td className="space-x-2 p-3">
                <Link className="underline" href={`/users/${account.id}`}>Editar</Link>
                {account.status !== "ConvitePendente" && (
                  <button
                    type="button"
                    disabled={changingId === account.id}
                    onClick={() => onStatusChange(account, account.status === "Ativa" ? "Inativa" : "Ativa")}
                    className="underline disabled:opacity-60"
                    aria-label={`${account.status === "Ativa" ? "Inativar" : "Reativar"} ${account.displayName}`}
                  >
                    {account.status === "Ativa" ? "Inativar" : "Reativar"}
                  </button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
