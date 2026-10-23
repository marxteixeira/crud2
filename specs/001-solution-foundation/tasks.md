# Tasks: Base executável da solução

**Input**: Design documents from `/specs/001-solution-foundation/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/](contracts/), [quickstart.md](quickstart.md)

**Tests**: Incluídos porque a constitution exige testes automatizados para toda funcionalidade, endpoints e limites entre camadas.

**Organization**: Tasks grouped by user story; the existing skeleton, solution layout, CI jobs, and health UI are extended rather than scaffolded again.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode ser executada em paralelo sem depender de arquivos/tarefas incompletas.
- **[Story]**: História relacionada; usada apenas nas fases de user stories.
- Toda tarefa de implementação aponta para os arquivos exatos que deve criar ou alterar.

## Estrutura do repositório

Manter os projetos e diretórios existentes: `src/services/api/SistemaEstoque.*`, `src/apps/web/`, `tests/api/`, `tests/web/` e `.github/workflows/ci.yml`. Não criar projeto ou diretório de topo.

## Phase 1: Setup

**Purpose**: Preparar a persistência aprovada no projeto de infraestrutura existente.

- [X] T001 Adicionar referências compatíveis com EF Core 8, Npgsql EF Core provider e tooling de design-time para migrações em `src/services/api/SistemaEstoque.Infrastructure/SistemaEstoque.Infrastructure.csproj`; não adicionar ORM, provider ou projeto novo.
- [X] T002 Criar o `SistemaEstoqueDbContext` vazio em `src/services/api/SistemaEstoque.Infrastructure/Persistence/SistemaEstoqueDbContext.cs`, sem entidades, tabelas ou migrações de negócio.

---

## Phase 2: Foundational

**Purpose**: Registrar dependências nos limites de camada aprovados antes das histórias.

- [X] T003 Criar o ponto de composição da camada Infrastructure em `src/services/api/SistemaEstoque.Infrastructure/DependencyInjection.cs` e registrar o `SistemaEstoqueDbContext` via Npgsql usando `ConnectionStrings:Default`, sem referenciar API ou adicionar lógica de negócio.
- [X] T004 Adicionar FluentValidation à camada Application em `src/services/api/SistemaEstoque.Application/SistemaEstoque.Application.csproj`, registrar validators da camada em `src/services/api/SistemaEstoque.Application/DependencyInjection.cs`, adicionar Zod às dependências web em `src/apps/web/package.json` e atualizar `package-lock.json`; deixar a base pronta para validação de entradas futuras sem criar fluxos ou regras de negócio nesta etapa.
- [X] T005 Estender `tests/api/SistemaEstoque.Domain.Tests/LayerDependencyTests.cs` para falhar se Domain referenciar projetos/pacotes externos ou se Application referenciar Infrastructure/API; manter todos os projetos e dependências permitidos pela constitution.

**Checkpoint**: A solução existente compõe suas camadas e registra a persistência aprovada; nenhuma história passa a depender de estrutura nova.

---

## Phase 3: User Story 1 — Executar a solução em desenvolvimento (Priority: P1) 🎯 MVP

**Goal**: Iniciar o web e o backend a partir do skeleton existente e confirmar a consulta de saúde sem edição manual de arquivos versionados.

**Independent Test**: Com pré-requisitos instalados e dependências configuradas, iniciar API e web, obter `200` de `GET /health` com `status` e `timestamp`, e observar a página web refletir saúde ou indisponibilidade.

### Tests for User Story 1

- [X] T006 [P] [US1] Estender `tests/api/SistemaEstoque.API.Tests/HealthEndpointTests.cs` para verificar `GET /health` retorna HTTP 200, `status` igual a `Healthy` e `timestamp` UTC parseável conforme `contracts/health-api.yaml`.
- [X] T007 [P] [US1] Estender `tests/web/HealthStatus.test.tsx` para verificar os estados de verificação, saudável e indisponível do componente existente, sem expor falha de rede como estado saudável.

### Implementation for User Story 1

- [X] T008 [US1] Preservar e ajustar o mapeamento de `GET /health` em `src/services/api/SistemaEstoque.API/Program.cs` para manter compatibilidade com o contrato de `status` e `timestamp` e não exigir conexão de banco para indicar disponibilidade do host.
- [X] T009 [US1] Garantir que `src/apps/web/components/HealthStatus.tsx` apresente a falha de comunicação com a API como indisponível e preserve o estado acessível `role="status"` já existente.

**Checkpoint**: A API e a página inicial do web continuam verificáveis independentemente de persistência e indicam saúde/indisponibilidade corretamente.

---

## Phase 4: User Story 2 — Preparar a base para cada ambiente (Priority: P2)

**Goal**: Separar configuração de desenvolvimento, testes e produção; manter segredos fora do repositório; validar a conexão PostgreSQL e isolar os testes.

**Independent Test**: Executar testes com configuração isolada e PostgreSQL descartável no CI; confirmar que configuração inválida falha com diagnóstico seguro, e que nenhum segredo de ambiente real está versionado.

### Tests for User Story 2

- [X] T010 [P] [US2] Criar `tests/api/SistemaEstoque.API.Tests/ConfigurationTests.cs` para verificar que `ConnectionStrings:Default` ausente ou inválida falha a validação do registro de Infrastructure com mensagem sem o valor configurado; usar configuração isolada de teste.
- [X] T011 [P] [US2] Criar `tests/api/SistemaEstoque.API.Tests/PersistenceConnectivityTests.cs` para verificar `SistemaEstoqueDbContext.Database.CanConnectAsync()` contra o PostgreSQL descartável fornecido pelo CI, sem criar tabelas de negócio; escrever o teste nesta fase e executá-lo após T015 configurar o serviço.

### Implementation for User Story 2

- [X] T012 [US2] Adicionar opções tipadas e validação no início da aplicação em `src/services/api/SistemaEstoque.API/Configuration/ApiOptions.cs` e registrar/validar essas opções em `src/services/api/SistemaEstoque.API/Program.cs`; não incluir valores secretos em arquivos versionados.
- [X] T013 [P] [US2] Completar configurações não secretas em `src/services/api/SistemaEstoque.API/appsettings.json`, `src/services/api/SistemaEstoque.API/appsettings.Development.json` e `src/services/api/SistemaEstoque.API/appsettings.Testing.json`, preservando CORS e níveis de log atuais e sem connection strings reais.
- [X] T014 [P] [US2] Documentar as variáveis de configuração de desenvolvimento do frontend em `src/apps/web/.env.example`, preservando `NEXT_PUBLIC_API_URL` e sem adicionar credenciais.
- [X] T015 [US2] Adicionar um serviço PostgreSQL descartável ao job `api` de `.github/workflows/ci.yml` e fornecer suas credenciais de teste apenas como configuração efêmera do workflow, sem segredos Supabase.
- [X] T016 [US2] Documentar a entrega local de segredos por User Secrets/variáveis de ambiente e a entrega de CI/produção pelo ambiente de execução em `README.md`, sem gravar valores reais.

**Checkpoint**: Configurações dos ambientes são independentes, testes não usam dados de produção e a persistência EF Core/Npgsql consegue validar conectividade com PostgreSQL.

---

## Phase 5: User Story 3 — Diagnosticar falhas com segurança (Priority: P3)

**Goal**: Registrar eventos estruturados, apresentar erros RFC 7807 sem dados internos e distinguir prontidão da disponibilidade geral.

**Independent Test**: Provocar falha controlada em integração HTTP, verificar resposta Problem Details sem stack trace/segredo e registro de diagnóstico; verificar que `/health/ready` responde 503 se PostgreSQL estiver indisponível.

**Dependency**: A prontidão persistente depende de T003, T012, T013 e T015 da US2 para registro do contexto, configuração do ambiente e provisionamento do PostgreSQL descartável. T011 verifica conectividade no âmbito da US2, mas não é pré-requisito de implementação da US3; T014 é configuração exclusiva do web.

### Tests for User Story 3

- [X] T017 [P] [US3] Criar `tests/api/SistemaEstoque.API.Tests/GlobalErrorHandlingTests.cs` para chamar `/__test/error`, rota mapeada somente quando o ambiente é `Testing`, e verificar status 500, `application/problem+json`, campos Problem Details e ausência de stack trace/segredos; não expor a rota em produção.
- [X] T018 [P] [US3] Estender `tests/api/SistemaEstoque.API.Tests/HealthEndpointTests.cs` para verificar o contrato `GET /health/ready`: HTTP 200 com dependência disponível, HTTP 503 com dependência indisponível e nenhuma mensagem interna de conexão na resposta.
- [X] T019 [P] [US3] Criar `tests/api/SistemaEstoque.API.Tests/StructuredLoggingTests.cs` para capturar eventos de requisição e falha via `WebApplicationFactory<Program>` e confirmar método, rota, status HTTP e identificador de correlação, sem body, credencial ou segredo.

### Implementation for User Story 3

- [X] T020 [US3] Criar middleware de logging estruturado por requisição em `src/services/api/SistemaEstoque.API/Middleware/RequestLoggingMiddleware.cs` e registrá-lo em `src/services/api/SistemaEstoque.API/Program.cs`; registrar método, rota, status HTTP e identificador de correlação quando disponível, omitindo body, credenciais e dados sensíveis.
- [X] T021 [US3] Configurar o formatter JSON do console integrado e níveis por ambiente em `src/services/api/SistemaEstoque.API/appsettings.json` e `src/services/api/SistemaEstoque.API/appsettings.Development.json`, sem incluir credenciais ou valores sensíveis.
- [X] T022 [US3] Evoluir o tratamento global já registrado em `src/services/api/SistemaEstoque.API/Program.cs` para responder erros inesperados em RFC 7807 sem stack trace, incluir contexto de correlação seguro e manter a exceção disponível nos logs operacionais.
- [X] T023 [US3] Registrar em `src/services/api/SistemaEstoque.API/Program.cs` uma verificação de readiness do `SistemaEstoqueDbContext` e mapear `GET /health/ready` com `503` quando a persistência não estiver disponível; preservar `GET /health` sem dependência do banco.
- [X] T024 [US3] Atualizar `specs/001-solution-foundation/contracts/health-api.yaml` para que respostas reais de `/health`, `/health/ready` e erros RFC 7807 correspondam ao contrato documentado.

**Checkpoint**: Falhas ficam diagnosticáveis nos logs, mas os clientes recebem erros seguros; disponibilidade do host e prontidão PostgreSQL são distinguíveis.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Alinhar documentação, quickstart e gates com o comportamento entregue.

- [X] T025 [P] Atualizar instruções de pré-requisitos, execução web/API, configuração por ambiente e comandos de validação em `README.md` e `specs/001-solution-foundation/quickstart.md`, mantendo exemplos compatíveis com PowerShell e sem segredos reais.
- [X] T026 Revisar o conjunto de mudanças em `src/services/api/`, `src/apps/web/`, `tests/` e `.github/workflows/ci.yml` contra `.specify/memory/constitution.md`, executar build/testes API e web conforme `specs/001-solution-foundation/quickstart.md` e corrigir violações ou falhas antes da conclusão.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Nenhuma dependência; prepara o DbContext e as referências EF Core/Npgsql.
- **Foundational (Phase 2)**: Depende de Setup; bloqueia todas as histórias até haver composição de dependências e teste dos limites de camada.
- **US1 (Phase 3)**: Depende da composição foundational; não depende da persistência nem das outras histórias.
- **US2 (Phase 4)**: Depende da composição foundational; prepara configuração e PostgreSQL descartável.
- **US3 (Phase 5)**: Depende da composição foundational e de T012, T013 e T015 da US2 para verificar readiness real; testes de logging e erros podem ser preparados antes do serviço PostgreSQL.
- **Polish (Phase 6)**: Depende das histórias que serão entregues.

### User Story Dependencies

- **US1 (P1)**: após Phase 2; independente de US2/US3.
- **US2 (P2)**: após Phase 2; independente de US1.
- **US3 (P3)**: após Phase 2; a implementação e teste de readiness real requerem T003, T012, T013 e T015 da US2. T017, T019 e T020 podem ser desenvolvidas sem dependência do PostgreSQL; readiness não depende do teste T011 nem da configuração web T014.

### Within Each User Story

- As tarefas de teste [P] podem ser desenvolvidas em paralelo entre si e antes/junto da implementação.
- Tarefas de configuração e extensão do mesmo arquivo devem ser integradas sequencialmente; em particular, alterações de `Program.cs` em US1/US2/US3 não são paralelas.
- A conexão de banco é registrada antes de configurar readiness; os testes que a exercitam exigem o serviço PostgreSQL do workflow.

### Dependency Graph

```text
T001 -> T002 -> T003 -> T004 -> T005
                         |       |
                         +------>+--> US1 (T006-T009)
                         +----------> US2 (T010-T016)
                                      |
                                      +--> US3 readiness (T018, T023)
                                           (requires T003, T012, T013, T015)
