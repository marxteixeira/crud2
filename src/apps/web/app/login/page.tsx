"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { requestWithSession } from "@/lib/api";
import { mapInvitationError, parseAuthErrorFromUrl } from "@/lib/auth/invitation";
import { getSupabaseBrowserClient } from "@/lib/supabase/client";

const loginSchema = z.object({
  email: z.string().trim().email("Informe um e-mail válido."),
  password: z.string().min(1, "Informe sua senha."),
});

type LoginValues = z.infer<typeof loginSchema>;

export default function LoginPage(): JSX.Element {
  const router = useRouter();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginValues>({ resolver: zodResolver(loginSchema) });

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
    <main className="mx-auto flex min-h-screen w-full max-w-md items-center px-6">
      <section aria-labelledby="login-heading" className="w-full rounded-lg border p-6 shadow-sm">
        <h1 id="login-heading" className="mb-6 text-2xl font-bold">Entrar no sistema</h1>
        <form className="space-y-4" onSubmit={handleSubmit(submit)} noValidate>
          <div>
            <label className="mb-1 block font-medium" htmlFor="email">E-mail</label>
            <input id="email" autoComplete="username" type="email" {...register("email")}
              aria-invalid={Boolean(errors.email)} aria-describedby={errors.email ? "email-error" : undefined}
              className="w-full rounded border p-2" />
            {errors.email && <p id="email-error" role="alert" className="text-sm text-red-700">{errors.email.message}</p>}
          </div>
          <div>
            <label className="mb-1 block font-medium" htmlFor="password">Senha</label>
            <input id="password" autoComplete="current-password" type="password" {...register("password")}
              aria-invalid={Boolean(errors.password)} aria-describedby={errors.password ? "password-error" : undefined}
              className="w-full rounded border p-2" />
            {errors.password && <p id="password-error" role="alert" className="text-sm text-red-700">{errors.password.message}</p>}
          </div>
          {errorMessage && <p role="alert" className="text-sm text-red-700">{errorMessage}</p>}
          <button type="submit" disabled={isSubmitting} className="w-full rounded bg-blue-700 p-2 font-medium text-white disabled:opacity-60">
            {isSubmitting ? "Entrando..." : "Entrar"}
          </button>
        </form>
        <p className="mt-4 text-sm">Recebeu um convite? <Link className="underline" href="/activate">Ative sua conta</Link>.</p>
      </section>
    </main>
  );
}
