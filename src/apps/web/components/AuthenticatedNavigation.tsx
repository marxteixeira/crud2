"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { getCurrentAccount } from "@/lib/auth";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { requestWithSession } from "@/lib/api";

const navItems = [
  { href: "/products", label: "Produtos" },
  { href: "/locations", label: "Localizações" },
  { href: "/suppliers", label: "Fornecedores" },
];

export default function AuthenticatedNavigation(): JSX.Element {
  const router = useRouter();
  const pathname = usePathname() ?? "";
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
    <header
      data-app-sidebar
      className="flex flex-wrap items-center justify-between gap-space-md bg-surface-container-lowest px-space-lg py-space-md shadow-[0_1px_8px_rgba(0,0,0,0.04)] md:fixed md:inset-y-0 md:left-0 md:z-50 md:w-64 md:flex-col md:flex-nowrap md:items-stretch md:justify-between md:px-space-md md:py-0"
    >
      <div className="flex flex-wrap items-center gap-space-md md:flex-col md:items-stretch md:gap-0">
        <Link href="/" className="flex items-center gap-space-sm md:h-16 md:px-space-sm">
          <span className="flex flex-col">
            <span className="font-title text-title-md tracking-tight text-primary">StockSystem</span>
            <span className="text-caption text-secondary">Sistema de Estoque</span>
          </span>
        </Link>
        <nav aria-label="Navegação principal" className="flex flex-wrap gap-space-xs md:flex-col md:py-space-sm">
          <p className="hidden px-space-sm text-label-sm uppercase tracking-wider text-on-surface-variant md:block">Operações</p>
          {navItems.map((item) => {
            const active = pathname === item.href || pathname.startsWith(`${item.href}/`);
            return (
              <Link
                key={item.href}
                href={item.href}
                aria-current={active ? "page" : undefined}
                className={`rounded-lg px-space-md py-2.5 text-body-md transition-colors ${
                  active
                    ? "bg-secondary-container font-title text-title-md text-on-secondary-fixed"
                    : "text-on-surface-variant hover:bg-surface-container-low hover:text-on-surface"
                }`}
              >
                {item.label}
              </Link>
            );
          })}
          {accountQuery.data?.role === "Administrador" && (
            <Link
              href="/users"
              aria-current={pathname === "/users" || pathname.startsWith("/users/") ? "page" : undefined}
              className={`rounded-lg px-space-md py-2.5 text-body-md transition-colors ${
                pathname === "/users" || pathname.startsWith("/users/")
                  ? "bg-secondary-container font-title text-title-md text-on-secondary-fixed"
                  : "text-on-surface-variant hover:bg-surface-container-low hover:text-on-surface"
              }`}
            >
              Usuários
            </Link>
          )}
        </nav>
      </div>
      <div className="flex items-center gap-space-md md:flex-col md:items-stretch md:gap-space-sm md:py-space-md">
        <span aria-live="polite" className="truncate text-label-md text-on-surface-variant md:px-space-sm">
          {accountQuery.data?.displayName ?? accountQuery.data?.email ?? "Carregando sessão..."}
        </span>
        <button
          type="button"
          onClick={() => void logout()}
          className="rounded-lg px-space-md py-1.5 text-label-md text-on-surface ring-1 ring-outline-variant transition-colors hover:bg-surface-container-low"
        >
          Sair
        </button>
      </div>
      {errorMessage && <p role="alert" className="w-full text-body-sm text-error">{errorMessage}</p>}
    </header>
  );
}
