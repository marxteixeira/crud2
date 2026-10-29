import Link from "next/link";
import type { Product } from "@/lib/products";
import { DataCell, DataRow, DataTable } from "@/components/ui/DataTable";
import StatusBadge from "@/components/ui/StatusBadge";

interface ProductListProps {
  products: Product[];
}

export default function ProductList({ products }: ProductListProps): JSX.Element {
  if (products.length === 0) {
    return <p className="rounded-xl bg-surface-container-low p-space-lg text-center text-body-md text-on-surface-variant">Nenhum produto encontrado.</p>;
  }

  return (
    <DataTable caption="Catálogo de produtos" headers={["Produto e SKU", "Localização Física", "Fornecedor", "Quantidade", "Preço Unitário", "Situação", "Ações"]}>
      {products.map((product) => (
        <DataRow key={product.id}>
          <DataCell>
            <Link href={`/products/${product.id}`} className="font-title text-title-md text-on-surface hover:text-primary hover:underline">{product.name}</Link>
            <p className="mt-1 text-body-sm text-on-surface-variant">ID: {product.id}</p>
          </DataCell>
          <DataCell>
            <span className="inline-flex rounded-lg bg-secondary-container px-space-sm py-1 text-body-sm text-on-secondary-fixed-variant">
              {product.rackCode && product.shelfCode
                ? `Estante ${product.rackCode} / Prateleira ${product.shelfCode}`
                : "Sem localização"}
            </span>
          </DataCell>
          <DataCell className="text-on-surface-variant">—</DataCell>
          <DataCell>{product.quantity} un</DataCell>
          <DataCell>{product.price == null ? "Sem preço" : `R$ ${product.price.toFixed(2)}`}</DataCell>
          <DataCell><StatusBadge active={product.status === "Active"} /></DataCell>
          <DataCell>
            <Link href={`/products/${product.id}`} className="rounded-lg px-space-sm py-1.5 text-label-md text-primary hover:bg-secondary-container">Ver produto</Link>
          </DataCell>
        </DataRow>
      ))}
    </DataTable>
  );
}
