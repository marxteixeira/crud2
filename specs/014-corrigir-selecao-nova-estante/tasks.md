# Tasks: Corrigir seleção da nova estante após o cadastro

**Input**: Design documents from `/specs/014-corrigir-selecao-nova-estante/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: Obrigatórios (FR-008 da spec e constituição do projeto). Escreva o teste antes da correção e confirme que ele falha.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: User Story 1 - Ver a nova estante selecionada após cadastrá-la (Priority: P1) 🎯 MVP

**Goal**: Após cadastrar uma estante, a nova estante fica selecionada, sem voltar à primeira da lista.

**Independent Test**: Com estantes existentes, cadastrar uma nova com o refetch da lista atrasado e verificar que a nova estante permanece selecionada antes e depois de o refetch concluir.

- [X] T001 [US1] Em `tests/web/LocationsPage.test.tsx`, adicionar teste de regressão: `listRacks` retorna a lista antiga até uma promise controlada ser resolvida; cadastrar a estante pelo modal e verificar que o seletor de estante mostra a nova estante (e não a primeira) imediatamente após fechar o modal e depois que o refetch resolver com a nova estante incluída. Confirmar que o teste falha antes da correção.
- [X] T002 [US1] Em `tests/web/LocationsPage.test.tsx`, adicionar testes de borda: falha no cadastro mantém a estante selecionada e o modal aberto; escolha manual de outra estante após o cadastro é respeitada.
- [X] T003 [US1] Em `src/apps/web/app/locations/LocationsPageClient.tsx`, no `onSuccess` de `createRackMutation`, inserir a estante criada no cache `["locations", "racks"]` com `queryClient.setQueryData` (sem duplicar o id), depois chamar `setSelectedRackId(rack.id)` e `await refresh()`.

**Checkpoint**: T001 e T002 passam; a nova estante fica selecionada.

---

## Phase 2: Polish

- [X] T004 Executar `npm run test -w @crud2/web-tests -- LocationsPage.test.tsx` e `npm run lint -w @crud2/web`, e conferir o roteiro manual de `specs/014-corrigir-selecao-nova-estante/quickstart.md`.

---

## Dependencies & Execution Order

- T001 e T002 (mesmo arquivo, sem `[P]`) antes de T003; T004 por último.

## Implementation Strategy

MVP = User Story 1 (única história): testes falhando → correção → validação.
