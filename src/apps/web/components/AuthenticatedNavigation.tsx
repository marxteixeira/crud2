"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { getCurrentAccount } from "@/lib/auth";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { requestWithSession } from "@/lib/api";

export default function AuthenticatedNavigation(): JSX.Element {
  const router = useRouter();
  const queryClient = useQueryClient();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const accountQuery = useQuery({
    queryKey: ["current-account"],
    queryFn: () => getCurrentAccount(getSupabaseBrowserClient()),
  });

  useEffect(() => {
    if (!accountQuery.isError) return;
    const clearRejectedSession = async (): Promise<void> => {
      const { error } = await getSupabaseBrowserClient().auth.signOut();
      if (error) {
        setErrorMessage("A sessão foi recusada, mas não foi possível limpá-la neste navegador.");
      }
      router.replace("/login?error=session");
      router.refresh();
    };
    void clearRejectedSession();
  }, [accountQuery.isError, router]);

  async function logout(): Promise<void> {
    setErrorMessage(null);
    try {
      await requestWithSession<void>(getSupabaseBrowserClient(), "POST", "/api/auth/logout");
    } catch {
      setErrorMessage("Não foi possível confirmar a saída com o servidor. Tente novamente.");
      return;
    }

    queryClient.clear();
    const { error } = await getSupabaseBrowserClient().auth.signOut();
    if (error) {
      setErrorMessage("A sessão foi revogada no servidor, mas não foi possível limpar a sessão local.");
      return;
    }
    router.replace("/login");
    router.refresh();
  }

  return (
    <header className="flex flex-wrap items-center justify-between gap-4 border-b px-6 py-4">
      <nav aria-label="Navegação principal" className="flex flex-wrap items-center gap-4">
        <Link href="/" className="font-semibold">Sistema de Estoque</Link>
        {accountQuery.data?.role === "Administrador" && <Link href="/users" className="underline">Usuários</Link>}
      </nav>
      <div className="flex items-center gap-3">
        <span aria-live="polite" className="text-sm">
          {accountQuery.data?.displayName ?? accountQuery.data?.email ?? "Carregando sessão..."}
        </span>
        <button type="button" onClick={() => void logout()} className="rounded border px-3 py-1">
          Sair
        </button>
      </div>
      {errorMessage && <p role="alert" className="w-full text-sm text-red-700">{errorMessage}</p>}
    </header>
  );
}
