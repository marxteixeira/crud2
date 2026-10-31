# Implementation Plan: Corrigir seleção da nova estante após o cadastro

**Branch**: `014-corrigir-selecao-nova-estante` | **Date**: 2026-10-30 | **Spec**: [spec.md](./spec.md)

## Summary

Após cadastrar uma estante, a tela volta para a primeira estante da lista. Causa confirmada em `LocationsPageClient.tsx`: `onSuccess` chama `setSelectedRackId(rack.id)` antes de a lista recarregada conter a nova estante; o `useEffect` de validação da seleção não encontra o id na lista em cache e o substitui por `racks[0]`. Correção: inserir a nova estante no cache da lista antes de selecioná-la (ver [research.md](./research.md)).

## Technical Context

**Language/Version**: TypeScript estrito, React / Next.js (App Router)

**Primary Dependencies**: TanStack Query (já em uso); nenhuma nova

**Storage**: N/A (sem mudança de dados)

**Testing**: Vitest, happy-dom, Testing Library (`tests/web/LocationsPage.test.tsx`)

**Target Platform**: Web

**Project Type**: Aplicação web (monorepo)

**Performance Goals**: N/A

**Constraints**: Correção restrita ao cliente; sem mudança de API ou de aparência

**Scale/Scope**: 1 componente e 1 arquivo de testes

## Constitution Check

- Testes automatizados: teste de regressão que reproduz a corrida (refetch atrasado). ✅
- Estado de servidor via TanStack Query: mantido; apenas atualização do cache. ✅
- TypeScript estrito, sem novas dependências, UI em português. ✅

Sem violações; reavaliado após o design, sem mudanças.

## Project Structure

### Documentation (this feature)

```text
specs/014-corrigir-selecao-nova-estante/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── tasks.md   # /speckit-tasks
```

### Source Code (repository root)

```text
src/apps/web/app/locations/LocationsPageClient.tsx
tests/web/LocationsPage.test.tsx
```

**Structure Decision**: alteração localizada nos dois arquivos acima; sem contratos externos novos, por isso não há `contracts/`.

## Complexity Tracking

Sem violações a justificar.
