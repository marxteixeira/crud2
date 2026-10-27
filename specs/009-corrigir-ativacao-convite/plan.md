# Implementation Plan: Corrigir ativação de conta por convite

**Branch**: `009-corrigir-ativacao-convite` | **Date**: 2026-10-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/009-corrigir-ativacao-convite/spec.md`

## Summary

Causa raiz provável (ver [research.md](./research.md)): o convite é criado em `SupabaseAdminClient.InviteAsync` sem `redirect_to` e o link do e-mail usa o fluxo implícito (tokens no fragmento `#`). O Supabase redireciona para o Site URL (`/login#error=...otp_expired`); `/auth/callback` espera `?code=` (PKCE) e nunca recebe. Na tela `/activate` não existe sessão, então `supabase.auth.updateUser` falha localmente (sem requisição de rede) e a página mostra a mensagem genérica. Além disso, links de uso único podem ser consumidos por pré-visualização de e-mail (`otp_expired`).

Correção: (1) o convite passa a apontar para uma rota própria `/auth/confirm` do web, com `token_hash` no e-mail; (2) a rota exibe uma página com botão "Continuar" e só então verifica o token (imune a pré-visualização automática) e abre sessão; (3) `/activate` valida a sessão antes do envio e mapeia erros reais; `/login` interpreta erros do fragmento/query; (4) administrador pode reenviar convite a usuário pendente; (5) URL de retorno configurável por ambiente.

## Technical Context

**Language/Version**: C# / .NET 8; TypeScript (Next.js 14)

**Primary Dependencies**: Supabase Auth via `@supabase/ssr` e `SupabaseAdminClient`, ASP.NET Core, FluentValidation, Zod, xUnit, Vitest

**Storage**: PostgreSQL (Supabase); sem novas tabelas (reenvio usa o estado pendente existente)

**Testing**: xUnit em `tests/api/`, Vitest + Testing Library em `tests/web/`

**Target Platform**: web local `localhost:3000`, API local `localhost:5013`

**Project Type**: web-service + web app (monorepo)

**Performance Goals**: ativação concluída em < 2 min após abrir o e-mail (SC-002)

**Constraints**: sem segredos versionados; template de e-mail do Supabase é configuração externa (documentada no quickstart); mensagens em português; tokens nunca logados

**Scale/Scope**: 1 rota/página nova, 2 páginas ajustadas, 1 endpoint de reenvio, 1 opção de configuração

## Constitution Check

| Princípio | Avaliação |
|-----------|-----------|
| I. Clean Architecture | OK. Reenvio como caso de uso em Application; chamada ao Supabase em Infrastructure; controller fino. |
| II. Monorepo | OK. Sem novos diretórios de topo. |
| III. Testes obrigatórios | OK. xUnit (caso de uso, endpoint) e Vitest (confirm, activate, login). |
| IV. Dependência entre camadas | OK. Sem novas referências. |
| V. Tipagem/Erros | OK. Problem Details no reenvio; Zod no front; sem `any`. |
| VI. Frontend | OK. Lógica em `lib/`; `"use client"` só onde há interação; acessível. |
| VII. Segurança | OK. Token de uso único, sem log de token; reenvio só Administrador. |

Reavaliado após o design: sem violações.

## Project Structure

### Documentation (this feature)

```text
specs/009-corrigir-ativacao-convite/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/invitation-api.md
└── tasks.md             # /speckit-tasks
```

### Source Code (repository root)

```text
src/apps/web/
├── app/auth/confirm/page.tsx        # novo: botão "Continuar" + verificação do token
├── app/auth/callback/route.ts       # mantido (compatibilidade com code)
├── app/activate/page.tsx            # valida sessão; erros distintos
├── app/login/page.tsx               # lê erro de fragmento/query
├── app/users/UsersPageClient.tsx    # ação "Reenviar convite"
├── lib/auth/invitation.ts           # novo: mapeamento de erros
└── lib/users.ts                     # resendInvitation

src/services/api/
├── SistemaEstoque.Application/Users/        # ResendInvitation, IIdentityAdministration
├── SistemaEstoque.Infrastructure/Identity/  # redirect_to no invite; reenvio
└── SistemaEstoque.API/Controllers/UsersController.cs  # POST /api/users/{id}/invitation

tests/api/ e tests/web/
```

**Structure Decision**: Reaproveitar a estrutura do monorepo; mudanças localizadas nos módulos de identidade (features 003/006).

## Complexity Tracking

Sem violações a justificar.
