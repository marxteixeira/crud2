import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ActivatePage from "@/app/activate/page";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";
import LoginPage from "@/app/login/page";

const mocks = vi.hoisted(() => ({
  signInWithPassword: vi.fn(),
  updateUser: vi.fn(),
  signOut: vi.fn(),
  requestWithSession: vi.fn(),
  replace: vi.fn(),
  refresh: vi.fn(),
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: mocks.replace, refresh: mocks.refresh }),
}));
vi.mock("@/lib/api", () => ({ requestWithSession: mocks.requestWithSession }));
vi.mock("@/lib/supabase/client", () => ({
  getSupabaseBrowserClient: () => ({
    auth: {
      signInWithPassword: mocks.signInWithPassword,
      updateUser: mocks.updateUser,
      signOut: mocks.signOut,
    },
  }),
}));

describe("AuthFlow", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mocks.signInWithPassword.mockResolvedValue({ error: null });
    mocks.updateUser.mockResolvedValue({ error: null });
    mocks.signOut.mockResolvedValue({ error: null });
    mocks.requestWithSession.mockResolvedValue({ id: "account-id" });
  });

  it("valida e-mail e senha antes de autenticar", async () => {
    render(<LoginPage />);
    fireEvent.change(screen.getByLabelText("E-mail"), { target: { value: "invalido" } });
    fireEvent.click(screen.getByRole("button", { name: "Entrar" }));

    expect(await screen.findByText("Informe um e-mail válido.")).toBeInTheDocument();
    expect(mocks.signInWithPassword).not.toHaveBeenCalled();
  });

  it("limpa sessão de conta recusada pela API com mensagem genérica", async () => {
    mocks.requestWithSession.mockRejectedValue(new Error("inactive account"));
    render(<LoginPage />);
    fireEvent.change(screen.getByLabelText("E-mail"), { target: { value: "operator@example.com" } });
    fireEvent.change(screen.getByLabelText("Senha"), { target: { value: "correct-password" } });
    fireEvent.click(screen.getByRole("button", { name: "Entrar" }));

    expect(await screen.findByText(
      "Não foi possível entrar. Verifique suas credenciais e o status da sua conta.",
    )).toBeInTheDocument();
    expect(mocks.signOut).toHaveBeenCalledOnce();
    expect(mocks.replace).not.toHaveBeenCalled();
  });

  it("valida conta na API antes de navegar após login", async () => {
    render(<LoginPage />);
    fireEvent.change(screen.getByLabelText("E-mail"), { target: { value: "user@example.com" } });
    fireEvent.change(screen.getByLabelText("Senha"), { target: { value: "password123" } });
    fireEvent.click(screen.getByRole("button", { name: "Entrar" }));

    await waitFor(() => expect(mocks.replace).toHaveBeenCalledWith("/"));
    expect(mocks.requestWithSession).toHaveBeenCalledWith(
      expect.anything(),
      "GET",
      "/api/auth/me",
    );
    expect(mocks.signOut).not.toHaveBeenCalled();
  });

  it("define senha e ativa o convite confirmado pela API", async () => {
    render(<ActivatePage />);
    fireEvent.change(screen.getByLabelText("Crie uma senha"), { target: { value: "secure-password" } });
    fireEvent.change(screen.getByLabelText("Confirme sua senha"), { target: { value: "secure-password" } });
    fireEvent.click(screen.getByRole("button", { name: "Definir senha e ativar" }));

    await waitFor(() => expect(mocks.replace).toHaveBeenCalledWith("/"));
    expect(mocks.updateUser).toHaveBeenCalledWith({ password: "secure-password" });
    expect(mocks.requestWithSession).toHaveBeenCalledWith(
      expect.anything(),
      "POST",
      "/api/auth/activation",
    );
  });

  it("revoga a sessão na API antes de encerrar a sessão Supabase", async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    mocks.requestWithSession.mockResolvedValue({
      displayName: "Test User",
      email: "user@example.com",
      role: "Operador",
    });
    render(
      <QueryClientProvider client={queryClient}>
        <AuthenticatedNavigation />
      </QueryClientProvider>,
    );
    fireEvent.click(await screen.findByRole("button", { name: "Sair" }));

    await waitFor(() => expect(mocks.replace).toHaveBeenCalledWith("/login"));
    expect(mocks.requestWithSession).toHaveBeenCalledWith(
      expect.anything(),
      "POST",
      "/api/auth/logout",
    );
    expect(mocks.signOut).toHaveBeenCalledOnce();
  });
});
