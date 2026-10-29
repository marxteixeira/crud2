"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import Button from "@/components/ui/Button";
import type { Supplier, SupplierInput } from "@/lib/suppliers";

const optionalText = (maxLength: number) => z.string().trim().max(maxLength).nullable();
const fieldClass = "w-full rounded-lg border border-outline-variant bg-surface-container-lowest px-space-md py-2 text-body-md text-on-surface placeholder:text-secondary focus:border-primary-container focus:outline-none focus:ring-2 focus:ring-primary-container/30";

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
    <form className="space-y-space-lg" onSubmit={handleSubmit(submit)} noValidate>
      <div className="grid gap-space-lg md:grid-cols-2">
        <div className="flex flex-col gap-space-xs">
          <label htmlFor="supplier-name" className="text-label-md text-on-surface">Nome ou razão social</label>
          <input id="supplier-name" autoComplete="organization" {...register("name")}
            aria-invalid={Boolean(errors.name)} aria-describedby={errors.name ? "supplier-name-error" : undefined}
            className={fieldClass} />
          {errors.name && <p id="supplier-name-error" role="alert" className="text-body-sm text-error">{errors.name.message}</p>}
        </div>
        <div className="flex flex-col gap-space-xs">
          <label htmlFor="supplier-tax-id" className="text-label-md text-on-surface">CPF ou CNPJ</label>
          <input id="supplier-tax-id" inputMode="numeric" {...register("taxIdentifier")}
            aria-invalid={Boolean(errors.taxIdentifier)} aria-describedby={errors.taxIdentifier ? "supplier-tax-id-error" : undefined}
            className={fieldClass} />
          {errors.taxIdentifier && <p id="supplier-tax-id-error" role="alert" className="text-body-sm text-error">{errors.taxIdentifier.message}</p>}
        </div>
        <div className="flex flex-col gap-space-xs">
          <label htmlFor="supplier-phone" className="text-label-md text-on-surface">Telefone (opcional)</label>
          <input id="supplier-phone" autoComplete="tel" {...register("phone")}
            aria-invalid={Boolean(errors.phone)} aria-describedby={errors.phone ? "supplier-phone-error" : undefined}
            className={fieldClass} />
          {errors.phone && <p id="supplier-phone-error" role="alert" className="text-body-sm text-error">{errors.phone.message}</p>}
        </div>
        <div className="flex flex-col gap-space-xs">
          <label htmlFor="supplier-email" className="text-label-md text-on-surface">E-mail (opcional)</label>
          <input id="supplier-email" type="email" autoComplete="email" {...register("email")}
            aria-invalid={Boolean(errors.email)} aria-describedby={errors.email ? "supplier-email-error" : undefined}
            className={fieldClass} />
          {errors.email && <p id="supplier-email-error" role="alert" className="text-body-sm text-error">{errors.email.message}</p>}
        </div>
        <div className="flex flex-col gap-space-xs">
          <label htmlFor="supplier-contact" className="text-label-md text-on-surface">Pessoa de contato (opcional)</label>
          <input id="supplier-contact" {...register("contactName")}
            aria-invalid={Boolean(errors.contactName)} aria-describedby={errors.contactName ? "supplier-contact-error" : undefined}
            className={fieldClass} />
          {errors.contactName && <p id="supplier-contact-error" role="alert" className="text-body-sm text-error">{errors.contactName.message}</p>}
        </div>
        <div className="flex flex-col gap-space-xs md:col-span-2">
          <label htmlFor="supplier-notes" className="text-label-md text-on-surface">Observações (opcional)</label>
          <textarea id="supplier-notes" {...register("notes")}
            aria-invalid={Boolean(errors.notes)} aria-describedby={errors.notes ? "supplier-notes-error" : undefined}
            className={`${fieldClass} min-h-28`} />
          {errors.notes && <p id="supplier-notes-error" role="alert" className="text-body-sm text-error">{errors.notes.message}</p>}
        </div>
      </div>
      {errorMessage && <p role="alert" className="rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">{errorMessage}</p>}
      <div className="flex justify-end border-t border-outline-variant/50 pt-space-md">
        <Button type="submit" disabled={submitting}>
          {submitting ? "Salvando..." : supplier ? "Salvar alterações" : "Cadastrar fornecedor"}
        </Button>
      </div>
    </form>
  );
}
