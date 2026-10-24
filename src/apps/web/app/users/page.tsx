import { Suspense } from "react";
import UsersPageClient from "./UsersPageClient";

export default function UsersPage(): JSX.Element {
  return (
    <Suspense fallback={<main className="p-6" role="status">Carregando usuários...</main>}>
      <UsersPageClient />
    </Suspense>
  );
}
