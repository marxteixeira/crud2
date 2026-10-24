# Tasks: Configuração segura do banco Supabase

**Input**: Artefatos de design em `specs/004-supabase-database-config/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Incluídos porque a constituição do projeto exige testes automatizados para toda funcionalidade.

**Organization**: Tarefas separadas por história de usuário para permitir implementação e validação incremental.

## Formato

Cada tarefa usa `- [ ] TNNN [P?] [USn?] descrição com caminho de arquivo`. `[P]` indica trabalho paralelo em arquivos diferentes. Tarefas de setup/fundação/polimento não recebem rótulo de história; tarefas de história recebem `[US1]`, `[US2]` ou `[US3]`.

## Phase 1: Setup

**Purpose**: Preparar a documentação do fluxo de configuração sem adicionar dependências.

- [ ] T001 Atualizar `README.md` para apontar ao contrato de configuração do banco e ao quickstart desta feature, sem incluir valores de credenciais.

---

## Phase 2: Foundational

**Purpose**: Habilitar o armazenamento local de configurações privadas e garantir que defaults versionados não contenham credenciais.

- [ ] T002 Adicionar `UserSecretsId` ao projeto `src/services/api/SistemaEstoque.API/SistemaEstoque.API.csproj` para que a API carregue ASP.NET Core User Secrets em desenvolvimento.
- [ ] T003 Remover valores de senha, inclusive placeholders com aparência de credencial, e manter somente defaults locais não secretos em `src/services/api/SistemaEstoque.API/appsettings.Development.json` e `src/services/api/SistemaEstoque.API/appsettings.Testing.json`.

**Checkpoint**: As histórias podem iniciar com User Secrets disponíveis localmente e arquivos de configuração versionados sem credenciais.

---

## Phase 3: User Story 1 - Conectar o sistema ao banco hospedado (Priority: P1)

**Goal**: Permitir que a API conecte ao PostgreSQL por uma configuração de ambiente segura, valide a conexão TLS e preserve prontidão e migrações explícitas.

**Independent Test**: Com o PostgreSQL descartável do CI, verificar que a configuração segura aceita conexão válida, rejeita configuração de produção sem TLS autenticado, retorna readiness de acordo com a disponibilidade e não executa migrações no startup.

### Tests for User Story 1

- [ ] T004 [P] [US1] Adicionar testes em `tests/api/SistemaEstoque.API.Tests/ConfigurationTests.cs` para exigir `VerifyFull` em configuração de produção, recusar TLS desativado ou validação permissiva e aceitar configurações locais de teste isolado.
- [ ] T005 [P] [US1] Estender `tests/api/SistemaEstoque.API.Tests/HealthEndpointTests.cs` para verificar `/health` independente do banco e `/health/ready` com `200` para banco acessível e `503` para indisponibilidade, sem detalhes de conexão na resposta.

### Implementation for User Story 1

- [ ] T006 [US1] Validar no host em `src/services/api/SistemaEstoque.API/Program.cs` o modo TLS da connection string por ambiente, exigindo validação de certificado e hostname em produção e preservando a configuração explícita do PostgreSQL isolado em testes locais/CI.
- [ ] T007 [US1] Preservar o uso de `ConnectionStrings:Default` e a validação sem ecoar valores em `src/services/api/SistemaEstoque.Infrastructure/DependencyInjection.cs`; garantir que a configuração Npgsql aceita a connection string aprovada do endpoint Supabase, incluindo caracteres especiais e CA quando configurada.
- [ ] T008 [US1] Detalhar em `specs/004-supabase-database-config/contracts/database-configuration.md` e `specs/004-supabase-database-config/quickstart.md` o provisionamento operacional de role runtime sem DDL, role separada para migrações com grants DDL mínimos e execução explícita de migrations por conexão direta.

**Checkpoint**: A API conecta ao banco de teste com prontidão correta; uma credencial de produção não pode ser usada com TLS permissivo, e startup não aplica migrations.

---

## Phase 4: User Story 2 - Proteger e manter as credenciais (Priority: P1)

**Goal**: Restringir segredos ao servidor e às fontes próprias de cada ambiente, tratar erros sem exposição e documentar rotação e isolamento de testes.

**Independent Test**: Usando credenciais falsas em testes e PostgreSQL descartável, confirmar que configurações ausentes/inválidas falham sem revelar valores, que nenhuma credencial real é necessária no CI e que a documentação descreve rotação segura.

### Tests for User Story 2

- [ ] T009 [P] [US2] Criar `tests/api/SistemaEstoque.API.Tests/SecretConfigurationTests.cs` para verificar que configuração ausente/inválida e falhas de conectividade não expõem senha, connection string ou segredo administrativo em exceções, logs ou respostas de saúde.
- [ ] T010 [P] [US2] Estender `tests/api/SistemaEstoque.API.Tests/PersistenceConnectivityTests.cs` para exercitar apenas o PostgreSQL isolado de teste e confirmar que o teste não depende de configuração Supabase nem de credenciais de produção.

### Implementation for User Story 2

- [ ] T011 [US2] Sanitizar erros de parsing e conexão sem preservar exceções internas que contenham credenciais em `src/services/api/SistemaEstoque.Infrastructure/DependencyInjection.cs` e `src/services/api/SistemaEstoque.Infrastructure/Persistence/DatabaseReadinessHealthCheck.cs`.
- [ ] T012 [US2] Garantir em `.github/workflows/ci.yml` que testes da API usam somente o serviço PostgreSQL descartável com credenciais de teste e não recebem segredos ou endpoints do projeto Supabase.
- [ ] T013 [US2] Documentar em `README.md` e `specs/004-supabase-database-config/quickstart.md` configuração local por User Secrets, armazenamento no secret manager em produção, limitações de proteção local dos User Secrets e rotação obrigatória da senha compartilhada antes do uso.

**Checkpoint**: Segredos não aparecem em arquivos versionados, logs, respostas ou bundle web; CI e desenvolvimento usam ambientes isolados e o procedimento de rotação é executável sem alterar código.

---

## Phase 5: User Story 3 - Distinguir conexão de banco de configuração de cliente (Priority: P2)

**Goal**: Usar somente configuração pública no cliente Supabase web, sem confundir chave publicável com credencial PostgreSQL ou segredo administrativo.

**Independent Test**: Com somente URL e chave publicável falsas configuradas no web, verificar que os clientes browser/server e middleware inicializam e que nenhum segredo do banco ou chave administrativa é requerido ou incluído no bundle.

### Tests for User Story 3

- [ ] T014 [P] [US3] Criar `tests/web/SupabaseConfiguration.test.ts` para cobrir a leitura da URL e chave publicável e falhar quando a configuração pública obrigatória estiver ausente, sem exigir connection string do banco.

### Implementation for User Story 3

- [ ] T015 [US3] Atualizar os consumidores em `src/apps/web/lib/supabase/client.ts`, `src/apps/web/lib/supabase/server.ts` e `src/apps/web/middleware.ts` para usar `NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY` e nunca ler configuração de banco ou chaves administrativas.
- [ ] T016 [US3] Atualizar `src/apps/web/.env.example` com `NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY` vazio e remover o nome legado de chave pública depois de atualizar todos os consumidores e testes.
- [ ] T017 [US3] Atualizar testes e setup do web em `tests/web/AuthFlow.test.tsx` e `tests/web/setup.ts` para cobrir a configuração publicável sem incluir segredos de servidor.

**Checkpoint**: O cliente web usa somente URL e chave publicável; autenticação existente permanece funcional e o bundle não depende da senha PostgreSQL nem de chave administrativa.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validar os cenários documentados e resolver divergências finais.

- [ ] T018 [P] Executar os comandos de validação de `specs/004-supabase-database-config/quickstart.md` para API e web e corrigir falhas nos arquivos afetados.
- [ ] T019 [P] Revisar `src/services/api/SistemaEstoque.API/appsettings.json`, `src/services/api/SistemaEstoque.API/appsettings.Development.json`, `src/services/api/SistemaEstoque.API/appsettings.Testing.json`, `src/apps/web/.env.example` e o diff da feature para confirmar que nenhum segredo ou connection string de produção foi versionado.

---

## Dependências e ordem de execução

### Dependências entre fases

- **Setup (Phase 1)**: inicia a documentação de entrada; sem dependências.
- **Foundational (Phase 2)**: depende de T001 e bloqueia configuração local segura nas histórias.
- **US1 (Phase 3)**: depende de T002-T003; testes T004-T005 precedem a implementação T006-T008.
- **US2 (Phase 4)**: depende de T002-T003; os testes T009-T010 podem ser desenvolvidos independentemente e precedem T011-T013.
- **US3 (Phase 5)**: depende de T002-T003; T014 precede T015-T017. Pode ser implementada em paralelo com US1 e US2 após a fundação.
- **Polimento (Phase 6)**: depende das histórias selecionadas e de seus checkpoints.

### Dependências entre histórias

- **US1 (P1)**: pode começar após a fundação; não depende de outras histórias.
- **US2 (P1)**: pode começar após a fundação; não depende de US1 ou US3.
- **US3 (P2)**: pode começar após a fundação; não depende de US1 ou US2.
- Para implantação do serviço hospedado, completar US1 e US2 antes de habilitar tráfego; US3 é necessário aos fluxos de autenticação do cliente web, não à conexão PostgreSQL da API.

## Oportunidades de paralelização

- **Setup/Fundação**: T002 e T003 podem ser feitas em paralelo depois de T001; arquivos distintos.
- **US1**: T004 e T005 podem ser escritos em paralelo; arquivos de teste distintos. T006-T008 começam depois de ambos.
- **US2**: T009 e T010 podem ser escritos em paralelo; arquivos de teste distintos. T011-T013 começam depois de ambos.
- **US3**: T014 é independente das histórias US1/US2 após a fundação; T015 e T016 compartilham contrato de nome, então devem ser integrados com cuidado, e T017 valida os consumidores atualizados.
- **Entre histórias**: US1, US2 e US3 podem ser executadas por responsáveis diferentes após T002-T003, respeitando a ordem interna de cada história.
- **Polimento**: T018 e T019 podem ser executadas em paralelo após a implementação escolhida.

## Exemplo de paralelização: US1

```text
Executor A: T004 - testes de configuração TLS em ConfigurationTests.cs
Executor B: T005 - testes HTTP de health/readiness em HealthEndpointTests.cs
Após ambos:
Executor A: T006-T007 - validação TLS no host e configuração Npgsql
Executor B: T008 - procedimento de roles e migrations no contrato/quickstart
```

## Estratégia de implementação

### MVP recomendado

Implementar **US1 e US2 juntas como MVP operacional**:

1. Completar Setup e Foundational (T001-T003).
2. Implementar US1 para conexão segura, readiness e migrações explícitas (T004-T008).
3. Implementar US2 para fontes de segredos, sanitização, CI isolado e rotação (T009-T013).
4. Validar independentemente com o PostgreSQL descartável do CI e o quickstart; não conectar testes à produção.

US1 sozinha demonstra conectividade, mas não deve ser habilitada em produção até que US2 proteja e isole as credenciais. Implementar US3 em seguida para padronizar a chave publicável entregue pelo usuário ao web, sem impacto na conexão do banco.

### Entrega incremental

1. Setup + Foundational → configuração local segura disponível.
2. US1 → conexão, TLS, readiness e migrações explícitas validados em banco isolado.
3. US2 → segredos e rotação protegidos; condição necessária para ativar ambiente hospedado.
4. US3 → cliente web usa somente URL e chave publicável.
5. Polimento → quickstart, testes e revisão de segredos.

## Validação do formato

- Todas as tarefas usam checkbox `- [ ]`, ID sequencial `T001`–`T019` e caminho de arquivo.
- Tarefas das histórias usam rótulo `[US1]`, `[US2]` ou `[US3]`; Setup, Foundational e Polish não usam rótulo de história.
- `[P]` aparece somente nas tarefas independentes em arquivos distintos.
