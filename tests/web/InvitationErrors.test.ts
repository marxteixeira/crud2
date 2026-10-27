import { describe, expect, it } from "vitest";
import {
  classifyInvitationError,
  invitationMessages,
  mapInvitationError,
  parseAuthErrorFromUrl,
} from "@/lib/auth/invitation";

describe("invitation errors", () => {
  it("lê o erro do fragmento retornado pelo provedor", () => {
    const result = parseAuthErrorFromUrl(
      "http://localhost:3000/login#error=access_denied&error_code=otp_expired&error_description=Email+link+is+invalid+or+has+expired&sb=",
    );

    expect(result).toEqual({ code: "otp_expired", description: "Email link is invalid or has expired" });
  });

  it("ignora URLs sem erro de convite e parâmetros internos de erro", () => {
    expect(parseAuthErrorFromUrl("http://localhost:3000/login")).toBeNull();
    expect(parseAuthErrorFromUrl("http://localhost:3000/login?error=session")).toBeNull();
  });

  it("mapeia códigos para mensagens em português", () => {
    expect(mapInvitationError("otp_expired")).toBe(invitationMessages.expired);
    expect(mapInvitationError("weak_password")).toBe(invitationMessages.password);
    expect(mapInvitationError("outro")).toBe(invitationMessages.unknown);
  });

  it("classifica erros de sessão, senha e serviço", () => {
    expect(classifyInvitationError({ name: "AuthSessionMissingError" })).toBe("expired");
    expect(classifyInvitationError({ code: "same_password", status: 422 })).toBe("password");
    expect(classifyInvitationError({ name: "AuthRetryableFetchError", status: 0 })).toBe("service");
    expect(classifyInvitationError({ status: 503 })).toBe("service");
  });
});
