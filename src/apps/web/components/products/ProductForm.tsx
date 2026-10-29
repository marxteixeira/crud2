"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import type { Product, ProductInput } from "@/lib/products";
import type { Rack } from "@/lib/locations";

const productSchema = z.object({
  name: z.string().trim().min(1, "Informe o nome do produto.").max(200),
  quantity: z.string().regex(/^\d+$/, "Informe uma quantidade inteira maior ou igual a zero."),
  price: z.string().optional().refine(
    (value) => !value || /^\d+([.]\d{1,2})?$/.test(value),
    "Informe um preço não negativo com até duas casas decimais.",
  ),
  shelfId: z.string(),
});

type ProductFormValues = z.infer<typeof productSchema>;

interface ProductFormProps {
  racks: Rack[];
  product?: Product;
  isAdministrator: boolean;
  submitting: boolean;
  errorMessage?: string | null;
  onSubmit: (input: ProductInput) => Promise<void>;
}

export default function ProductForm({
  racks,
  product,
  isAdministrator,
  submitting,
  errorMessage,
  onSubmit,
}: ProductFormProps): JSX.Element {
  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<ProductFormValues>({
    resolver: zodResolver(productSchema),
    defaultValues: {
      name: product?.name ?? "",
      quantity: String(product?.quantity ?? 0),
      price: product?.price == null ? "" : String(product.price),
      shelfId: product?.shelfId ?? "",
    },
  });
  const [selectedRackId, setSelectedRackId] = useState(
    racks.find((rack) => rack.shelves.some((shelf) => shelf.id === product?.shelfId))?.id ?? "",
  );

  async function submit(values: ProductFormValues): Promise<void> {
    await onSubmit({
      name: values.name.trim(),
      quantity: Number(values.quantity),
      ...(isAdministrator ? { price: values.price ? Number(values.price) : null } : {}),
      shelfId: values.shelfId || null,
    });
  }

  return (
    <form className="space-y-space-lg" onSubmit={handleSubmit(submit)} noValidate>
      <div>
        <label htmlFor="product-name" className="mb-space-xs block text-label-md text-on-surface">Nome</label>
        <input id="product-name" {...register("name")} required className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md text-on-surface placeholder:text-secondary focus:border-primary-container"
          aria-invalid={Boolean(errors.name)} aria-describedby={errors.name ? "product-name-error" : undefined} />
        {errors.name && <p id="product-name-error" role="alert" className="text-body-sm text-error">{errors.name.message}</p>}
      </div>
      <div>
        <label htmlFor="product-quantity" className="mb-space-xs block text-label-md text-on-surface">Quantidade</label>
        <input id="product-quantity" type="number" min="0" step="1" {...register("quantity")}
          className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md text-on-surface placeholder:text-secondary focus:border-primary-container" aria-invalid={Boolean(errors.quantity)}
          aria-describedby={errors.quantity ? "product-quantity-error" : undefined} />
        {errors.quantity && <p id="product-quantity-error" role="alert" className="text-body-sm text-error">{errors.quantity.message}</p>}
      </div>
      {isAdministrator && (
        <div>
          <label htmlFor="product-price" className="mb-space-xs block text-label-md text-on-surface">Preço (R$)</label>
          <input id="product-price" type="number" min="0" step="0.01" {...register("price")}
            className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md text-on-surface placeholder:text-secondary focus:border-primary-container" aria-invalid={Boolean(errors.price)}
            aria-describedby={errors.price ? "product-price-error" : undefined} />
          {errors.price && <p id="product-price-error" role="alert" className="text-body-sm text-error">{errors.price.message}</p>}
        </div>
      )}
      <div>
        <label htmlFor="product-rack" className="mb-space-xs block text-label-md text-on-surface">Estante</label>
        <select id="product-rack" value={selectedRackId}
          onChange={(event) => {
            setSelectedRackId(event.target.value);
            setValue("shelfId", "");
          }}
          className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md text-on-surface placeholder:text-secondary focus:border-primary-container">
          <option value="">Selecione uma estante</option>
          {racks.filter((rack) => rack.status === "Active" || rack.id === selectedRackId)
            .map((rack) => <option key={rack.id} value={rack.id}>{rack.code}</option>)}
        </select>
        <label htmlFor="product-shelf" className="mb-space-xs mt-space-md block text-label-md text-on-surface">Prateleira</label>
        <select id="product-shelf" {...register("shelfId")} className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-space-md py-2.5 text-body-md text-on-surface placeholder:text-secondary focus:border-primary-container">
          <option value="">Sem localização</option>
          {racks.filter((rack) => rack.id === selectedRackId).flatMap((rack) => rack.shelves
            .filter((shelf) => shelf.status === "Active" || shelf.id === product?.shelfId)
            .map((shelf) => (
              <option key={shelf.id} value={shelf.id}>
                {rack.code} / {shelf.code}{shelf.status === "Inactive" ? " (inativa)" : ""}
              </option>
            )))}
        </select>
      </div>
      {product && !isAdministrator && (
        <p>Preço: {product.price == null ? "Sem preço" : `R$ ${product.price.toFixed(2)}`}</p>
      )}
      {product?.price == null && <p className="rounded-lg bg-secondary-container p-space-md text-body-sm text-on-secondary-fixed-variant">Sem preço: o produto permanecerá inativo até um Administrador informar o preço.</p>}
      {errorMessage && <p role="alert" className="text-body-sm text-error">{errorMessage}</p>}
      <button type="submit" disabled={submitting}
        className="rounded-lg bg-primary-container px-space-lg py-2.5 text-label-md text-on-primary hover:bg-primary disabled:opacity-60">
        {submitting ? "Salvando..." : product ? "Salvar produto" : "Cadastrar produto"}
      </button>
    </form>
  );
}
