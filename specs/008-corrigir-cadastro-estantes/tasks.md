# Tasks: Corrigir cadastro de estantes

**Input**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [quickstart.md](./quickstart.md)

**Status do P1**: o usuário aplicou as migrations e validou o cadastro de estante manualmente na aplicação. A causa (migrations pendentes) está confirmada e a User Story 1 está resolvida sem alteração de código. Restam apenas o registro dessa validação, a prevenção de recorrência (US3) e a diagnosticabilidade (US2), todas de baixo risco e opcionais para destravar o teste manual.

**Tests**: exigidos pela constituição (princípio III) para código novo.

## Phase 1: User Story 1 - Cadastrar estante (Priority: P1) 🎯 já resolvida

**Independent Test**: cadastrar estante `A1` em Localizações e vê-la na lista; repetir `a1 ` gera mensagem de duplicidade; código vazio gera validação.

- [X] T001 [US1] Registrar em `specs/008-corrigir-cadastro-estantes/quickstart.md` (seção "Resultado") que a causa foi migrations pendentes e que o cadastro foi validado manualmente após `dotnet ef database update`
- [X] T002 [P] [US1] Confirmar em `tests/api/SistemaEstoque.API.Tests/LocationEndpointsTests.cs` que existem testes de criação de estante com sucesso, duplicidade (409) e validação (400); adicionar os que faltarem (FR-010)

**Checkpoint**: US1 concluída.

## Phase 2: User Story 3 - Prevenir recorrência em ambiente novo (Priority: P2)

**Independent Test**: com banco sem a migration, a API avisa na inicialização e `/health/ready` retorna 503; com banco atualizado, sem aviso e 200.

- [X] T003 [P] [US3] Documentar em `README.md` o passo `dotnet ef database update --project src/services/api/SistemaEstoque.Infrastructure --startup-project src/services/api/SistemaEstoque.API` para ambiente local (FR-007)
- [X] T004 [P] [US3] Corrigir o pré-requisito de migração em `specs/007-localizacao-produtos/quickstart.md` (linhas 36 e 62) apontando para o README
- [X] T005 [US3] (Ajustado: o health check `ready` quebraria o CI, cujo banco não é migrado; usado aviso de startup.) Criar `IPendingMigrationsProvider`/`EfPendingMigrationsProvider` e `PendingMigrationsStartupCheck` em `src/services/api/SistemaEstoque.Infrastructure/Persistence/`
- [X] T006 [US3] Em `src/services/api/SistemaEstoque.API/Program.cs`, registrar o aviso (`AddPendingMigrationsStartupCheck`) na inicialização quando houver migrations pendentes, sem aplicar DDL e sem derrubar a API; ignorado no ambiente `Testing`
- [X] T007 [US3] Testes em `tests/api/SistemaEstoque.API.Tests/PendingMigrationsStartupCheckTests.cs`: pendente → aviso com nomes; sem pendência → sem aviso; banco indisponível → não lança

## Phase 3: User Story 2 - Falhas diagnosticáveis (Priority: P2)

**Independent Test**: provocar falha em operação de localização e achar a causa no log pelo `traceId` retornado.

- [X] T008 [US2] Em `src/services/api/SistemaEstoque.API/Middleware/RequestLoggingMiddleware.cs`, incluir `Path` no log e usá-lo quando o `DisplayName` do endpoint for `405 HTTP Method Not Supported` ou ausente (FR-009)
- [X] T009 [P] [US2] Teste em `tests/api/SistemaEstoque.API.Tests/StructuredLoggingTests.cs`: preflight `OPTIONS /api/locations/racks` registra o path e não o texto "405 HTTP Method Not Supported" como rota
- [X] T010 [P] [US2] Teste em `tests/api/SistemaEstoque.API.Tests/LocationEndpointsTests.cs`: falha inesperada em `POST /api/locations/racks` retorna 500 Problem Details com `traceId`, sem detalhes técnicos, e registra a exceção com o mesmo `TraceId` (FR-004, FR-005)
- [X] T011 [US2] Em `src/apps/web/lib/api.ts` (`describeApiError`) e `LocationsPageClient.tsx`, exibir o `traceId` na mensagem de erro 500 e evitar rejeição não tratada no envio dos formulários; coberto em `tests/web/LocationsPage.test.tsx`

## Phase 4: Polish

- [X] T012 Executar `dotnet test src/services/api/SistemaEstoque.sln` (API 86 ok/1 skip, Domain 31 ok) e `npm --prefix tests/web test` (26 ok); lint web ok
- [X] T013 Percorrer o fluxo de cadastro de estante após aplicar a migration (validado manualmente pelo usuário)

## Dependencies & Execution Order

- US1 (T001–T002) não depende de nada; já está funcional.
- US3 e US2 são independentes entre si. T005 → T006 → T007; T008 → T009.
- Polish depende de todas as tarefas escolhidas.

## Parallel Examples

- T001/T002/T003/T004 podem rodar juntas (arquivos distintos).
- T009/T010 em paralelo após T008.

## Implementation Strategy

- **MVP**: Phase 1 (apenas documentar/validar). O defeito já está resolvido.
- **Recomendado**: Phase 1 + T003/T004 (documentação, custo mínimo) para evitar que o problema reapareça em outro ambiente.
- **Opcional**: restante de US3/US2 (código), se quiser diagnóstico e aviso automáticos.
