import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ConfirmInvitationPage from "@/app/auth/confirm/page";

const mocks = vi.hoisted(() => ({
  verifyOtp: vi.fn(),
  replace: vi.fn(),
  search: "token_hash=hash-123&type=invite",
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: mocks.replace }),
  useSearchParams: () => new URLSearchParams(mocks.search),
}));
vi.mock("@/lib/supabase/client", () => ({
  getSupabaseBrowserClient: () => ({ auth: { verifyOtp: mocks.verifyOtp } }),
}));

describe("ConfirmInvitationPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mocks.search = "token_hash=hash-123&type=invite";
    mocks.verifyOtp.mockResolvedValue({ error: null });
  });

  it("só verifica o token após o clique em Continuar", async () => {
    render(<ConfirmInvitationPage />);
    expect(mocks.verifyOtp).not.toHaveBeenCalled();

    fireEvent.click(screen.getByRole("button", { name: "Continuar" }));

    await waitFor(() => expect(mocks.replace).toHaveBeenCalledWith("/activate"));
    expect(mocks.verifyOtp).toHaveBeenCalledWith({ token_hash: "hash-123", type: "invite" });
  });

  it("rejeita tipos diferentes de convite sem chamar o provedor", () => {
    mocks.search = "token_hash=hash-123&type=recovery";
    render(<ConfirmInvitationPage />);

    expect(screen.getByRole("alert")).toHaveTextContent("O convite é inválido ou expirou.");
    expect(screen.queryByRole("button", { name: "Continuar" })).not.toBeInTheDocument();
    expect(mocks.verifyOtp).not.toHaveBeenCalled();
  });

  it("mostra mensagem amigável quando o convite expirou", async () => {
    mocks.verifyOtp.mockResolvedValue({ error: { code: "otp_expired", status: 403, name: "AuthApiError" } });
    render(<ConfirmInvitationPage />);

    fireEvent.click(screen.getByRole("button", { name: "Continuar" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("O convite é inválido ou expirou.");
    expect(mocks.replace).not.toHaveBeenCalled();
  });
});
