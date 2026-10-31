# Implementation Plan: Corrigir carregamento dos campos da estante selecionada

**Branch**: `015-corrigir-campos-estante-selecionada` | **Date**: 2026-10-30 | **Spec**: [spec.md](./spec.md)

## Summary

Ao trocar a estante selecionada, os campos Código e Descrição do formulário "Editar estante" mantêm os valores da estante anterior. Causa confirmada em `LocationsPageClient.tsx`: os inputs são não controlados (`defaultValue`) e o `<form>` é reutilizado pelo React entre estantes, então `defaultValue` não é reaplicado. Correção: forçar a remontagem do formulário por estante com `key={selectedRack.id}` (ver [research.md](./research.md)).

## Technical Context

**Language/Version**: TypeScript estrito, React / Next.js (App Router)

**Primary Dependencies**: TanStack Query (já em uso); nenhuma nova

**Storage**: N/A

**Testing**: Vitest, happy-dom, Testing Library (`tests/web/LocationsPage.test.tsx`)

**Target Platform**: Web

**Project Type**: Aplicação web (monorepo)

**Constraints**: Correção restrita ao cliente; sem mudança de API, dados ou aparência

**Scale/Scope**: 1 componente e 1 arquivo de testes

## Constitution Check

- Testes automatizados: teste de regressão selecionando estantes distintas. ✅
- Estado de servidor via TanStack Query mantido. ✅
- TypeScript estrito, sem novas dependências, UI em português, sem mudança visual (Design System). ✅

Sem violações; reavaliado após o design, sem mudanças.

## Project Structure

```text
specs/015-corrigir-campos-estante-selecionada/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── tasks.md   # /speckit-tasks

src/apps/web/app/locations/LocationsPageClient.tsx
tests/web/LocationsPage.test.tsx
```

**Structure Decision**: alteração localizada nos dois arquivos acima; sem contratos externos novos, por isso não há `contracts/`.

## Complexity Tracking

Sem violações a justificar.
