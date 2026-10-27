# Tasks: Corrigir ativação de conta por convite

**Input**: Design documents em `/specs/009-corrigir-ativacao-convite/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/invitation-api.md, quickstart.md

**Tests**: Obrigatórios pela constituição (Princípio III): xUnit em `tests/api/` e Vitest em `tests/web/`.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: Setup

- [ ] T001 Reproduzir a falha localmente: cadastrar usuário em `/users`, capturar a URL do e-mail e confirmar que cai em `/login#error=...otp_expired`; registrar o achado em `specs/009-corrigir-ativacao-convite/research.md`
- [ ] T002 [P] Adicionar `Supabase:RedirectUrl` (`http://localhost:3000`) em `src/services/api/SistemaEstoque.API/appsettings.json` e a propriedade `RedirectUrl` em `SupabaseAdminOptions` (`src/services/api/SistemaEstoque.Infrastructure/Identity/`)

---

## Phase 2: Foundational

- [ ] T003 Criar `src/apps/web/lib/auth/invitation.ts` com `mapInvitationError(code)` retornando mensagens em português para: convite expirado/inválido (`otp_expired`, `access_denied`), senha rejeitada, serviço indisponível; e `parseAuthErrorFromUrl(url)` lendo `error`/`error_code` de query e fragmento (sem `any`)
- [ ] T004 [P] Teste Vitest de `mapInvitationError` e `parseAuthErrorFromUrl` em `tests/web/InvitationErrors.test.ts`

**Checkpoint**: utilitários de erro prontos.

---

## Phase 3: User Story 1 - Ativar conta a partir do link do convite (P1) 🎯 MVP

**Goal**: link válido leva à tela de ativação e a senha é definida.

**Independent Test**: cenário 1 do quickstart.

### Tests

- [ ] T005 [P] [US1] Teste xUnit: `InviteAsync` envia `redirect_to` configurado no corpo/consulta de `auth/v1/invite` em `tests/api/SistemaEstoque.API.Tests/SupabaseInvitationRequestTests.cs`
- [ ] T006 [P] [US1] Teste Vitest da página `/auth/confirm`: sem clique não chama `verifyOtp`; com clique chama `verifyOtp({ token_hash, type: "invite" })` e navega para `/activate`; `type` diferente de `invite` é rejeitado, em `tests/web/AuthConfirm.test.tsx`
- [ ] T007 [P] [US1] Teste Vitest de `/activate` com sessão válida: `updateUser` + `POST /api/auth/activation` e redirecionamento para `/`, em `tests/web/ActivatePage.test.tsx`

### Implementation

- [ ] T008 [US1] Em `src/services/api/SistemaEstoque.Infrastructure/Identity/SupabaseAdminClient.cs`, enviar `redirect_to` (de `Supabase:RedirectUrl` + `/auth/confirm`) em `InviteAsync`
- [ ] T009 [US1] Criar `src/apps/web/app/auth/confirm/page.tsx` (client): lê `token_hash` e `type`, aceita apenas `invite`, botão "Continuar" acessível que chama `verifyOtp` e faz `router.replace("/activate")`; em erro, mostra mensagem via `mapInvitationError`
- [ ] T010 [US1] Incluir `auth/confirm` na exclusão do matcher em `src/apps/web/middleware.ts`
- [ ] T011 [US1] Documentar no `specs/009-corrigir-ativacao-convite/quickstart.md` e no `README.md` o template "Invite user" do Supabase (`{{ .SiteURL }}/auth/confirm?token_hash={{ .TokenHash }}&type=invite`) e as URLs permitidas

**Checkpoint**: ativação por convite funciona ponta a ponta.

---

## Phase 4: User Story 2 - Mensagens claras para convite inválido ou expirado (P2)

**Goal**: erros reais e acionáveis, sem falha silenciosa.

**Independent Test**: cenários 2 e 4 do quickstart.

### Tests

- [ ] T012 [P] [US2] Teste Vitest: `/activate` sem sessão mostra "convite inválido ou expirado" e não chama `updateUser`; erros de senha e de serviço mostram mensagens distintas, em `tests/web/ActivatePage.test.tsx`
- [ ] T013 [P] [US2] Teste Vitest: `/login#error=access_denied&error_code=otp_expired` exibe mensagem amigável, em `tests/web/AuthFlow.test.tsx`

