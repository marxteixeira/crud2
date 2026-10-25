import { Suspense } from "react";
import LocationsPageClient from "./LocationsPageClient";

export default function LocationsPage(): JSX.Element {
  return (
    <Suspense fallback={<main className="p-6" role="status">Carregando localizações...</main>}>
      <LocationsPageClient />
    </Suspense>
  );
}
