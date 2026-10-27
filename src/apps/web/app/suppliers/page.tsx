import { Suspense } from "react";
import SuppliersPageClient from "./SuppliersPageClient";

export default function SuppliersPage(): JSX.Element {
  return (
    <Suspense fallback={<main className="p-6" role="status">Carregando fornecedores...</main>}>
      <SuppliersPageClient />
    </Suspense>
  );
}
