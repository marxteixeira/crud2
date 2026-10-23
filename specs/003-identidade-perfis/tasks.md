---
description: "Lista de tarefas da funcionalidade de identidade e perfis"
---

# Tasks: Identidade e perfis

**Input**: Documentos de design em `/specs/003-identidade-perfis/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/user-management-api.yaml`

**Tests**: Incluídos porque a constituição exige testes automatizados para regras de domínio, endpoints e comportamento web. Escrever testes antes ou junto da implementação e validar os cenários da história de forma independente.

**Organization**: Tarefas agrupadas por história de usuário.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode ser executada em paralelo sem depender de arquivo/tarefa incompleta.
- **[Story]**: História relacionada, obrigatória apenas nas fases de histórias.
- Toda tarefa aponta para os caminhos exatos dos arquivos que deve criar ou alterar.

## Path Conventions

Manter a estrutura do monorepo existente: `src/services/api/SistemaEstoque.*`, `src/apps/web/`, `tests/api/`, `tests/web/` e `.github/workflows/ci.yml`. Não criar projeto, camada ou diretório de topo.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Adicionar as dependências aprovadas e preparar configuração segura compartilhada.

- [ ] T001 Adicionar `Microsoft.AspNetCore.Authentication.JwtBearer` ao projeto `src/services/api/SistemaEstoque.API/SistemaEstoque.API.csproj` e `@supabase/supabase-js` e `@supabase/ssr` ao workspace web em `src/apps/web/package.json`; atualizar o `package-lock.json` pela instalação npm do repositório.
- [ ] T002 [P] Criar `src/apps/web/.env.example` com `NEXT_PUBLIC_SUPABASE_URL`, `NEXT_PUBLIC_SUPABASE_ANON_KEY` e `NEXT_PUBLIC_API_URL`, sem segredos administrativos; documentar a configuração não secreta equivalente em `src/services/api/SistemaEstoque.API/appsettings.json`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Criar a persistência e os pontos de integração/autorização que todas as histórias dependem.

**Critical**: Nenhuma história começa antes do modelo de conta, persistência e integração de autenticação estarem disponíveis.

- [ ] T003 [P] Criar testes de validação da configuração Supabase e JWT em `tests/api/SistemaEstoque.API.Tests/AuthenticationConfigurationTests.cs`, cobrindo valores ausentes/inválidos e garantindo que mensagens não contenham chaves ou tokens.
- [ ] T004 [P] Criar a entidade `ContaDeUsuario` em `src/services/api/SistemaEstoque.Domain/Entities/ContaDeUsuario.cs` com `identity_subject` UUID único e imutável após vinculação; e-mail normalizado único e comparado sem distinção entre maiúsculas/minúsculas; `pending_email` opcional único contra e-mails ativos e pendentes de outras contas; `display_name` não vazio; `role` limitado a `Administrador` ou `Operador`; `status` limitado a `ConvitePendente`, `Ativa` ou `Inativa`, sendo somente `Ativa` autorizada; `created_at` e `updated_at` em UTC; `created_by` e `updated_by` identificando o Administrador ou o valor reservado `bootstrap`; não armazenar senhas nem credenciais administrativas.
- [ ] T005 [P] Criar a entidade de domínio para revogação de sessão em `src/services/api/SistemaEstoque.Domain/Entities/SessaoRevogada.cs`, usando `provider_session_id` UUID único do Supabase e `revoked_at` em UTC; armazenar apenas a marca de revogação, nunca a sessão ou credenciais completas.
- [ ] T006 [P] Criar testes unitários de regras das entidades em `tests/api/SistemaEstoque.Domain.Tests/ContaDeUsuarioTests.cs` e `tests/api/SistemaEstoque.Domain.Tests/SessaoRevogadaTests.cs`, cobrindo estados permitidos, identidade imutável, nome não vazio e unicidade do identificador da sessão; a regra do último Administrador é testada na camada de aplicação em T020.
- [ ] T007 Configurar `SistemaEstoqueDbContext` e mapeamentos em `src/services/api/SistemaEstoque.Infrastructure/Persistence/SistemaEstoqueDbContext.cs` e `src/services/api/SistemaEstoque.Infrastructure/Persistence/Configurations/ContaDeUsuarioConfiguration.cs` e `src/services/api/SistemaEstoque.Infrastructure/Persistence/Configurations/SessaoRevogadaConfiguration.cs`, incluindo índices únicos para `identity_subject`, e-mail normalizado e e-mail pendente, tipos/enums válidos e colunas de auditoria.
- [ ] T008 Criar e verificar a migração PostgreSQL da conta e revogação em `src/services/api/SistemaEstoque.Infrastructure/Persistence/Migrations/`; a migração não pode inserir identidades pessoais ou segredos e não deve ser aplicada automaticamente durante o startup.
- [ ] T009 Implementar opções tipadas e validação da configuração Supabase em `src/services/api/SistemaEstoque.API/Configuration/SupabaseOptions.cs` e registrar cliente HTTP administrativo servidor-only em `src/services/api/SistemaEstoque.Infrastructure/DependencyInjection.cs`; credenciais vêm do ambiente e nunca são incluídas em logs.
- [ ] T010 Configurar validação JWT bearer, `sub`/`session_id`, política de ativação pendente e política de Administrador em `src/services/api/SistemaEstoque.API/Program.cs` e `src/services/api/SistemaEstoque.API/Configuration/AuthenticationExtensions.cs`; validar assinatura assimétrica via JWKS, emissor, público e expiração, derivar perfil/situação da conta PostgreSQL atual e recusar sessões revogadas em toda requisição protegida.
- [ ] T011 Criar doubles de provedor e persistência para testes HTTP autenticados em `tests/api/SistemaEstoque.API.Tests/Infrastructure/AuthenticationTestFactory.cs` e `tests/api/SistemaEstoque.API.Tests/Infrastructure/FakeSupabaseAdminClient.cs`, sem segredos ou chamadas de produção.