T017, T019 -------------------------------> T020 (request logging middleware)
T017 -------------------------------------> T022 (global error handling)
T003, T012, T013, T015 -> T018 -> T023 (readiness test then endpoint)
T020 -> T022 -> T023 (Program.cs integration order)
US1 + US2 + US3 --------------------------> Polish (T025-T026)
```

### Parallel Opportunities

- **Setup**: T001 seguido por T002; ambos alteram arquivos distintos, mas o DbContext depende do pacote referenciado.
- **Foundational**: após T003, T004 e T005 trabalham em arquivos/projetos distintos e podem avançar em paralelo.
- **US1**: T006 e T007 são paralelas (API test vs. web test); implementação de API e web também pode ser paralela depois dos testes.
- **US2**: T010 e T011 são paralelas; T013 e T014 alteram arquivos distintos; T012/T015/T016 têm dependências ou caminhos separados.
- **US3**: T017, T018 e T019 podem ser escritos em paralelo; executar T018 requer o PostgreSQL de T015. T020, T022 e T023 alteram `Program.cs` e devem ser integrados sequencialmente; T021 configura o formatter antes da validação final dos logs; T024 segue o contrato final.
- **Polish**: T025 pode avançar paralelamente à revisão T026 apenas se as instruções finais forem integradas antes da validação final.

### Parallel Example: User Story 1

```text
Em paralelo:
- T006: teste de integração do contrato GET /health em tests/api/SistemaEstoque.API.Tests/HealthEndpointTests.cs
- T007: teste do estado web em tests/web/HealthStatus.test.tsx

