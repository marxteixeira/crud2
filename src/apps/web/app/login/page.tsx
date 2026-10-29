"use client";

import Link from "next/link";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";
import { requestWithSession } from "@/lib/api";
import { mapInvitationError, parseAuthErrorFromUrl } from "@/lib/auth/invitation";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";
import AuthLayout from "@/components/AuthLayout";
import { InputField } from "@/components/ui/Field";
import Button from "@/components/ui/Button";

const loginSchema = z.object({
  email: z.string().trim().email("Informe um e-mail válido."),
  password: z.string().min(1, "Informe sua senha."),
});

type LoginValues = z.infer<typeof loginSchema>;

export default function LoginPage(): JSX.Element {
  const router = useRouter();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  useEffect(() => {
    const urlError = parseAuthErrorFromUrl(window.location.href);
    if (urlError) {
      setErrorMessage(mapInvitationError(urlError.code));
    }
  }, []);

  async function submit(values: LoginValues): Promise<void> {
    setErrorMessage(null);
    const supabase = getSupabaseBrowserClient();
    const { error } = await supabase.auth.signInWithPassword(values);
    if (error) {
      setErrorMessage("Não foi possível entrar. Verifique suas credenciais e o status da sua conta.");
      return;
    }

    try {
      await requestWithSession(supabase, "GET", "/api/auth/me");
      router.replace("/");
    } catch {
      await supabase.auth.signOut();
      setErrorMessage("Não foi possível entrar. Verifique suas credenciais e o status da sua conta.");
    }
  }

  return (
    <AuthLayout>
      <section aria-labelledby="login-heading" className="w-full">
        <p className="mb-space-sm text-label-md text-primary">Acesso seguro ao sistema</p>
        <h1 id="login-heading" className="font-headline text-headline-lg-mobile text-on-surface md:text-headline-lg">Entrar no sistema</h1>
        <p className="mt-2 text-body-md text-on-surface-variant">Digite suas credenciais para acessar a gestão do seu estoque.</p>
        <form className="mt-space-xl space-y-space-md" onSubmit={handleSubmit(submit)} noValidate>
          <Controller name="email" control={control} render={({ field }) => <InputField label="E-mail" autoComplete="username" type="email" name={field.name} value={field.value} onChange={field.onChange} onBlur={field.onBlur} error={errors.email?.message} />} />
          <Controller name="password" control={control} render={({ field }) => <InputField label="Senha" autoComplete="current-password" type="password" name={field.name} value={field.value} onChange={field.onChange} onBlur={field.onBlur} error={errors.password?.message} />} />
          {errorMessage && <p role="alert" className="rounded-lg bg-error-container p-space-md text-body-sm text-on-error-container">{errorMessage}</p>}
          <Button type="submit" disabled={isSubmitting} className="w-full py-3">
            {isSubmitting ? "Entrando..." : "Entrar"}
          </Button>
        </form>
        <p className="mt-space-lg rounded-lg bg-surface-container-low p-space-md text-body-sm text-on-surface-variant">
          Recebeu um convite? <Link className="font-title text-primary underline-offset-4 hover:underline" href="/activate">Ative sua conta</Link>.
        </p>
      </section>
    </AuthLayout>
  );
}
