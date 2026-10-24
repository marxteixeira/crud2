import { createServerClient, type CookieOptions } from "@supabase/ssr";
import { NextResponse, type NextRequest } from "next/server";

export async function middleware(request: NextRequest): Promise<NextResponse> {
  const response = NextResponse.next({ request });
  const redirectWithSessionCookies = (destination: URL): NextResponse => {
    const redirect = NextResponse.redirect(destination);
    for (const cookie of response.cookies.getAll()) {
      redirect.cookies.set(cookie);
    }
    return redirect;
  };
  const supabaseUrl = process.env.NEXT_PUBLIC_SUPABASE_URL;
  const publishableKey = process.env.NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY;
  const apiUrl = process.env.NEXT_PUBLIC_API_URL;
  if (!supabaseUrl || !publishableKey || !apiUrl) {
    return redirectWithSessionCookies(new URL("/login?error=configuration", request.url));
  }

  const supabase = createServerClient(supabaseUrl, publishableKey, {
    cookies: {
      getAll: () => request.cookies.getAll(),
      setAll: (values: { name: string; value: string; options: CookieOptions }[]) => values.forEach(({ name, value, options }) => {
        request.cookies.set(name, value);
        response.cookies.set(name, value, options);
      }),
    },
  });
  const { data, error } = await supabase.auth.getUser();
  if (error || !data.user) {
    await supabase.auth.signOut();
    return redirectWithSessionCookies(new URL("/login", request.url));
  }

  const { data: sessionData } = await supabase.auth.getSession();
  const accessToken = sessionData.session?.access_token;
  if (!accessToken) {
    await supabase.auth.signOut();
    return redirectWithSessionCookies(new URL("/login", request.url));
  }

  try {
    const validation = await fetch(new URL("/api/auth/me", apiUrl), {
      headers: { Authorization: `Bearer ${accessToken}` },
      cache: "no-store",
    });
    if (!validation.ok) {
      await supabase.auth.signOut();
      return redirectWithSessionCookies(new URL("/login?error=session", request.url));
    }

    if (request.nextUrl.pathname === "/users" || request.nextUrl.pathname.startsWith("/users/")) {
      const profile: unknown = await validation.json();
      if (!hasAdministratorRole(profile)) {
        return redirectWithSessionCookies(new URL("/", request.url));
      }
    }
    return response;
  } catch {
    await supabase.auth.signOut();
    return redirectWithSessionCookies(new URL("/login?error=session", request.url));
  }
}

function hasAdministratorRole(value: unknown): boolean {
  return typeof value === "object"
    && value !== null
    && "role" in value
    && value.role === "Administrador";
}

export const config = {
  matcher: ["/((?!api|login|activate|auth/callback|_next/static|_next/image|favicon.ico).*)"],
};
