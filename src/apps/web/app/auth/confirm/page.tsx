"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useState } from "react";
import { classifyInvitationError, invitationMessages } from "@/lib/auth/invitation";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";

export default function ConfirmInvitationPage(): JSX.Element {
  return (
    <Suspense fallback={null}>
      <ConfirmInvitation />
    </Suspense>
  );
}

function ConfirmInvitation(): JSX.Element {
  const router = useRouter();
  const searchParams = useSearchParams();
  const tokenHash = searchParams.get("token_hash");
  const type = searchParams.get("type");
  const validLink = Boolean(tokenHash) && type === "invite";
  const [pending, setPending] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(
    validLink ? null : invitationMessages.expired,
  );

  async function confirm(): Promise<void> {
    if (!tokenHash || !validLink) return;
    setPending(true);
    setErrorMessage(null);
    try {
      const { error } = await getSupabaseBrowserClient().auth.verifyOtp({ token_hash: tokenHash, type: "invite" });
      if (error) {
        setErrorMessage(invitationMessages[classifyInvitationError(error)]);
        return;
      }
      router.replace("/activate");
    } catch {
      setErrorMessage(invitationMessages.service);
    } finally {
      setPending(false);
    }
  }

  return (
    <main className="mx-auto flex min-h-screen w-full max-w-md items-center px-6">
      <section aria-labelledby="confirm-heading" className="w-full rounded-lg border p-6 shadow-sm">
        <h1 id="confirm-heading" className="mb-4 text-2xl font-bold">Confirmar convite</h1>
        {validLink && <p className="mb-4 text-sm">Clique em continuar para definir a senha da sua conta.</p>}
        {errorMessage && <p role="alert" className="mb-4 text-sm text-red-700">{errorMessage}</p>}
        {validLink && (
          <button type="button" onClick={() => void confirm()} disabled={pending}
            className="w-full rounded bg-blue-700 p-2 font-medium text-white disabled:opacity-60">
            {pending ? "Verificando..." : "Continuar"}
          </button>
        )}
      </section>
    </main>
  );
}
