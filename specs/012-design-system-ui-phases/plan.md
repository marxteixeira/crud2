# Implementation Plan: Adoção do Design System nas telas

**Branch**: `012-design-system-ui-phases` | **Date**: 2026-10-28 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/012-design-system-ui-phases/spec.md`

## Summary

Alinhar as telas existentes do frontend aos sete protótipos de `docs/05-design-system/` sem alterar regras de negócio. Abordagem: (1) F0 traduz os tokens de `design-systen.md` para a configuração do Tailwind e cria um conjunto pequeno de componentes visuais compartilhados e um layout autenticado; (2) F1–F5 aplicam esses componentes tela a tela, cada uma comparada ao `screen.png`; (3) F6 valida consistência, celular, acessibilidade e regressão. O controle de execução vive em [execution-control.md](./execution-control.md).

## Technical Context

**Language/Version**: TypeScript (strict) em Next.js 14 (App Router), React 18

**Primary Dependencies**: Tailwind CSS 3, TanStack Query v5, React Hook Form + Zod, Supabase SSR (já existentes). Fontes Plus Jakarta Sans e Manrope via `next/font` (já contido no Next.js). Shadcn UI conforme constituição, adotado apenas se necessário (ver research.md R2).

**Storage**: N/A (nenhuma mudança de dados)

**Testing**: Vitest + Testing Library em `tests/web/` (existente); comparação visual manual contra os prints com captura registrada

**Target Platform**: Navegadores modernos; desktop (referência dos protótipos) e celular (Mobile-First)

**Project Type**: Aplicação web (somente `src/apps/web`)

**Performance Goals**: Sem regressão perceptível de carregamento; fontes com `display: swap`

**Constraints**: Sem alterações em `lib/`, API, permissões ou contratos; rótulos em português; acessibilidade (aria-label, teclado)

**Scale/Scope**: 7 telas principais + telas derivadas (edição/detalhe de produto e fornecedor, vínculos, prateleira, ativação, confirmação)

## Constitution Check

| Princípio | Avaliação |
|-----------|-----------|
| I, IV Camadas / testes de dependência | N/A — backend não é tocado. |
| II Monorepo | OK — alterações só em `src/apps/web` e `tests/web`. |
| III Testes obrigatórios | OK — testes existentes devem permanecer verdes; componentes compartilhados com comportamento recebem testes. |
| V Tipagem estrita | OK — componentes tipados, sem `any`. |
| VI Next/Tailwind/Shadcn/A11y | OK — Tailwind com tokens; Server Components por padrão, `"use client"` só onde há interatividade; lógica fica em `lib/`. |
| VII Regras de negócio/segurança | OK — nenhuma regra alterada; botões restritos a Administrador mantêm a condição atual. |
| Idioma | OK — português nos textos. |
| Design System (orientação nova) | Ver nota abaixo. |

**Nota**: o texto de `.specify/memory/constitution.md` (v1.0.0) não contém a orientação sobre o design system mencionada; apenas [copilot-instructions.md](../../.github/copilot-instructions.md) a referencia, com nome de arquivo incorreto (D4). Recomenda-se emendar a constituição (MINOR) antes de `/speckit-implement`. Não bloqueia o plano.

**Resultado**: sem violações. Reavaliado após o design (Fase 1): sem violações.

## Project Structure

### Documentation (this feature)

```text
specs/012-design-system-ui-phases/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── execution-control.md   # controle vivo de fases, telas e divergências
├── contracts/
│   └── ui-contract.md
└── tasks.md               # /speckit-tasks
```

### Source Code (repository root)

```text
src/apps/web/
├── tailwind.config.ts          # F0: tokens (cores, fontes, raios, espaçamentos)
├── app/
│   ├── globals.css             # F0: fontes e base
│   ├── layout.tsx              # F0: carregamento de fontes
│   └── (telas existentes)      # F1–F5: login, activate, users, locations, products, suppliers
└── components/
    ├── ui/                     # F0: Button, Field, DataTable, Dialog, StatusBadge, PageHeader, estados
    ├── AuthenticatedNavigation.tsx  # F0: navegação conforme protótipo
    └── (products|suppliers|users)/  # F2–F5: ajustes visuais
tests/web/                      # testes dos componentes compartilhados e regressão
```

**Structure Decision**: apenas o frontend existente; criar `components/ui/` para os primitivos compartilhados, sem novos diretórios de topo (ADR não necessário).

## Complexity Tracking

Sem violações a justificar.
