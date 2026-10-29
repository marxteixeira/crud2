"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";
import { classifyInvitationError, invitationMessages } from "@/lib/auth/invitation";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import { requestWithSession } from "@/lib/api";
import AuthLayout from "@/components/AuthLayout";
import { InputField } from "@/components/ui/Field";
import Button from "@/components/ui/Button";

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
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ActivationValues>({
    resolver: zodResolver(activationSchema),
    defaultValues: { password: "", confirmation: "" },
  });

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
    return <AuthLayout><p role="status" className="rounded-lg bg-surface-container-low p-space-lg text-body-md text-on-surface-variant">Verificando convite...</p></AuthLayout>;
  }

  if (sessionState === "invalid") {
    return (
      <AuthLayout>
        <section aria-labelledby="activation-heading">
          <p className="mb-space-sm text-label-md text-primary">Convite de acesso</p>
          <h1 id="activation-heading" className="font-headline text-headline-lg-mobile text-on-surface md:text-headline-lg">Ative sua conta</h1>
          <p role="alert" className="mt-space-md rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">{errorMessage ?? invitationMessages.expired}</p>
        </section>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout>
      <section aria-labelledby="activation-heading">
        <p className="mb-space-sm text-label-md text-primary">Convite de acesso</p>
        <h1 id="activation-heading" className="font-headline text-headline-lg-mobile text-on-surface md:text-headline-lg">Ative sua conta</h1>
        <p className="mt-2 text-body-md text-on-surface-variant">Crie uma senha para concluir a ativação do seu acesso.</p>
        <form className="mt-space-xl space-y-space-md" onSubmit={handleSubmit(submit)} noValidate>
          <Controller name="password" control={control} render={({ field }) => <InputField label="Crie uma senha" autoComplete="new-password" type="password" name={field.name} value={field.value} onChange={field.onChange} onBlur={field.onBlur} error={errors.password?.message} />} />
          <Controller name="confirmation" control={control} render={({ field }) => <InputField label="Confirme sua senha" autoComplete="new-password" type="password" name={field.name} value={field.value} onChange={field.onChange} onBlur={field.onBlur} error={errors.confirmation?.message} />} />
          {errorMessage && <p role="alert" className="rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">{errorMessage}</p>}
          <Button type="submit" disabled={isSubmitting} className="w-full py-3">
            {isSubmitting ? "Ativando..." : "Definir senha e ativar"}
          </Button>
        </form>
      </section>
    </AuthLayout>
  );
}