Depois dos testes, em arquivos distintos:
- T008: implementar/preservar endpoint em src/services/api/SistemaEstoque.API/Program.cs
- T009: implementar/preservar estado acessível em src/apps/web/components/HealthStatus.tsx
```

## Implementation Strategy

### MVP First (User Story 1)

1. Completar Phase 1 e Phase 2 para registrar EF/Npgsql e compor as camadas sem alterar a arquitetura.
2. Completar US1 e validar execução web/API e contrato existente de saúde.
3. **Parar e validar** a experiência independente da US1; isso fornece o MVP executável, embora as tarefas de configuração e readiness das fases posteriores ainda sejam necessárias para concluir a feature inteira.

### Incremental Delivery

1. Setup + Foundational: base de persistência e injeção preparada.
2. US1: inicialização reproduzível e saúde observável a partir do frontend.
3. US2: perfis de configuração seguros e persistência isolada/testável.
4. US3: erros padronizados, logs seguros e readiness dependente do banco.
5. Polish: documentação, verificação da constitution e execução do quickstart.

### Parallel Team Strategy

1. Concluir Setup antes de compor as camadas.
2. Concluir o composition root e os testes de arquitetura em Foundational.
3. Desenvolver US1 e US2 em paralelo, desde que `Program.cs` não receba edições concorrentes.
4. Começar testes independentes de erro/logging da US3 em paralelo à US2; finalizar readiness após o provisionamento e registro de persistência da US2.
5. Executar a validação completa após integrar todas as fases.

## Notes

- Cada tarefa tem checkbox, ID sequencial, marcador `[P]` somente quando aplicável, label `[USn]` em fases de histórias e caminho de arquivo.
- Não criar autenticação, CRUD, esquema de negócio, dependências fora da stack aprovada ou novos projetos.
- A aplicação explícita de migrações permanece operação de implantação; esta feature não cria nem aplica migração de negócio.
