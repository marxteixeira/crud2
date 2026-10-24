import type { SupabaseClient } from "@supabase/supabase-js";
import { requestWithSession } from "@/lib/api";

export type UserRole = "Administrador" | "Operador";
export type UserStatus = "ConvitePendente" | "Ativa" | "Inativa";

export interface UserAccount {
  id: string;
  email: string;
  displayName: string;
  pendingEmail: string | null;
  role: UserRole;
  status: UserStatus;
}

export interface PaginatedUsers {
  items: UserAccount[];
  page: number;
  pageSize: number;
  total: number;
}

export interface UserInput {
  email: string;
  displayName: string;
  role: UserRole;
}

export interface UserUpdateInput {
  email?: string;
  displayName?: string;
  role?: UserRole;
}

export async function listUsers(
  client: SupabaseClient,
  filters: { search: string; status: string; page: number; pageSize: number },
): Promise<PaginatedUsers> {
  const query = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
  });
  if (filters.search) query.set("search", filters.search);
  if (filters.status) query.set("status", filters.status);
  return requestWithSession<PaginatedUsers>(client, "GET", `/api/users?${query.toString()}`);
}

export function getUser(client: SupabaseClient, userId: string): Promise<UserAccount> {
  return requestWithSession<UserAccount>(client, "GET", `/api/users/${userId}`);
}

export function inviteUser(client: SupabaseClient, input: UserInput): Promise<UserAccount> {
  return requestWithSession<UserAccount>(client, "POST", "/api/users", input);
}

export function updateUser(
  client: SupabaseClient,
  userId: string,
  input: UserUpdateInput,
): Promise<UserAccount> {
  return requestWithSession<UserAccount>(client, "PATCH", `/api/users/${userId}`, input);
}

export function setUserStatus(
  client: SupabaseClient,
  userId: string,
  status: Exclude<UserStatus, "ConvitePendente">,
): Promise<void> {
  return requestWithSession<void>(client, "PUT", `/api/users/${userId}/activation`, { status });
}
