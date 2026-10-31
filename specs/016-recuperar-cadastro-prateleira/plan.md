# Implementation Plan: Recuperar cadastro de prateleira

**Branch**: `016-recuperar-cadastro-prateleira` | **Date**: 2026-10-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/016-recuperar-cadastro-prateleira/spec.md`

## Summary

Restabelecer na tela de Localizações o cadastro de uma prateleira para a estante ativa selecionada. A interface reutilizará a operação `createShelf` e o endpoint existente, atualizará a lista após o sucesso, e apresentará validações e falhas no contexto do formulário. Não há mudança necessária no domínio, persistência, API pública ou permissões.

## Technical Context

**Language/Version**: TypeScript strict; C# / .NET 8 existente, sem alteração prevista no backend.

**Primary Dependencies**: Next.js 14 App Router, React, TanStack Query v5, Axios, Supabase Auth client, Vitest e Testing Library; todas já existentes.

**Storage**: PostgreSQL/Supabase existente; nenhuma alteração de esquema prevista.

**Testing**: Teste focal em `tests/web/LocationsPage.test.tsx`; suíte web com Vitest e Testing Library. O endpoint e seus testes de integração existentes permanecem cobertos pela suíte de API quando necessário.

**Target Platform**: Aplicação web responsiva existente.

**Project Type**: Monorepo com aplicação Next.js e API ASP.NET Core em camadas.

**Performance Goals**: Cadastro concluído e refletido na interface sem atualização manual pela pessoa, em conformidade com SC-001 e SC-002.

**Constraints**: Reutilizar o endpoint `POST /api/locations/racks/{rackId}/shelves` e `createShelf`; associar o código à estante ativa selecionada; manter validação e unicidade existentes; não permitir iniciar o fluxo sem estante ativa; preservar consulta, edição, situação e navegação de produtos das prateleiras existentes; manter acessibilidade e tokens visuais do projeto.

**Scale/Scope**: Uma prateleira por submissão, na estante ativa selecionada, em uma tela de gestão de localizações. Sem novas dependências, mudanças de banco ou novas operações de servidor.

## Constitution Check

**Gate antes da pesquisa: PASS**

- **Camadas e dependências (I, IV)**: a operação já segue Domain → Application → Infrastructure → API. A feature consome a API existente por `src/apps/web/lib/locations.ts` e não adiciona lógica de domínio à interface.
- **Monorepo (II)**: mudanças limitadas à página existente em `src/apps/web`, aos testes em `tests/web` e aos artefatos da feature em `specs/`.
- **Testes obrigatórios (III)**: cobrir cadastro bem-sucedido e fluxos de erro/validação no teste web da página; regras e endpoint existentes têm cobertura de API.
- **Tipagem e erros (V)**: manter tipos estritos existentes, validação do servidor e mensagens explícitas para falhas, sem sucesso presumido.
- **Frontend e acessibilidade (VI)**: preservar Next.js, TanStack Query, acesso à API via `lib/`, responsividade, rótulos associados e interação acessível por teclado.
- **Segurança e dados (VII)**: reutilizar autenticação e autorização existentes; não contornar validação ou regras de negócio do servidor.
- **Design system (VIII)**: consultar o protótipo existente de estantes e prateleiras e manter tokens e padrões de tela; restaurar apenas os controles solicitados.

Não há violações ou decisões bloqueantes.

## Project Structure

### Documentation (this feature)

```text
specs/016-recuperar-cadastro-prateleira/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── locations-ui.md
└── tasks.md                 # Gerado posteriormente por /speckit-tasks
```

### Source Code (repository root)

```text
src/apps/web/
├── app/locations/
│   └── LocationsPageClient.tsx  # Formulário de cadastro da prateleira
└── lib/
    └── locations.ts            # createShelf existente

tests/web/
└── LocationsPage.test.tsx      # Fluxos da tela e regressões
```

**Structure Decision**: Reutilizar a organização e os arquivos existentes da tela de Localizações. Não criar novos projetos, módulos de domínio ou camadas.

## Phase 0: Research

Consultar [research.md](research.md). A pesquisa confirmou que a API, a validação, a entidade, a regra de normalização e a função cliente para criação de prateleiras já existem. O trabalho se limita a reconectar o formulário da tela a esses recursos e validar o fluxo por testes web.

## Phase 1: Design & Contracts

- Documentar em [data-model.md](data-model.md) as entidades e invariantes existentes, sem propor alteração de persistência.
- Documentar em [contracts/locations-ui.md](contracts/locations-ui.md) os estados, campos, ações, feedback e acessibilidade do cadastro reintroduzido.
- Descrever cenários executáveis e resultados esperados em [quickstart.md](quickstart.md).
- Não criar contrato de API novo ou alterar o OpenAPI: o endpoint de criação de prateleira já está documentado em `specs/007-localizacao-produtos/contracts/catalog-api.yaml`.

## Constitution Check (post-design)

**Gate pós-design: PASS**

- O desenho não altera camadas, dependências, entidades ou persistência.
- A criação continua passando pela API existente, que verifica estante ativa, valida o código e rejeita duplicidade.
- A tela mantém o acesso HTTP via `lib/`, estado remoto com TanStack Query e tratamento visível de erro.
- O contrato de interface orienta controle acessível, feedback de submissão e preservação das funções existentes.
- Os testes cobrem o fluxo restaurado sem retirar os testes de seleção, edição ou gestão existentes.

Não há violações da constituição ou complexidade adicional a justificar.
