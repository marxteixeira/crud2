# Tasks: Resolver bloqueios do fechamento de Identidade e perfis

**Input**: `specs/006-resolver-bloqueios-identidade/` (plan, spec, research, data-model, quickstart)

**Prerequisites**: plan.md, spec.md

**Regras**: nenhum segredo ou dado pessoal em código ou relatórios; não alterar regras de negócio; gates .NET nunca em paralelo no mesmo checkout. Itens manuais (003 T036, T040; 004 T018 parte manual) são considerados concluídos pelo responsável (declaração de 2026-10-25).

## Phase 1: Setup

- [X] T001 Reexecutar `dotnet build` e `dotnet test` em `src/services/api/SistemaEstoque.sln` e anotar as 26 falhas atuais por grupo (A/B/C) em `specs/005-fechamento-identidade-perfis/readiness-report.md`

## Phase 2: Foundational

- [X] T002 Confirmar em `src/services/api/SistemaEstoque.API/Program.cs` e `Configuration/AuthenticationExtensions.cs` que Development registra Bearer quando `Supabase:Url` existe (User Secrets) e que nenhuma mudança de produção é necessária

## Phase 3: User Story 1 - Suíte automatizada verde (P1) 🎯 MVP

**Goal**: 0 falhas na suíte, sem credenciais reais.

**Independent Test**: `dotnet test src/services/api/SistemaEstoque.sln` com 0 falhas, com e sem User Secrets de Supabase.

- [X] T003 [US1] Grupo A: em `tests/api/SistemaEstoque.API.Tests/Infrastructure/AuthenticationTestFactory.cs`, injetar `ConnectionStrings:Default` fictícia (sem segredo) via configuração em memória; aplicar o mesmo em fábricas que usam `WebApplicationFactory<Program>` em `GlobalErrorHandlingTests.cs`, `StructuredLoggingTests.cs`, `UserManagementEndpointsTests.cs`, `AuthenticationEndpointsTests.cs`
- [X] T004 [US1] Grupo B: em `tests/api/SistemaEstoque.API.Tests/RoleAuthorizationTests.cs` (`JwtApiFactory`), isolar a configuração de User Secrets e remover o registro prévio de Bearer antes de `AddSupabaseAuthentication`, mantendo o `PostConfigure` de validação de assinatura, audiência e emissor
- [X] T005 [US1] Grupo C: em `tests/api/SistemaEstoque.API.Tests/PersistenceConnectivityTests.cs`, usar `ConnectionStrings__Default` de banco descartável, remover a asserção fixa de host e ignorar o teste com motivo explícito quando a variável estiver ausente
- [X] T006 [US1] Confirmar em `.github/workflows/ci.yml` que o job da API fornece `ConnectionStrings__Default` do serviço Postgres e atualizar `specs/004-supabase-database-config/quickstart.md` com o comando Docker de banco descartável
- [X] T007 [US1] Executar `dotnet test` e depois `npm run lint`, `npm test`, `npm run build` na raiz; corrigir regressões até 0 falhas (T037)

**Checkpoint**: gates verdes.

## Phase 4: User Story 2 - Desempenho e evidências (P1)

**Goal**: T039 concluído e relatório com evidências.

**Independent Test**: teste de desempenho passa com p95 ≤ 3 s.

- [X] T008 [US2] Criar `tests/api/SistemaEstoque.API.Tests/UserManagementPerformanceTests.cs` com 1.000 contas sintéticas em memória, ≥ 100 requisições de listagem e asserção de p95 ≤ 3 s (T039)
- [X] T009 [US2] Executar o teste de desempenho e registrar p95 e total de requisições (sem dados pessoais) em `specs/005-fechamento-identidade-perfis/readiness-report.md`
- [X] T010 [US2] Registrar T036, T040 e a parte manual do T018 como "declarado pelo responsável em 2026-10-25" no `readiness-report.md`

## Phase 5: User Story 3 - Decisão final (P2)

**Goal**: decisão `pronto` com evidência.

- [X] T011 [US3] Marcar T036, T037, T039, T040 em `specs/003-identidade-perfis/tasks.md` e T018 em `specs/004-supabase-database-config/tasks.md` apenas com evidência registrada
- [X] T012 [US3] Atualizar a decisão em `specs/005-fechamento-identidade-perfis/readiness-report.md` para `pronto` (ou `bloqueada` com pendências) e indicar a próxima fase "Localização e produtos"

## Dependencies

- T001 → T002 → T003/T004/T005 (paralelizáveis, arquivos distintos) → T006 → T007
- T008 depende de T007; T009 → T010 → T011 → T012

## Parallel Opportunities

- T003, T004 e T005 tocam arquivos diferentes (executar os testes só ao final, sequencialmente).

## Implementation Strategy

MVP = User Story 1 (T001–T007). Depois T039 e a decisão final.
