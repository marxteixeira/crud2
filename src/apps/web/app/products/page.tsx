import { Suspense } from "react";
import ProductsPageClient from "./ProductsPageClient";

export default function ProductsPage(): JSX.Element {
  return (
    <Suspense fallback={<main className="p-6" role="status">Carregando catálogo...</main>}>
      <ProductsPageClient />
    </Suspense>
  );
}
