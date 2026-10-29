"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";
import type { UserAccount, UserInput } from "@/lib/users";
import Button from "@/components/ui/Button";
import { InputField, SelectField } from "@/components/ui/Field";

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

export default function UserForm({ account, submitting, errorMessage, onSubmit }: UserFormProps): JSX.Element {
  const {
    control,
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
    <form className="space-y-space-md" onSubmit={handleSubmit(submit)} noValidate>
      <Controller name="email" control={control} render={({ field }) => <InputField label="E-mail" autoComplete="email" type="email" name={field.name} value={field.value} onChange={field.onChange} onBlur={field.onBlur} error={errors.email?.message} />} />
      {account?.pendingEmail && <p className="-mt-space-xs text-body-sm text-on-surface-variant">Novo e-mail aguardando confirmação: {account.pendingEmail}</p>}
      <Controller name="displayName" control={control} render={({ field }) => <InputField label="Nome" autoComplete="name" name={field.name} value={field.value} onChange={field.onChange} onBlur={field.onBlur} error={errors.displayName?.message} />} />
      <Controller name="role" control={control} render={({ field }) => (
        <SelectField label="Perfil" name={field.name} value={field.value} onChange={field.onChange} onBlur={field.onBlur}>
          <option value="Operador">Operador</option>
          <option value="Administrador">Administrador</option>
        </SelectField>
      )} />
      {errorMessage && <p role="alert" className="rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">{errorMessage}</p>}
      <Button type="submit" disabled={submitting} className="w-full sm:w-auto">
        {submitting ? "Salvando..." : account ? "Salvar alterações" : "Enviar convite"}
      </Button>
    </form>
  );
}
