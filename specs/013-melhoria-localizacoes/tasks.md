# Tasks: Melhorar a experiência de Localizações

**Input**: Design documents from `specs/013-melhoria-localizacoes/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/locations-ui.md`

**Tests**: Testes automatizados incluídos conforme a constituição do projeto, que exige cobertura para funcionalidades de interface com comportamento.

**Organization**: Tarefas agrupadas por história de usuário para permitir implementação e validação por incremento.

## Phase 1: Setup

**Purpose**: Preparar estrutura compartilhada do projeto.

Nenhuma tarefa de setup necessária: a aplicação web, o componente `Dialog` e o teste de Localizações já existem nos caminhos definidos em `plan.md`.

---

## Phase 2: Foundational

**Purpose**: Pré-requisitos comuns que bloqueiam as histórias.

Nenhuma tarefa fundacional necessária: não há mudanças em dependências, API, autenticação, persistência ou estrutura do projeto.

---

## Phase 3: User Story 1 - Cadastrar estante pelo modal (Priority: P1) 🎯 MVP

**Goal**: Abrir o cadastro de estante sob demanda e, após salvar, fechar o modal, atualizar a lista e deixar a nova estante selecionada.

**Independent Test**: Na página de Localizações, confirmar que o formulário está inicialmente fechado; abri-lo pelo botão, cancelar sem salvar e, em seguida, cadastrar uma estante válida e confirmar que o diálogo fecha e a nova estante aparece selecionada. Simular uma falha e confirmar que o diálogo e os valores digitados permanecem.

### Tests for User Story 1

- [X] T001 [US1] Atualizar `tests/web/LocationsPage.test.tsx` para validar formulário inicialmente fechado, abertura pelo botão, cancelamento sem chamada de criação, sucesso com fechamento e estante criada selecionada, e falha preservando diálogo e valores digitados.

### Implementation for User Story 1

- [X] T002 [US1] Em `src/apps/web/app/locations/LocationsPageClient.tsx`, substituir o link-âncora por botão que abre o componente `Dialog`, mover o formulário de cadastro para o modal e fechá-lo/limpá-lo somente após sucesso; preservar campos e mensagem de erro em caso de falha e manter a seleção da estante retornada após atualizar a consulta.

**Checkpoint**: A pessoa consegue abrir e cancelar o cadastro e criar estante pelo modal com atualização e seleção automáticas.

---

## Phase 4: User Story 2 - Consultar prateleiras sem controles redundantes (Priority: P2)

**Goal**: Exibir campos de nova estante vazios e sem placeholders, retirar a criação de prateleiras da tela e preservar a consulta e a manutenção dos registros existentes.

**Independent Test**: Abrir o modal e confirmar que Código e Descrição começam vazios sem texto de exemplo; selecionar estante com prateleiras e confirmar que a lista, edição, situação e consulta de produtos continuam disponíveis, mas não há campo "Código da prateleira" nem botão "Cadastrar prateleira".

### Tests for User Story 2

- [X] T003 [US2] Atualizar `tests/web/LocationsPage.test.tsx` para verificar os campos Código e Descrição vazios e sem placeholder no modal, ausência dos controles de criação de prateleira e permanência das prateleiras existentes com suas ações de consulta e edição.

### Implementation for User Story 2

- [X] T004 [US2] Em `src/apps/web/app/locations/LocationsPageClient.tsx`, remover o formulário de cadastro de prateleira e seu campo/botão, removendo também o estado e imports de criação que ficarem sem uso; manter a lista, edição, alteração de situação e consulta de produtos das prateleiras existentes e garantir que os campos de novo cadastro de estante não tenham valores nem placeholders.

**Checkpoint**: A seção continua permitindo consultar e manter prateleiras existentes sem controles redundantes de criação.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Confirmar a qualidade e a compatibilidade das duas histórias.

- [X] T005 Executar os testes focados de `tests/web/LocationsPage.test.tsx` e o lint do workspace web para validar as mudanças da página de Localizações.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Nenhuma tarefa necessária; estrutura e ferramentas já existem.
- **Foundational (Phase 2)**: Nenhuma tarefa necessária; nenhuma dependência compartilhada nova foi introduzida.
- **User Stories (Phase 3+)**: Ambas dependem somente do estado atual funcional da aplicação e dos componentes/testes existentes.
- **Polish (Phase 5)**: Depende da conclusão das duas histórias.

### User Story Dependencies

- **User Story 1 (P1)**: Pode ser implementada primeiro e entregue como MVP.
- **User Story 2 (P2)**: Funcionalmente independente de US1, mas recomenda-se executá-la depois de US1 porque ambas alteram `src/apps/web/app/locations/LocationsPageClient.tsx` e `tests/web/LocationsPage.test.tsx`, evitando conflitos nos mesmos arquivos.

### Within Each User Story

- Os testes em cada história devem ser adicionados antes ou junto da implementação e cobrir seus critérios independentes.
- T002 depende da cobertura prevista em T001; T004 depende da cobertura prevista em T003.
- T005 executa a validação depois que as duas histórias estiverem implementadas.

### Dependency Graph

```text
T001 -> T002 -> T003 -> T004 -> T005
```

## Parallel Opportunities

- Não há paralelismo seguro entre tarefas de histórias distintas, pois ambas editam os mesmos dois arquivos.
- Dentro de cada história, teste e implementação devem ser executados em sequência para validar o comportamento antes/depois; nenhuma tarefa foi marcada `[P]`.

## Parallel Example: User Story 1

```text
Executar T001 para especificar os comportamentos no arquivo de teste; em seguida, executar T002 no componente de Localizações.
```

## Parallel Example: User Story 2

```text
Após concluir US1, executar T003 para cobrir a simplificação da seção; em seguida, executar T004 no componente compartilhado com US1.
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Concluir T001 e T002.
2. Validar o modal, cancelamento, sucesso, atualização/seleção e preservação do formulário em falha.
3. Demonstrar o cadastro de estante sem implementar a simplificação de prateleiras.

### Incremental Delivery

1. Entregar US1 como MVP testável.
2. Implementar US2, preservando as ações sobre prateleiras já cadastradas.
3. Executar T005 para testar e aplicar lint ao resultado combinado.
