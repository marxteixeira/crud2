"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import type { Supplier, SupplierInput } from "@/lib/suppliers";

const optionalText = (maxLength: number) => z.string().trim().max(maxLength).nullable();

const supplierSchema = z.object({
  name: z.string().trim().min(1, "Informe o nome ou razão social.").max(200),
  taxIdentifier: z.string().trim().min(1, "Informe o CPF ou CNPJ.")
    .regex(/^[\d./\-\s]+$/, "Use somente números e pontuação de CPF/CNPJ.")
    .refine((value) => {
      const digits = value.replace(/\D/g, "");
      return digits.length === 11 || digits.length === 14;
    }, "CPF deve ter 11 dígitos e CNPJ deve ter 14 dígitos."),
  phone: optionalText(30),
  email: z.string().trim().max(254).refine(
    (value) => value === "" || z.string().email().safeParse(value).success,
    "Informe um e-mail válido.",
  ).nullable(),
  contactName: optionalText(200),
  notes: optionalText(2000),
});

type SupplierFormValues = z.infer<typeof supplierSchema>;

interface SupplierFormProps {
  supplier?: Supplier;
  submitting: boolean;
  errorMessage?: string | null;
  onSubmit: (input: SupplierInput) => Promise<void>;
}

export default function SupplierForm({
  supplier,
  submitting,
  errorMessage,
  onSubmit,
}: SupplierFormProps): JSX.Element {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<SupplierFormValues>({
    resolver: zodResolver(supplierSchema),
    defaultValues: {
      name: supplier?.name ?? "",
      taxIdentifier: supplier?.taxIdentifier ?? "",
      phone: supplier?.phone ?? "",
      email: supplier?.email ?? "",
      contactName: supplier?.contactName ?? "",
      notes: supplier?.notes ?? "",
    },
  });

  async function submit(values: SupplierFormValues): Promise<void> {
    await onSubmit({
      ...values,
      phone: values.phone || null,
      email: values.email || null,
      contactName: values.contactName || null,
      notes: values.notes || null,
    });
  }

  return (
    <form className="space-y-4" onSubmit={handleSubmit(submit)} noValidate>
      <div>
        <label htmlFor="supplier-name" className="mb-1 block font-medium">Nome ou razão social</label>
        <input id="supplier-name" autoComplete="organization" {...register("name")}
          aria-invalid={Boolean(errors.name)} aria-describedby={errors.name ? "supplier-name-error" : undefined}
          className="w-full rounded border p-2" />
        {errors.name && <p id="supplier-name-error" role="alert">{errors.name.message}</p>}
      </div>
      <div>
        <label htmlFor="supplier-tax-id" className="mb-1 block font-medium">CPF ou CNPJ</label>
        <input id="supplier-tax-id" inputMode="numeric" {...register("taxIdentifier")}
          aria-invalid={Boolean(errors.taxIdentifier)}
          aria-describedby={errors.taxIdentifier ? "supplier-tax-id-error" : undefined}
          className="w-full rounded border p-2" />
        {errors.taxIdentifier && <p id="supplier-tax-id-error" role="alert">{errors.taxIdentifier.message}</p>}
      </div>
      <div>
        <label htmlFor="supplier-phone" className="mb-1 block font-medium">Telefone (opcional)</label>
        <input id="supplier-phone" autoComplete="tel" {...register("phone")}
          aria-invalid={Boolean(errors.phone)} className="w-full rounded border p-2" />
        {errors.phone && <p role="alert">{errors.phone.message}</p>}
      </div>
      <div>
        <label htmlFor="supplier-email" className="mb-1 block font-medium">E-mail (opcional)</label>
        <input id="supplier-email" type="email" autoComplete="email" {...register("email")}
          aria-invalid={Boolean(errors.email)} className="w-full rounded border p-2" />
        {errors.email && <p role="alert">{errors.email.message}</p>}
      </div>
      <div>
        <label htmlFor="supplier-contact" className="mb-1 block font-medium">Pessoa de contato (opcional)</label>
        <input id="supplier-contact" {...register("contactName")}
          aria-invalid={Boolean(errors.contactName)} className="w-full rounded border p-2" />
        {errors.contactName && <p role="alert">{errors.contactName.message}</p>}
      </div>
      <div>
        <label htmlFor="supplier-notes" className="mb-1 block font-medium">Observações (opcional)</label>
        <textarea id="supplier-notes" {...register("notes")}
          aria-invalid={Boolean(errors.notes)} className="w-full rounded border p-2" />
        {errors.notes && <p role="alert">{errors.notes.message}</p>}
      </div>
      {errorMessage && <p role="alert" className="text-sm text-red-700">{errorMessage}</p>}
      <button type="submit" disabled={submitting}
        className="rounded bg-blue-700 px-4 py-2 font-medium text-white disabled:opacity-60">
        {submitting ? "Salvando..." : supplier ? "Salvar alterações" : "Cadastrar fornecedor"}
      </button>
    </form>
  );
}