### Implementation

- [ ] T014 [US2] Em `src/apps/web/app/activate/page.tsx`, verificar `getSession()` ao montar (estado Verificando/Pronto/Convite inválido), bloquear envio sem sessão e usar `mapInvitationError` para distinguir senha rejeitada, convite inválido e falha de serviço (FR-004, FR-005)
- [ ] T015 [US2] Em `src/apps/web/app/login/page.tsx`, usar `parseAuthErrorFromUrl` (query e fragmento) e exibir a mensagem com `role="alert"` (FR-003)
- [ ] T016 [US2] Registrar via `console.warn` sem dados sensíveis apenas o código do erro nas falhas de ativação (FR-008) em `src/apps/web/app/activate/page.tsx`

**Checkpoint**: nenhum erro de convite aparece apenas na URL ou como mensagem genérica.

---

## Phase 5: User Story 3 - Administrador reenvia convite (P3)

**Goal**: reenviar convite a usuário pendente.

**Independent Test**: cenário 5 do quickstart.

### Tests

- [ ] T017 [P] [US3] Testes xUnit do caso de uso `ResendInvitation` (pendente OK; ativo → conflito; inexistente → não encontrado) em `tests/api/SistemaEstoque.API.Tests/UserManagementServiceTests.cs`
- [ ] T018 [P] [US3] Testes de integração de `POST /api/users/{id}/invitation` (204, 404, 409, 401/403 para não administrador) em `tests/api/SistemaEstoque.API.Tests/UserManagementEndpointsTests.cs`
- [ ] T019 [P] [US3] Teste Vitest: ação "Reenviar convite" visível só para usuário pendente, em `tests/web/UserManagementPage.test.tsx`

### Implementation

- [ ] T020 [US3] Adicionar `ResendInvitationAsync(email, displayName)` a `IIdentityAdministration` (`src/services/api/SistemaEstoque.Application/Users/IIdentityAdministration.cs`) e implementar em `SupabaseAdminClient.cs` reemitindo o convite com `redirect_to`
- [ ] T021 [US3] Adicionar `ResendInvitationAsync(Guid id)` em `UserManagementContracts.cs` e `src/services/api/SistemaEstoque.Infrastructure/Identity/UserManagementService.cs`: somente conta pendente, 409 caso contrário, sem alterar estado/perfil
- [ ] T022 [US3] Adicionar `POST /api/users/{id}/invitation` em `src/services/api/SistemaEstoque.API/Controllers/UsersController.cs` (policy de Administrador, Problem Details, log com TraceId sem tokens)
- [ ] T023 [P] [US3] Adicionar `resendInvitation(client, id)` em `src/apps/web/lib/users.ts`
- [ ] T024 [US3] Adicionar botão "Reenviar convite" em `src/apps/web/app/users/UsersPageClient.tsx` para usuários pendentes, com feedback de sucesso/erro acessível
- [ ] T025 [P] [US3] Atualizar `specs/003-identidade-perfis/contracts/user-management-api.yaml` com o novo endpoint

**Checkpoint**: administrador resolve convites expirados sem recriar usuário.

---

## Phase 6: Polish

- [ ] T026 Executar `dotnet test` na solução e `npm test` em `tests/web`; corrigir regressões
- [ ] T027 Executar o quickstart completo (cenários 1–5) em ambiente local
- [ ] T028 [P] Executar lint/typecheck do web (`npm run lint`, `tsc --noEmit`) em `src/apps/web`

---

## Dependencies & Execution Order

- Setup → Foundational → US1 (MVP) → US2 → US3 → Polish.
- US2 depende de T003 e toca `activate/page.tsx` já alterada em US1 (T007 testa o mesmo arquivo de teste que T012: executar em sequência).
- US3 depende de T008 (`redirect_to`/opções) mas é independente de US2.

## Parallel Opportunities

- T002 ‖ T003/T004; T005 ‖ T006 ‖ T007; T012 ‖ T013; T017 ‖ T018 ‖ T019; T023 ‖ T025.

## Implementation Strategy

- **MVP**: Phases 1–3 (US1) resolvem o bloqueio de ativação.
- Entrega incremental: US2 (mensagens), depois US3 (reenvio).
