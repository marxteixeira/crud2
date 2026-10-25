import axios from "axios";
import type { SupabaseClient } from "@supabase/supabase-js";

export interface HealthResponse {
  status: string;
  timestamp: string;
}

const apiUrl: string = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5013";

const http = axios.create({ baseURL: apiUrl, timeout: 5000 });

export async function getAccessToken(client: SupabaseClient): Promise<string> {
  const { data, error } = await client.auth.getSession();
  if (error) {
    throw error;
  }
  const accessToken = data.session?.access_token;
  if (!accessToken) {
    throw new Error("A sessão autenticada não está disponível.");
  }
  return accessToken;
}

export async function requestWithSession<T>(
  client: SupabaseClient,
  method: "GET" | "POST" | "PATCH" | "PUT",
  path: string,
  body?: unknown,
): Promise<T> {
  const accessToken = await getAccessToken(client);
  const response = await http.request<T>({
    method,
    url: path,
    data: body,
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  return response.data;
}

export async function getHealth(): Promise<HealthResponse> {
  const response = await http.get<HealthResponse>("/health");
  return response.data;
}

export function describeApiError(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error) && (error.response?.status ?? 0) >= 500) {
    const data: unknown = error.response?.data;
    const traceId =
      typeof data === "object" && data !== null && "traceId" in data
        ? (data as { traceId?: unknown }).traceId
        : undefined;
    if (typeof traceId === "string" && traceId.length > 0) {
      return `${fallback} Erro interno no servidor (código de rastreio: ${traceId}).`;
    }
  }
  return fallback;
}
