import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import AuthenticatedNavigation from "@/components/AuthenticatedNavigation";

const mocks = vi.hoisted(() => ({
  requestWithSession: vi.fn(),
  signOut: vi.fn(),
  replace: vi.fn(),
  refresh: vi.fn(),
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: mocks.replace, refresh: mocks.refresh }),
}));
vi.mock("@/lib/api", () => ({ requestWithSession: mocks.requestWithSession }));
vi.mock("@/lib/supabase/client", () => ({
  getSupabaseBrowserClient: () => ({ auth: { signOut: mocks.signOut } }),
}));

function renderNavigation() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <AuthenticatedNavigation />
    </QueryClientProvider>,
  );
}

describe("RoleAccess", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mocks.signOut.mockResolvedValue({ error: null });
  });

  it("shows user-management navigation only for an Administrator", async () => {
    mocks.requestWithSession.mockResolvedValue({
      displayName: "Admin",
      email: "admin@example.com",
      role: "Administrador",
    });
    const adminView = renderNavigation();
    expect(await screen.findByRole("link", { name: "Usuários" })).toBeInTheDocument();
    adminView.unmount();

    mocks.requestWithSession.mockResolvedValue({
      displayName: "Operator",
      email: "operator@example.com",
      role: "Operador",
    });
    renderNavigation();
    await screen.findByText("Operator");
    expect(screen.queryByRole("link", { name: "Usuários" })).not.toBeInTheDocument();
  });

  it("clears and redirects a session rejected by the API", async () => {
    mocks.requestWithSession.mockRejectedValue(new Error("session revoked"));
    renderNavigation();

    await waitFor(() => expect(mocks.replace).toHaveBeenCalledWith("/login?error=session"));

    expect(mocks.signOut).toHaveBeenCalledOnce();
    expect(mocks.refresh).toHaveBeenCalledOnce();
  });
});
