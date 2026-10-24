import HealthStatus from "@/components/HealthStatus";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";

export default function Home(): JSX.Element {
  return (
    <>
      <AuthenticatedNavigation />
      <main className="flex min-h-screen flex-col items-center justify-center gap-4 p-6">
        <h1 className="text-2xl font-bold">Sistema de Estoque</h1>
        <HealthStatus />
      </main>
    </>
  );
}
