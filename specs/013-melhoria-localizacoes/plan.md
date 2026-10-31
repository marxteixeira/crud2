# Implementation Plan: Melhorar a experiência de Localizações

**Branch**: `013-melhoria-localizacoes` | **Date**: 2026-10-30 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/013-melhoria-localizacoes/spec.md`

## Summary

Melhorar a página de Localizações movendo o cadastro de estantes para um modal acionado pelo botão principal. Após salvar com sucesso, fechar o modal, atualizar a lista e selecionar a estante criada. Simplificar a área de prateleiras removendo o controle de criação redundante, sem afetar a consulta, edição ou situação das prateleiras existentes.

## Technical Context

**Language/Version**: TypeScript, React 18, Next.js 14

**Primary Dependencies**: TanStack Query v5; existing accessible `Dialog` component; existing locations client/API helpers

**Storage**: Existing location service; no schema or persistence changes

**Testing**: Vitest + Testing Library, focused on `tests/web/LocationsPage.test.tsx`

**Target Platform**: Responsive web application

**Project Type**: Frontend application in a web/API monorepo

**Performance Goals**: Saving a rack must refresh the visible location data without requiring a manual page reload

**Constraints**: Preserve strict TypeScript, existing API behavior, accessibility and design-system tokens; do not introduce dependencies

**Scale/Scope**: One Localizações page, its rack-creation dialog, shelf controls and focused web tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Evidence |
|---|---|---|
| Clean Architecture / monorepo boundaries | PASS | UI-only change stays in `src/apps/web`; no API or data-layer changes |
| Required automated tests | PASS | Behavior is covered by the existing Vitest/Testing Library suite in `tests/web` |
| Strict typing and error handling | PASS | Reuse existing typed mutations and their user-visible error handling |
| Frontend and accessibility standards | PASS | Reuse the shared `Dialog`; retain keyboard dismissal, labels and established Tailwind design tokens |
| Design system / screen prototype | PASS | Consulted `docs/05-design-system/estantes_e_prateleiras/screen.png` and `code.html`; adapt the rack-creation content into the requested modal and remove only the redundant shelf-creation controls |
| New architecture or dependencies | PASS | None |

## Project Structure

### Documentation (this feature)

```text
specs/013-melhoria-localizacoes/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── locations-ui.md
└── tasks.md             # Created by /speckit-tasks
```

### Source Code (repository root)

```text
src/apps/web/
├── app/locations/LocationsPageClient.tsx
└── components/ui/Dialog.tsx

tests/web/
└── LocationsPage.test.tsx
```

**Structure Decision**: Manter a implementação na página existente de Localizações e reutilizar o componente compartilhado de diálogo. Validar o comportamento no teste de página já existente; não criar módulos nem diretórios de código novos.

## Complexity Tracking

Não há violações da constituição ou complexidade adicional a justificar.
