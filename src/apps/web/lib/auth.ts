import type { SupabaseClient } from "@supabase/supabase-js";
import { requestWithSession } from "@/lib/api";

export interface CurrentAccount {
  id: string;
  email: string;
  displayName: string;
  role: "Administrador" | "Operador";
  status: "ConvitePendente" | "Ativa" | "Inativa";
}

export function getCurrentAccount(client: SupabaseClient): Promise<CurrentAccount> {
  return requestWithSession<CurrentAccount>(client, "GET", "/api/auth/me");
}