**Checkpoint**: Entidades, migração, opções Supabase e autenticação/autorização reutilizáveis estão prontas; histórias podem ser implementadas sem depender de um projeto Supabase de produção.

---

## Phase 3: User Story 1 — Entrar e sair do sistema (Priority: P1) 🎯 MVP

**Goal**: Administrador e Operador entram com e-mail/senha, ativam convites, consultam seu perfil atual e encerram a sessão sem que tokens anteriores permaneçam autorizados.

**Independent Test**: Com identidades de teste já provisionadas, testar login correto/incorreto para os dois perfis, ativação apenas de conta pendente confirmada, consulta da identidade atual e logout; requisições protegidas com sessão encerrada devem retornar `401`.

### Tests for User Story 1

- [ ] T012 [P] [US1] Criar testes de integração para `GET /api/auth/me`, `POST /api/auth/activation` e `POST /api/auth/logout` em `tests/api/SistemaEstoque.API.Tests/AuthenticationEndpointsTests.cs`, verificando identidade, ativação de convite, recusas e revogação persistida.
- [ ] T013 [P] [US1] Criar testes web de login, logout, convite/ativação, validação de campos e mensagens de erro sem revelar se o e-mail está cadastrado em `tests/web/AuthFlow.test.tsx`.

### Implementation for User Story 1

- [ ] T014 [P] [US1] Criar clientes Supabase de navegador/servidor, leitura segura da sessão e integração Axios com bearer token em `src/apps/web/lib/supabase/client.ts`, `src/apps/web/lib/supabase/server.ts` e `src/apps/web/lib/api.ts`; não disponibilizar chave de serviço ao web.
- [ ] T015 [US1] Criar página acessível de login em `src/apps/web/app/login/page.tsx` com e-mail e senha, validação Zod, estado de envio/erro e fluxo Supabase `signInWithPassword`; usar mensagens que não revelem existência de e-mail.
- [ ] T016 [US1] Criar fluxo web de callback/ativação de convite em `src/apps/web/app/auth/callback/route.ts` e `src/apps/web/app/activate/page.tsx`, permitindo ao titular estabelecer senha própria e chamar somente a operação de ativação da conta autenticada.
- [ ] T017 [US1] Criar endpoints `GET /api/auth/me`, `POST /api/auth/activation` e `POST /api/auth/logout` em `src/services/api/SistemaEstoque.API/Controllers/AuthController.cs` e casos de uso em `src/services/api/SistemaEstoque.Application/Authentication/GetCurrentAccount.cs`, `src/services/api/SistemaEstoque.Application/Authentication/ActivateInvitedAccount.cs` e `src/services/api/SistemaEstoque.Application/Authentication/RevokeCurrentSession.cs`; logout deve persistir revogação antes de encerrar a sessão Supabase e falha de persistência não pode aparentar sucesso.
- [ ] T018 [US1] Criar middleware de sessão Supabase para o App Router em `src/apps/web/middleware.ts`, protegendo páginas autenticadas, preservando rota de ativação pendente e direcionando usuários sem sessão para `/login`.
- [ ] T019 [US1] Validar que `src/apps/web/app/layout.tsx` mantém `lang="pt-BR"`, foco, rótulos acessíveis e mensagens de autenticação responsivas; integrar ação de logout à navegação da sessão em `src/apps/web/components/AuthenticatedNavigation.tsx`.

