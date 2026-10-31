# Tasks: Corrigir carregamento dos campos da estante selecionada

**Input**: Design documents from `/specs/015-corrigir-campos-estante-selecionada/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: Obrigatórios (FR-008 da spec e constituição do projeto). Escreva o teste antes da correção e confirme que ele falha.

## Format: `[ID] [P?] [Story] Description`

## Phase 1: User Story 1 - Ver código e descrição da estante selecionada (Priority: P1) 🎯 MVP

**Goal**: Ao selecionar uma estante, os campos Código e Descrição do formulário "Editar estante" mostram os valores da estante escolhida.

**Independent Test**: Com duas estantes de código/descrição distintos, trocar a seleção e verificar os valores dos campos.

- [X] T001 [US1] Em `tests/web/LocationsPage.test.tsx`, adicionar teste de regressão: `listRacks` retorna duas estantes com códigos e descrições distintos (uma sem descrição); verificar que os campos "Código" e "Descrição" (`edit-rack-code`, `edit-rack-description`) mostram os valores da estante inicial, trocar o seletor `rack-selector` para a outra estante e verificar os novos valores (descrição vazia incluída), depois voltar à primeira. Confirmar que o teste falha antes da correção.
- [X] T002 [US1] Em `tests/web/LocationsPage.test.tsx`, adicionar teste: digitar um valor no campo Código sem salvar e trocar de estante descarta a digitação e mostra os valores da estante escolhida.
- [X] T003 [US1] Em `src/apps/web/app/locations/LocationsPageClient.tsx`, adicionar `key={selectedRack.id}` ao `<form aria-label={`Editar estante ${selectedRack.code}`}>` para remontar os campos a cada estante (não usar código/descrição na key, para não apagar a digitação em refetch).

**Checkpoint**: T001 e T002 passam; os campos acompanham a estante selecionada.

---

## Phase 2: Polish

- [X] T004 Executar `npm run test -w @crud2/web-tests -- LocationsPage.test.tsx` e `npm run lint -w @crud2/web`, e conferir o roteiro manual de `specs/015-corrigir-campos-estante-selecionada/quickstart.md`.

---

## Dependencies & Execution Order

- T001 e T002 (mesmo arquivo) antes de T003; T004 por último.
- Escopo único: MVP = User Story 1. Sem oportunidades de paralelismo (mesmos arquivos).
