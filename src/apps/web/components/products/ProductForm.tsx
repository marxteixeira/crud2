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
    <form className="space-y-4" onSubmit={handleSubmit(submit)} noValidate>
      <div>
        <label htmlFor="product-name" className="mb-1 block font-medium">Nome</label>
        <input id="product-name" {...register("name")} required className="w-full rounded border p-2"
          aria-invalid={Boolean(errors.name)} aria-describedby={errors.name ? "product-name-error" : undefined} />
        {errors.name && <p id="product-name-error" role="alert" className="text-sm text-red-700">{errors.name.message}</p>}
      </div>
      <div>
        <label htmlFor="product-quantity" className="mb-1 block font-medium">Quantidade</label>
        <input id="product-quantity" type="number" min="0" step="1" {...register("quantity")}
          className="w-full rounded border p-2" aria-invalid={Boolean(errors.quantity)}
          aria-describedby={errors.quantity ? "product-quantity-error" : undefined} />
        {errors.quantity && <p id="product-quantity-error" role="alert" className="text-sm text-red-700">{errors.quantity.message}</p>}
      </div>
      {isAdministrator && (
        <div>
          <label htmlFor="product-price" className="mb-1 block font-medium">Preço (R$)</label>
          <input id="product-price" type="number" min="0" step="0.01" {...register("price")}
            className="w-full rounded border p-2" aria-invalid={Boolean(errors.price)}
            aria-describedby={errors.price ? "product-price-error" : undefined} />
          {errors.price && <p id="product-price-error" role="alert" className="text-sm text-red-700">{errors.price.message}</p>}
        </div>
      )}
      <div>
        <label htmlFor="product-rack" className="mb-1 block font-medium">Estante</label>
        <select id="product-rack" value={selectedRackId}
          onChange={(event) => {
            setSelectedRackId(event.target.value);
            setValue("shelfId", "");
          }}
          className="w-full rounded border p-2">
          <option value="">Selecione uma estante</option>
          {racks.filter((rack) => rack.status === "Active" || rack.id === selectedRackId)
            .map((rack) => <option key={rack.id} value={rack.id}>{rack.code}</option>)}
        </select>
        <label htmlFor="product-shelf" className="mb-1 mt-2 block font-medium">Prateleira</label>
        <select id="product-shelf" {...register("shelfId")} className="w-full rounded border p-2">
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
      {product?.price == null && <p className="text-sm text-amber-800">Sem preço: o produto permanecerá inativo até um Administrador informar o preço.</p>}
      {errorMessage && <p role="alert" className="text-sm text-red-700">{errorMessage}</p>}
      <button type="submit" disabled={submitting}
        className="rounded bg-blue-700 px-4 py-2 font-medium text-white disabled:opacity-60">
        {submitting ? "Salvando..." : product ? "Salvar produto" : "Cadastrar produto"}
      </button>
    </form>
  );
}
