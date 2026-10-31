---

description: "Tarefas para recuperar o cadastro de prateleiras na tela de Localizações"
---

# Tasks: Recuperar cadastro de prateleira

**Input**: Design documents from `specs/016-recuperar-cadastro-prateleira/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/locations-ui.md, quickstart.md

**Tests**: Testes automatizados incluídos porque FR-012 da especificação exige cobertura para cadastro válido, código obrigatório, duplicidade por estante e reutilização do código em outra estante.

**Organization**: Tarefas agrupadas por história de usuário; as funções de API, validações de domínio e infraestrutura de testes já existem e não serão recriadas.

## Phase 1: Setup

**Purpose**: Preparar infraestrutura compartilhada.

Não há tarefas de setup: Vitest, Testing Library, cliente de API e ambiente de desenvolvimento já estão configurados.

## Phase 2: Foundational

**Purpose**: Pré-requisitos compartilhados que bloqueiam as histórias.

Não há tarefas fundacionais: `createShelf`, o endpoint autenticado e as regras de validação e persistência já existem.

---

## Phase 3: User Story 1 - Cadastrar prateleira na estante selecionada (Priority: P1) - MVP

**Goal**: Reintroduzir o cadastro de uma prateleira para a estante ativa selecionada e exibi-la na lista sem atualização manual.

**Independent Test**: Selecionar uma estante ativa, cadastrar um código inédito e confirmar que a nova prateleira aparece nessa estante, que a área vazia desaparece quando aplicável e que os fluxos existentes continuam disponíveis.

### Tests for User Story 1

- [X] T001 [US1] Adicionar em `tests/web/LocationsPage.test.tsx` testes do cadastro de prateleira bem-sucedido, incluindo associação à estante ativa selecionada, inclusão após atualização da lista e substituição da expectativa de que os controles de cadastro não aparecem.

### Implementation for User Story 1

- [X] T002 [US1] Em `src/apps/web/app/locations/LocationsPageClient.tsx`, reintroduzir o campo obrigatório "Código da prateleira" e o botão "Cadastrar prateleira" somente para uma estante ativa selecionada; chamar `createShelf` com o ID da estante capturado no envio, impedir submissões repetidas, atualizar a lista após sucesso e preservar edição, situação e consulta de produtos.

**Checkpoint**: A pessoa pode cadastrar uma prateleira válida e vê-la na estante correta sem ação manual adicional.

---

## Phase 4: User Story 2 - Receber validação clara ao cadastrar (Priority: P2)

**Goal**: Comunicar código obrigatório, duplicidade e falhas sem perder os dados digitados ou atribuir a operação à estante errada.

**Independent Test**: Com a estante ativa selecionada, tentar enviar código vazio, repetir um código existente na mesma estante e cadastrar o mesmo código em outra estante; confirmar validação apropriada, ausência de duplicatas e preservação do código em caso de falha.

### Tests for User Story 2

- [X] T003 [US2] Adicionar em `tests/web/LocationsPage.test.tsx` testes para código obrigatório sem chamada à API, duplicidade recusada na mesma estante, mesmo código aceito em outra estante e falha de criação com valor informado preservado e mensagem de erro visível.

### Implementation for User Story 2

- [X] T004 [US2] Em `src/apps/web/app/locations/LocationsPageClient.tsx`, garantir validação de código não vazio após trim, mensagens de erro claras para validação/conflito/falha e preservação do formulário em falhas; após criação persistida, atualizar os dados locais com a prateleira retornada e distinguir falha posterior de atualização da lista de falha na criação, evitando sugerir repetição de um cadastro já concluído.

**Checkpoint**: Código vazio e duplicado não criam registros; códigos repetidos em estantes diferentes continuam permitidos, e falhas não apagam dados nem geram falsa indicação de sucesso.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validar a entrega completa e verificar regressões nos fluxos relacionados.

- [X] T005 Executar o teste focal indicado em `specs/016-recuperar-cadastro-prateleira/quickstart.md` e corrigir eventuais regressões no cadastro e manutenção de estantes e prateleiras.
- [X] T006 Executar a suíte web indicada em `specs/016-recuperar-cadastro-prateleira/quickstart.md` e confirmar que os fluxos existentes de seleção de localização de produtos continuam disponíveis.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Nenhuma dependência; sem tarefas porque a infraestrutura já existe.
- **Foundational (Phase 2)**: Sem tarefas; API, validação, persistência e cliente estão implementados.
- **User Stories (Phase 3+)**: Independentes da API nova, mas executadas em ordem de prioridade; a US2 depende da interface reintroduzida pela US1.
- **Polish (Phase 5)**: Depende da conclusão das histórias desejadas; T006 depende de T005.

### User Story Dependencies

- **User Story 1 (P1)**: Pode começar imediatamente; não depende de outra história.
- **User Story 2 (P2)**: Executar após US1 porque amplia os estados de validação e erro do mesmo formulário e dos mesmos testes.

### Within Each User Story

- Escrever ou atualizar os testes antes da implementação.
- Implementar o fluxo e seus estados depois que os testes estiverem definidos.
- Validar a história independentemente antes de avançar.

### Parallel Opportunities

- Não há tarefas de código marcadas `[P]`: as histórias modificam o mesmo arquivo de página e o mesmo arquivo de testes, então executá-las em paralelo criaria conflito.
- T005 e T006 são sequenciais, pois a suíte completa deve confirmar o resultado do teste focal e da implementação final.

## Parallel Example: User Story 1

As tarefas de teste e implementação compartilham arquivos e têm dependência intencional; execute-as sequencialmente:

```text
T001 -> T002
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Manter a infraestrutura existente; nenhum setup ou trabalho fundacional é necessário.
2. Executar T001 para cobrir o fluxo de sucesso e a restauração dos controles.
3. Executar T002 para recuperar o cadastro usando o cliente e o endpoint existentes.
4. Validar o cenário independente da US1 antes de expandir o escopo.

### Incremental Delivery

1. Concluir US1 e validar o cadastro de prateleira válida na estante selecionada.
2. Concluir US2 e validar campos vazios, duplicidade, reutilização entre estantes e falhas.
3. Executar T005 e T006 para validar os testes focalizados e a suíte web.

Cada etapa preserva os recursos de edição, situação e consulta de produtos já existentes.
