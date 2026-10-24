"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import type { UserAccount, UserInput } from "@/lib/users";

const userSchema = z.object({
  email: z.string().trim().email("Informe um e-mail válido."),
  displayName: z.string().trim().min(1, "Informe o nome.").max(200),
  role: z.enum(["Administrador", "Operador"]),
});

type UserFormValues = z.infer<typeof userSchema>;

interface UserFormProps {
  account?: UserAccount;
  submitting: boolean;
  errorMessage?: string | null;
  onSubmit: (input: UserInput) => Promise<void>;
}

export default function UserForm({
  account,
  submitting,
  errorMessage,
  onSubmit,
}: UserFormProps): JSX.Element {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UserFormValues>({
    resolver: zodResolver(userSchema),
    defaultValues: {
      email: account?.email ?? "",
      displayName: account?.displayName ?? "",
      role: account?.role ?? "Operador",
    },
  });

  async function submit(values: UserFormValues): Promise<void> {
    await onSubmit(values);
  }

  return (
    <form className="space-y-4" onSubmit={handleSubmit(submit)} noValidate>
      <div>
        <label className="mb-1 block font-medium" htmlFor="user-email">E-mail</label>
        <input id="user-email" autoComplete="email" type="email" {...register("email")}
          aria-invalid={Boolean(errors.email)} aria-describedby={errors.email ? "user-email-error" : undefined}
          className="w-full rounded border p-2" />
        {errors.email && <p id="user-email-error" role="alert" className="text-sm text-red-700">{errors.email.message}</p>}
        {account?.pendingEmail && <p className="mt-1 text-sm">Novo e-mail aguardando confirmação: {account.pendingEmail}</p>}
      </div>
      <div>
        <label className="mb-1 block font-medium" htmlFor="user-name">Nome</label>
        <input id="user-name" autoComplete="name" {...register("displayName")}
          aria-invalid={Boolean(errors.displayName)} aria-describedby={errors.displayName ? "user-name-error" : undefined}
          className="w-full rounded border p-2" />
        {errors.displayName && <p id="user-name-error" role="alert" className="text-sm text-red-700">{errors.displayName.message}</p>}
      </div>
      <div>
        <label className="mb-1 block font-medium" htmlFor="user-role">Perfil</label>
        <select id="user-role" {...register("role")} className="w-full rounded border p-2">
          <option value="Operador">Operador</option>
          <option value="Administrador">Administrador</option>
        </select>
      </div>
      {errorMessage && <p role="alert" className="text-sm text-red-700">{errorMessage}</p>}
      <button type="submit" disabled={submitting}
        className="rounded bg-blue-700 px-4 py-2 font-medium text-white disabled:opacity-60">
        {submitting ? "Salvando..." : account ? "Salvar alterações" : "Enviar convite"}
      </button>
    </form>
  );
}
