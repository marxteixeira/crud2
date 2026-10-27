export type InvitationErrorKind = "expired" | "password" | "service" | "unknown";

export interface AuthUrlError {
  code: string;
  description: string | null;
}

export const invitationMessages: Record<InvitationErrorKind, string> = {
  expired: "O convite é inválido ou expirou. Solicite um novo convite ao Administrador.",
  password: "A senha não foi aceita. Escolha outra senha com pelo menos 8 caracteres.",
  service: "O serviço de autenticação está indisponível. Tente novamente em instantes.",
  unknown: "Não foi possível concluir a ativação. Tente novamente ou solicite um novo convite ao Administrador.",
};

const expiredCodes = new Set([
  "otp_expired",
  "access_denied",
  "invalid_token",
  "session_not_found",
  "session_missing",
  "invalid_grant",
]);

const passwordCodes = new Set(["weak_password", "same_password", "validation_failed"]);

interface AuthErrorLike {
  code?: string;
  status?: number;
  name?: string;
}

export function classifyInvitationError(error: AuthErrorLike | null | undefined): InvitationErrorKind {
  if (!error) return "unknown";
  const code = error.code ?? "";
  if (expiredCodes.has(code) || error.name === "AuthSessionMissingError") return "expired";
  if (passwordCodes.has(code)) return "password";
  if (error.name === "AuthRetryableFetchError" || (error.status !== undefined && error.status >= 500)) {
    return "service";
  }
  return "unknown";
}

export function mapInvitationError(code: string | null | undefined): string {
  if (!code) return invitationMessages.unknown;
  if (expiredCodes.has(code)) return invitationMessages.expired;
  if (passwordCodes.has(code)) return invitationMessages.password;
  return invitationMessages.unknown;
}

export function parseAuthErrorFromUrl(url: string): AuthUrlError | null {
  const parsed = new URL(url, "http://localhost");
  const fragment = new URLSearchParams(parsed.hash.replace(/^#/, ""));
  const code = fragment.get("error_code") ?? parsed.searchParams.get("error_code") ?? fragment.get("error");
  if (!code) return null;
  return {
    code,
    description: fragment.get("error_description") ?? parsed.searchParams.get("error_description"),
  };
}
