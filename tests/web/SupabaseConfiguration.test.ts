import { beforeEach, describe, expect, it, vi } from "vitest";
import { createBrowserClient } from "@supabase/ssr";

vi.mock("@supabase/ssr", () => ({
  createBrowserClient: vi.fn(() => ({ auth: {} })),
}));

describe("Supabase public configuration", () => {
  beforeEach(() => {
    vi.resetModules();
    vi.mocked(createBrowserClient).mockClear();
    vi.stubEnv("NEXT_PUBLIC_SUPABASE_URL", "https://example.supabase.co");
    vi.stubEnv("NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY", "test-publishable-key");
  });

  it("requires the project URL and publishable key", async () => {
    vi.stubEnv("NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY", "");
    const { getSupabaseBrowserClient } = await import("@/lib/supabase/client");

    expect(() => getSupabaseBrowserClient()).toThrow(
      "A configuração pública do Supabase está ausente.",
    );
    expect(createBrowserClient).not.toHaveBeenCalled();
  });

  it("initializes the client with only public configuration", async () => {
    const { getSupabaseBrowserClient } = await import("@/lib/supabase/client");

    getSupabaseBrowserClient();

    expect(createBrowserClient).toHaveBeenCalledWith(
      "https://example.supabase.co",
      "test-publishable-key",
    );
  });
});