**Checkpoint**: Login/logout/ativação são testáveis independentemente com contas de teste, sem expor segredos e sem criar fluxos de catálogo.

---

## Phase 4: User Story 2 — Administrar usuários e perfis (Priority: P1)

**Goal**: Administradores convidam, localizam, alteram e inativam contas; e-mail alterado só substitui o login atual depois que o titular confirma o novo endereço.

**Independent Test**: Entrar como Administrador, convidar um Administrador e um Operador, ativar um convite, paginar/filtrar contas, editar perfil/nome/e-mail, confirmar troca de e-mail e inativar/reativar uma conta; duplicatas e remoção do último Administrador são recusadas.

### Tests for User Story 2

- [ ] T020 [P] [US2] Criar testes unitários de convite, e-mail duplicado/pendente, atualização de perfil, auditoria e proteção do último Administrador em `tests/api/SistemaEstoque.Domain.Tests/ContaDeUsuarioTests.cs` e `tests/api/SistemaEstoque.API.Tests/UserManagementServiceTests.cs`.
- [ ] T021 [P] [US2] Criar testes de integração das rotas `GET/POST /api/users`, `GET/PATCH /api/users/{userId}` e `PUT /api/users/{userId}/activation` em `tests/api/SistemaEstoque.API.Tests/UserManagementEndpointsTests.cs`, incluindo paginação, filtros, e-mail duplicado e mensagens RFC 7807.
- [ ] T022 [P] [US2] Criar testes de interface de listagem, convite, edição, perfil, situação e e-mail pendente em `tests/web/UserManagement.test.tsx`; verificar estados de carregamento, vazio, falha e acessibilidade.
- [ ] T023 [P] [US2] Criar testes do adaptador Supabase administrativo em `tests/api/SistemaEstoque.API.Tests/SupabaseAdminClientTests.cs`, verificando convite, metadados administrativos, atualização de e-mail e compensação em erro, sem chamadas externas reais.

### Implementation for User Story 2

- [ ] T024 [US2] Implementar integração HTTP administrativa Supabase em `src/services/api/SistemaEstoque.Infrastructure/Identity/SupabaseAdminClient.cs`, incluindo convite por e-mail, atualização de `app_metadata.role`, início de confirmação de novo e-mail e revogação do provedor; usar credencial server-only.
- [ ] T025 [US2] Implementar casos de uso de convite, consulta paginada, edição, ativação/inativação e troca pendente de e-mail em `src/services/api/SistemaEstoque.Application/Users/UserManagementService.cs` e validações em `src/services/api/SistemaEstoque.Application/Users/UserManagementValidators.cs`, com FluentValidation e `created_by`/`updated_by`; garantir compensação/diagnóstico explícito quando provedor e PostgreSQL divergirem.
- [ ] T026 [US2] Implementar endpoints protegidos e DTOs conforme `specs/003-identidade-perfis/contracts/user-management-api.yaml` em `src/services/api/SistemaEstoque.API/Controllers/UsersController.cs`; exigir Administrador, paginar listagens e usar consultas sem rastreamento para leitura.
- [ ] T027 [US2] Implementar administração de usuários mobile-first em `src/apps/web/app/users/page.tsx`, `src/apps/web/app/users/[userId]/page.tsx`, `src/apps/web/components/users/UserTable.tsx` e `src/apps/web/components/users/UserForm.tsx`; usar cliente da API em `src/apps/web/lib/users.ts`, não colocar lógica de negócio nos componentes e não expor segredo do provedor.
- [ ] T028 [US2] Implementar `POST /api/auth/email-change-confirmation` em `src/services/api/SistemaEstoque.API/Controllers/AuthController.cs` e caso de uso em `src/services/api/SistemaEstoque.Application/Users/ConfirmEmailChange.cs`; confirmar a identidade autenticada com o novo e-mail verificado pelo provedor antes de promovê-lo a `email`.
- [ ] T029 [US2] Documentar a configuração do primeiro Administrador, convites, ambiente local e segredos em `README.md` e `specs/003-identidade-perfis/quickstart.md`; não adicionar credenciais, e-mails reais ou identificadores de produção a arquivos versionados.

**Checkpoint**: Administradores gerenciam contas e convites; duplicidade, limites do último Administrador e dados de senha/segredo são tratados com segurança.

---

## Phase 5: User Story 3 — Aplicar permissões por perfil (Priority: P1)

