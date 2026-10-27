"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { classifyInvitationError, invitationMessages } from "@/lib/auth/invitation";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { requestWithSession } from "@/lib/api";

const activationSchema = z.object({
  password: z.string().min(8, "A senha deve ter pelo menos 8 caracteres."),
  confirmation: z.string(),
}).refine((values) => values.password === values.confirmation, {
  path: ["confirmation"],
  message: "As senhas não correspondem.",
});

type ActivationValues = z.infer<typeof activationSchema>;
type SessionState = "checking" | "ready" | "invalid";

export default function ActivatePage(): JSX.Element {
  const router = useRouter();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [sessionState, setSessionState] = useState<SessionState>("checking");
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ActivationValues>({ resolver: zodResolver(activationSchema) });

  useEffect(() => {
    let active = true;
    getSupabaseBrowserClient().auth.getSession()
      .then(({ data }) => {
        if (active) setSessionState(data.session ? "ready" : "invalid");
      })
      .catch(() => {
        if (active) setSessionState("invalid");
      });
    return () => {
      active = false;
    };
  }, []);

  async function submit(values: ActivationValues): Promise<void> {
    setErrorMessage(null);
    const supabase = getSupabaseBrowserClient();
    const { error: passwordError } = await supabase.auth.updateUser({ password: values.password });
    if (passwordError) {
      const kind = classifyInvitationError(passwordError);
      console.warn("Activation failed", { code: passwordError.code ?? passwordError.name, kind });
      if (kind === "expired") setSessionState("invalid");
      setErrorMessage(invitationMessages[kind]);
      return;
    }

    try {
      await requestWithSession(supabase, "POST", "/api/auth/activation");
      router.replace("/");
    } catch {
      console.warn("Activation confirmation failed", { kind: "service" });
      setErrorMessage(invitationMessages.unknown);
    }
  }

  if (sessionState === "checking") {
    return (
      <main className="mx-auto flex min-h-screen w-full max-w-md items-center px-6">
        <p role="status">Verificando convite...</p>
      </main>
    );
  }

  if (sessionState === "invalid") {
    return (
      <main className="mx-auto flex min-h-screen w-full max-w-md items-center px-6">
        <section aria-labelledby="activation-heading" className="w-full rounded-lg border p-6 shadow-sm">
          <h1 id="activation-heading" className="mb-4 text-2xl font-bold">Ative sua conta</h1>
          <p role="alert" className="text-sm text-red-700">{errorMessage ?? invitationMessages.expired}</p>
        </section>
      </main>
    );
  }

  return (
    <main className="mx-auto flex min-h-screen w-full max-w-md items-center px-6">
      <section aria-labelledby="activation-heading" className="w-full rounded-lg border p-6 shadow-sm">
        <h1 id="activation-heading" className="mb-6 text-2xl font-bold">Ative sua conta</h1>
        <form className="space-y-4" onSubmit={handleSubmit(submit)} noValidate>
          <div>
            <label className="mb-1 block font-medium" htmlFor="password">Crie uma senha</label>
            <input id="password" autoComplete="new-password" type="password" {...register("password")}
              aria-invalid={Boolean(errors.password)} aria-describedby={errors.password ? "password-error" : undefined}
              className="w-full rounded border p-2" />
            {errors.password && <p id="password-error" role="alert" className="text-sm text-red-700">{errors.password.message}</p>}
          </div>
          <div>
            <label className="mb-1 block font-medium" htmlFor="confirmation">Confirme sua senha</label>
            <input id="confirmation" autoComplete="new-password" type="password" {...register("confirmation")}
              aria-invalid={Boolean(errors.confirmation)} aria-describedby={errors.confirmation ? "confirmation-error" : undefined}
              className="w-full rounded border p-2" />
            {errors.confirmation && <p id="confirmation-error" role="alert" className="text-sm text-red-700">{errors.confirmation.message}</p>}
          </div>
          {errorMessage && <p role="alert" className="text-sm text-red-700">{errorMessage}</p>}
          <button type="submit" disabled={isSubmitting} className="w-full rounded bg-blue-700 p-2 font-medium text-white disabled:opacity-60">
            {isSubmitting ? "Ativando..." : "Definir senha e ativar"}
          </button>
        </form>
      </section>
    </main>
  );
}
