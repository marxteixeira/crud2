"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useState } from "react";
import { classifyInvitationError, invitationMessages } from "@/lib/auth/invitation";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import AuthLayout from "@/components/AuthLayout";
import Button from "@/components/ui/Button";

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
    <AuthLayout>
      <section aria-labelledby="confirm-heading">
        <p className="mb-space-sm text-label-md text-primary">Convite de acesso</p>
        <h1 id="confirm-heading" className="font-headline text-headline-lg-mobile text-on-surface md:text-headline-lg">Confirmar convite</h1>
        {validLink && <p className="mt-2 text-body-md text-on-surface-variant">Confirme seu convite para continuar e definir a senha da sua conta.</p>}
        {errorMessage && <p role="alert" className="mt-space-md rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">{errorMessage}</p>}
        {validLink && (
          <Button type="button" onClick={() => void confirm()} disabled={pending} className="mt-space-xl w-full py-3">
            {pending ? "Verificando..." : "Continuar"}
          </Button>
        )}
      </section>
    </AuthLayout>
  );
}