**Goal**: Garantir no servidor e na experiência web que cada ação respeite o perfil atual; mudanças de perfil, inativação e logout têm efeito nas sessões já abertas.

**Independent Test**: Reutilizar JWTs previamente emitidos em chamadas após alteração de perfil, inativação e logout; verificar que o papel atual e a sessão revogada são aplicados imediatamente e que nenhuma operação recusada altera dados.

### Tests for User Story 3

- [ ] T030 [P] [US3] Criar testes de integração de autenticação/autorização em `tests/api/SistemaEstoque.API.Tests/RoleAuthorizationTests.cs` para token ausente, inválido, assinatura/emissor/público/expiração incorretos, sessão revogada, conta inativa, role antiga e usuário pendente.
- [ ] T031 [P] [US3] Criar teste ponta a ponta de requisição com token já emitido antes de alteração de perfil/inativação/logout em `tests/api/SistemaEstoque.API.Tests/ImmediateAuthorizationChangeTests.cs`; conferir papel atual, `401`/`403` esperado e ausência de mutação indevida.
- [ ] T032 [P] [US3] Criar testes web para ocultar/mostrar ações conforme `GET /api/auth/me`, redirecionar sessão encerrada e tratar `401/403` em `tests/web/RoleAccess.test.tsx`.

### Implementation for User Story 3

- [ ] T033 [US3] Implementar resolução de perfil atual e checagem de `SessaoRevogada` por requisição protegida em `src/services/api/SistemaEstoque.API/Configuration/AuthenticationExtensions.cs` e `src/services/SistemaEstoque.Application/Authentication/CurrentAccountAuthorization.cs`; não autorizar com role antiga do JWT nem com estado em cache desatualizado.
- [ ] T034 [US3] Aplicar políticas `Administrador` e `Operador` e recusa uniforme RFC 7807 em `src/services/api/SistemaEstoque.API/Program.cs` e `src/services/api/SistemaEstoque.API/Configuration/AuthorizationPolicies.cs`; garantir que endpoints administrativos neguem Operador e anônimo independentemente da interface web.
- [ ] T035 [US3] Atualizar o estado da sessão e acesso da interface web após perfil alterado, inativação, logout ou resposta `401/403` em `src/apps/web/lib/auth.ts`, `src/apps/web/components/AuthenticatedNavigation.tsx` e `src/apps/web/middleware.ts`; nunca tratar falha de consulta de permissão como sucesso.

**Checkpoint**: Papéis são aplicados no servidor e na interface a partir do estado atual; tokens antigos não preservam privilégios ou sessões encerradas.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validar a integração completa, atualizar documentação geral e executar as gates de qualidade existentes.

- [ ] T036 [P] Atualizar exemplos de configuração/segredos e descrição do fluxo de execução no `README.md`, incluindo Supabase Auth, PostgreSQL, primeiro Administrador e redirects locais, sem credenciais reais.
- [ ] T037 [P] Verificar que testes de arquitetura em `tests/api/SistemaEstoque.Domain.Tests/LayerDependencyTests.cs` continuam bloqueando referências indevidas entre Domain, Application, Infrastructure e API.
- [ ] T038 Executar os cenários manuais de `specs/003-identidade-perfis/quickstart.md` com projeto Supabase isolado e PostgreSQL de teste; registrar sucesso/falhas sem dados pessoais.
- [ ] T039 Executar `dotnet build src/services/api/SistemaEstoque.sln`, `dotnet test src/services/api/SistemaEstoque.sln`, `npm run lint`, `npm test` e `npm run build`; corrigir falhas introduzidas pela funcionalidade sem incluir segredos nos logs.
- [ ] T040 Revisar `specs/003-identidade-perfis/contracts/user-management-api.yaml`, `specs/003-identidade-perfis/data-model.md`, configuração e bundles web para confirmar consistência dos enums, paginação, acesso Administrador, ausência de senhas/tokens/segredos e critérios de aceite.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: independente; instala os clientes JWT/Supabase e configura exemplos sem segredos.
- **Foundational (Phase 2)**: depende de Setup e bloqueia as histórias; cria modelo persistido e autenticação comum.
- **User Stories (Phases 3–5)**: dependem da Foundation; podem começar após T011, observadas as dependências entre histórias descritas abaixo.
- **Polish (Phase 6)**: depende da integração das histórias que serão entregues.

### User Story Dependencies

