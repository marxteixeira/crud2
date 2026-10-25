import Link from "next/link";
import type { Product } from "@/lib/products";

interface ProductListProps {
  products: Product[];
}

export default function ProductList({ products }: ProductListProps): JSX.Element {
  if (products.length === 0) {
    return <p>Nenhum produto encontrado.</p>;
  }

  return (
    <ul className="divide-y rounded border">
      {products.map((product) => (
        <li key={product.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
          <div>
            <Link href={`/products/${product.id}`} className="font-medium underline">{product.name}</Link>
            <p className="text-sm text-slate-600">
              {product.rackCode && product.shelfCode
                ? `Estante ${product.rackCode} / Prateleira ${product.shelfCode}`
                : "Sem localização"} · Quantidade: {product.quantity}
            </p>
          </div>
          <div className="text-right">
            <p>{product.price == null ? "Sem preço" : `R$ ${product.price.toFixed(2)}`}</p>
            <p className="text-sm">{product.status === "Active" ? "Ativo" : "Inativo"}</p>
          </div>
        </li>
      ))}
    </ul>
  );
}
