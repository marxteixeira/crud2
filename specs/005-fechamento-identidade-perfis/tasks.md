# Tasks: Fechamento da fase de identidade e perfis

**Input**: Documentos de design em `specs/005-fechamento-identidade-perfis/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `quickstart.md`

**Tests**: Não há implementação de produto nesta feature. As tarefas executam e registram os gates automatizados e as validações manuais/de desempenho/usabilidade que já são critérios obrigatórios das fases 003 e 004.

**Organization**: Tarefas organizadas por história de usuário. A decisão de transição depende do fechamento das evidências da US1.

## Formato

Cada tarefa segue `- [ ] TNNN [P?] [USn?] descrição com caminho(s) exato(s)`. `[P]` identifica trabalho paralelo independente em arquivos distintos. Tarefas de histórias incluem seu rótulo obrigatório.

## Convenções de registro

- Registrar somente resultados agregados e reprodutíveis em `specs/005-fechamento-identidade-perfis/readiness-report.md`.
- Não registrar senhas, tokens, connection strings, e-mails, identificadores pessoais nem credenciais.
- Para cada critério, registrar resultado `approved`, `failed`, `blocked` ou `not-run`, data, ambiente não secreto e próxima ação necessária.
- Não marcar tarefa de origem em `specs/003-identidade-perfis/tasks.md` ou `specs/004-supabase-database-config/tasks.md` como concluída sem evidência aprovada atual.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar um registro consistente para as evidências e a decisão desta avaliação.

- [ ] T001 Criar `specs/005-fechamento-identidade-perfis/readiness-report.md` com tabela de critérios, resultado, data, ambiente, evidência agregada e ação pendente; incluir os itens T036, T037, T039 e T040 de `specs/003-identidade-perfis/tasks.md` e T018 de `specs/004-supabase-database-config/tasks.md`, inicialmente como pendentes até confirmação de evidência atual.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Confirmar quais fontes e critérios governam o fechamento antes de executar validações.

**Critical**: As histórias não podem declarar prontidão antes que os critérios de origem e o registro estejam alinhados.

- [ ] T002 Conferir os critérios de saída da Fase 1 e de entrada da Fase 2 em `specs/002-implementation-roadmap/spec.md`, os estados das tarefas em `specs/003-identidade-perfis/tasks.md` e `specs/004-supabase-database-config/tasks.md`, e reconciliar a matriz em `specs/005-fechamento-identidade-perfis/readiness-report.md` sem alterar status sem evidência.

**Checkpoint**: Fontes de verdade e lista completa de validações impeditivas estão registradas; US1 pode recolher evidências.

---

## Phase 3: User Story 1 - Confirmar a conclusão da fase (Priority: P1) 🎯 MVP

**Goal**: Determinar com evidência atual quais gates automatizados, operacionais, de desempenho e usabilidade passaram, falharam ou ainda estão bloqueados.

**Independent Test**: A matriz em `readiness-report.md` cobre T036, T037, T039, T040 e T018, apresenta resultado e evidência para cada critério e distingue falha, bloqueio ambiental e teste não executado sem dados confidenciais.

### Validações automatizadas e correções

- [ ] T003 Reexecutar sequencialmente `dotnet build src/services/api/SistemaEstoque.sln` e `dotnet test src/services/api/SistemaEstoque.sln`; reproduzir e corrigir falhas de configuração de banco, registro duplicado de autenticação ou configuração de conectividade nos projetos `src/services/api/` e `tests/api/`, sem usar banco de produção nem registrar segredos; anotar comando, totais e causa comprovada em `specs/005-fechamento-identidade-perfis/readiness-report.md`.
- [ ] T004 Na raiz do repositório, executar `npm run lint`, `npm test` e `npm run build` usando os scripts npm e workspaces configurados em `package.json`; registrar código de saída e contagem de testes em `specs/005-fechamento-identidade-perfis/readiness-report.md`, sem incluir dados sensíveis.

### Validações operacionais

- [ ] T005 Executar os cenários manuais de `specs/003-identidade-perfis/quickstart.md` com Supabase de desenvolvimento isolado e PostgreSQL de teste; registrar resultado agregado para login nos dois perfis, permissões, mudança de perfil, inativação, logout/revogação e último Administrador em `specs/005-fechamento-identidade-perfis/readiness-report.md`; marcar T036 em `specs/003-identidade-perfis/tasks.md` somente após concluir os cenários.
- [ ] T006 Completar a validação T018 de `specs/004-supabase-database-config/quickstart.md` com PostgreSQL descartável, verificando conectividade, readiness, TLS conforme ambiente, isolamento de credenciais e ausência de migrations automáticas; registrar resultados em `specs/005-fechamento-identidade-perfis/readiness-report.md` e marcar T018 em `specs/004-supabase-database-config/tasks.md` somente se todos os critérios passarem.

### Desempenho e usabilidade

- [ ] T007 Criar `tests/api/SistemaEstoque.API.Tests/UserManagementPerformanceTests.cs` para preparar 1.000 contas sintéticas, aquecer a listagem paginada e medir ao menos 100 requisições; exigir p95 máximo de 3 segundos e garantir limpeza dos dados de teste sem acessar ambientes reais.
- [ ] T008 Executar o teste de desempenho de `tests/api/SistemaEstoque.API.Tests/UserManagementPerformanceTests.cs` no ambiente documentado; registrar método, ambiente e p95 agregado em `specs/005-fechamento-identidade-perfis/readiness-report.md` e marcar T039 em `specs/003-identidade-perfis/tasks.md` somente se o limite for atingido.
- [ ] T009 Conduzir o roteiro de usabilidade de `specs/003-identidade-perfis/quickstart.md` com pelo menos cinco participantes que representem os perfis; medir provisionamento, consulta, edição e inativação em até 3 minutos e conclusão sem ajuda de pelo menos 90%; guardar somente totais/medidas agregadas em `specs/005-fechamento-identidade-perfis/readiness-report.md` e marcar T040 em `specs/003-identidade-perfis/tasks.md` somente se as metas forem atingidas.

**Checkpoint**: Todos os resultados obrigatórios foram coletados e avaliados; qualquer falha ou validação não executada permanece explicitamente impeditiva para a US2.

---

## Phase 4: User Story 2 - Decidir a transição para a próxima fase (Priority: P1)

**Goal**: Aplicar os critérios do roadmap às evidências atuais e registrar uma decisão reproduzível de bloqueio ou liberação de Localização e produtos.

**Independent Test**: Duas pessoas revisoras conseguem chegar ao mesmo resultado a partir da matriz; a decisão `ready` só aparece se todos os critérios obrigatórios estiverem aprovados e nenhuma pendência impeditiva estiver aberta.

### Avaliação de prontidão

- [ ] T010 Comparar cada evidência em `specs/005-fechamento-identidade-perfis/readiness-report.md` com os critérios de saída/entrada em `specs/002-implementation-roadmap/spec.md`; manter como `blocked` qualquer critério obrigatório com resultado `failed`, `blocked` ou `not-run`, e registrar sua causa e ação seguinte no mesmo relatório.
- [ ] T011 Solicitar revisão independente de duas pessoas e registrar no `specs/005-fechamento-identidade-perfis/readiness-report.md` a conclusão individual e a decisão final; definir `ready` e atualizar a situação do relatório somente se todos os gates obrigatórios forem `approved`, caso contrário manter `blocked` com as pendências explicitadas.

**Checkpoint**: A decisão de avançar ou bloquear está assinada por evidências atuais, pode ser reproduzida por duas pessoas e não contém dados pessoais ou segredos.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: T001 cria a estrutura do registro; sem dependências anteriores.
- **Foundational (Phase 2)**: T002 depende de T001 e confirma a lista integral de critérios; bloqueia o fechamento das histórias.
- **US1 (Phase 3)**: T003–T009 dependem de T002. T003/T004 e T005/T006/T007/T009 podem avançar separadamente quando seus ambientes estiverem disponíveis; T008 depende de T007.
- **US2 (Phase 4)**: T010 e T011 dependem da conclusão e registro das evidências de US1; T011 depende de T010.

### User Story Dependencies

- **US1 (P1)**: Pode iniciar após a fundação; reúne as evidências de validação e não depende de outra história.
- **US2 (P1)**: Depende de US1; não pode liberar a fase de localização e produtos com evidências ausentes ou falhas.

### Dependency Graph

```text
T001 -> T002
T002 -> T003, T004, T005, T006, T007, T009
T007 -> T008
T003-T009 -> T010 -> T011
```

### Parallel Opportunities

- Após T002, T003 (API) e T004 (web) são independentes e alteram/registram evidência no relatório comum; por isso executar sequencialmente ao consolidar o mesmo arquivo, embora as verificações técnicas possam ocorrer em paralelo por responsáveis diferentes.
- T005 (cenários manuais), T006 (persistência), T007 (teste de desempenho) e T009 (usabilidade) usam ambientes/arquivos diferentes e podem ser executados em paralelo por pessoas diferentes; consolidar seus resultados no relatório após cada execução.
- T008 só começa após T007 e a existência do teste de desempenho.
- US2 não pode ser paralelizada com a coleta de evidências incompleta de US1.

## Parallel Example: User Story 1

```text
Responsável A: T003 - validar/corrigir gates API e registrar resultado
Responsável B: T004 - validar gates web e registrar resultado
Responsável C: T005 e T006 - executar roteiros isolados de identidade e persistência
Responsável D: T007 e T009 - preparar teste de desempenho e conduzir usabilidade
Após T007: executar T008 e consolidar todos os resultados em readiness-report.md
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Criar a matriz e confirmar seus critérios de origem (T001–T002).
2. Reproduzir os gates automatizados e coletar evidências das validações restantes (T003–T009).
3. Parar e confirmar que a US1 distingue claramente aprovado, falho, bloqueado e não executado.
4. Não liberar a próxima fase como resultado do MVP; a liberação exige completar a decisão da US2.

### Incremental Delivery

1. Setup + Foundational → matriz de critérios alinhada.
2. US1 → resultados atuais de testes, operação, desempenho e usabilidade documentados.
3. US2 → revisão independente e decisão reproduzível sobre localização e produtos.

## Format Validation

- As tarefas estão numeradas sequencialmente de T001 a T011.
- Cada linha executável começa com `- [ ]`, contém ID, inclui `[US1]` ou `[US2]` nas fases de histórias e aponta para caminho(s) exato(s) de arquivo ou artefato.
- Não há tarefas de teste de produto adicionadas sem solicitação; T007 valida um critério de desempenho que já é obrigatório na especificação de identidade/perfis.