- **US1 (P1)**: depende apenas de Setup/Foundational; primeiro incremento demonstrável com identidades de teste/Administrador bootstrap.
- **US2 (P1)**: depende de US1 para sessão autenticada e endpoints de identidade, e do guard Administrador foundational para proteger o gerenciamento.
- **US3 (P1)**: depende de US1 (sessões/revogação) e integra com US2 para validar alterações de perfil em contas administradas; pode desenvolver testes de autorização com fixtures enquanto US2 implementa a UI.

### Within Each User Story

- Escrever testes da história antes/junto da implementação; confirmar que o cenário falha antes de concluir a implementação.
- Concluir contratos e regras/persistência antes dos serviços; serviços antes de endpoints; endpoints antes da integração web.
- Alterações de `Program.cs`, `AuthController.cs` e `middleware.ts` devem ser integradas sequencialmente para evitar edições concorrentes.
- Tarefas marcadas `[P]` atuam em arquivos distintos e não dependem de implementação ainda incompleta.

### Dependency Graph

```text
T001, T002 -> T003-T011 (Foundation)
T004, T005 -> T007 -> T008
T009, T007, T008 -> T010 -> T011
T011 -> US1 (T012-T019)
US1 + T010 -> US2 (T020-T029)
US1 + US2 profile mutations -> US3 (T030-T035)
US1 + US2 + US3 -> Polish (T036-T040)
```

### Parallel Opportunities

- Setup: T001 (dependências .NET/npm) pode ser paralela a T002 (exemplo de ambiente).
- Foundation: T003, T004 e T005 trabalham em arquivos independentes; T006 cobre testes das entidades e pode avançar com esses testes. Após os modelos, T007/T008 são sequenciais; T009 depende apenas do setup; T010 depende de persistência/configuração.
- US1: T012 (integração API) e T013 (web) são paralelas; T014 pode ser desenvolvido em arquivo diferente antes da integração em páginas.
- US2: T020, T021, T022 e T023 são paralelas (domínio/serviço, API, web e adaptador de provedor em arquivos distintos); a implementação subsequente integra serviço, controller e UI em sequência.
- US3: T030, T031 e T032 são testes paralelos; após eles, autorização da API e atualização de estado web podem avançar em arquivos distintos desde que `Program.cs` e `middleware.ts` não recebam edições concorrentes.

### Parallel Example: User Story 1

```text
Em paralelo:
- T012: testar os endpoints de identidade em tests/api/SistemaEstoque.API.Tests/AuthenticationEndpointsTests.cs
- T013: testar login, logout e ativação em tests/web/AuthFlow.test.tsx
- T014: preparar os clientes Supabase e API em src/apps/web/lib/

Depois:
- T015-T016: construir login e ativação web
- T017-T018: integrar endpoints API e proteção de rotas
- T019: integrar navegação e validar acessibilidade
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Concluir Setup e Foundational, incluindo validação JWT e perfil/estado corrente na API.
2. Concluir US1: login, consulta da conta, ativação de convite e logout com revogação imediata.
3. Parar e validar US1 com identidades não produtivas; demonstrar login permitido/negado e token revogado sem usar segredos de produção.
4. O MVP da história não inclui gerenciamento completo de contas nem telas de produtos.

### Incremental Delivery

1. Setup + Foundational → persistência e autenticação-base prontas.
2. US1 → acesso por e-mail/senha, ativação e saída de sessão testados independentemente.
3. US2 → convite e gerenciamento de Administradores/Operadores, incluindo e-mail pendente.
4. US3 → autorização imediata para sessão/token existente e controles de UI.
5. Polish → quickstart, revisão do contrato e execução de todos os gates.

### Parallel Team Strategy

1. Uma pessoa integra Setup/Foundational, pois alterações de projeto, banco e pipeline de autenticação são pré-requisitos comuns.
2. Após T011, equipe pode desenvolver em paralelo testes web e API de US1.
3. Em US2, testes de serviço, endpoint, web e adaptador Supabase usam arquivos distintos; integrar por dependência antes de fechar o checkpoint.
4. US3 pode iniciar seus testes de regressão em paralelo ao fim da UI de US2; demonstrar efeitos imediatos usando a alteração de perfil já persistida.

## Notes

- A numeração de IDs é sequencial e cada tarefa tem checkbox, caminho(s) explícito(s) e rótulo `[USn]` nas fases de histórias.
- Não criar credenciais reais, endpoint público de cadastro, projetos ou diretórios de topo.
- O perfil armazenado no PostgreSQL da aplicação é a fonte de autorização da API; `app_metadata` do Supabase é sincronizado para futuras emissões de token, não substitui a verificação de estado atual.
- A inicialização da API não aplica migrações nem provisiona automaticamente o primeiro Administrador.
