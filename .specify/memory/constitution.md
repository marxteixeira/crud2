<!--
Sync Impact Report
- Version change: (template, unversioned) -> 1.0.0
- Modified principles: none (initial adoption; all principles are new)
- Added sections: Core Principles I-VII, Technology Constraints, Development Workflow & Quality Gates, Governance
- Removed sections: none
- Follow-up TODOs: none
-->
# Sistema de Estoque (CRUD) Constitution

## Core Principles

### I. Clean Architecture em Camadas (NON-NEGOTIABLE)
O backend .NET 8 MUST seguir as camadas `SistemaEstoque.Domain`, `Application`, `Infrastructure` e `API`.
Dependências MUST fluir apenas em um sentido: Domain (sem dependências) <- Application <- Infrastructure <- API.
- Domain contém entidades ricas, value objects, enums, interfaces de repositório e exceções de domínio,
  e MUST NOT referenciar pacotes ou projetos externos.
- Application contém casos de uso, DTOs, mapeamentos e validadores FluentValidation.
- Infrastructure contém EF Core (`DbContext`, Fluent API, migrações) e clientes externos (Supabase).
- API contém controllers, middlewares e injeção de dependências, e MUST NOT conter lógica de banco ou de negócio.
Rationale: isolamento de responsabilidades e testabilidade (ADR-0001).

### II. Monorepo Modular
O código MUST seguir a estrutura do ADR-0001: `src/apps/web` (Next.js), `src/services/api` (solução .NET),
`tests/` (testes), `docs/` (documentação) e CI único em `.github/workflows/ci.yml` com jobs `api` e `web`.
Novos diretórios de topo ou mudanças estruturais MUST ser registrados em um novo ADR.

### III. Testes Obrigatórios (NON-NEGOTIABLE)
Toda funcionalidade MUST ser entregue com testes automatizados.
- Backend: xUnit em `tests/api/`. Regras de domínio e casos de uso MUST ter testes unitários;
  endpoints MUST ter testes de integração.
- Frontend: Vitest + Testing Library em `tests/web/`. Componentes e lógica com comportamento MUST ter testes.
- Testes SHOULD ser escritos antes ou junto da implementação; um PR com testes falhando MUST NOT ser integrado.

### IV. Testes de Dependência entre Camadas
Os testes de arquitetura (`LayerDependencyTests`) MUST permanecer verdes e MUST ser estendidos quando
uma camada ou projeto for criado. Violação da direção de dependências do Princípio I MUST falhar o build.

### V. Tipagem Estrita e Tratamento de Erros
- TypeScript MUST operar em strict mode; C# MUST declarar tipos explicitamente.
  `any` e `dynamic` são proibidos.
- Todo endpoint MUST ter logs estruturados; exceções tipadas MUST ser tratadas por middleware global
  que retorna RFC 7807 (Problem Details) sem expor stack traces.
- Entradas MUST ser validadas com FluentValidation (backend) e Zod (frontend).

### VI. Frontend: Next.js, Tailwind e Acessibilidade
O frontend MUST usar Next.js 14+ (App Router), Tailwind CSS e Shadcn UI, com abordagem Mobile-First.
- Server Components são o padrão; `"use client"` MUST ser usado apenas onde houver interatividade.
- Estado de servidor MUST usar TanStack Query; filtros e paginação MUST ficar na URL; Redux é proibido.
- Componentes MUST ser acessíveis (`aria-label`, navegação por teclado).
- Lógica de negócio MUST NOT residir em componentes; o acesso à API MUST passar por `lib/`.

### VII. Regras de Negócio, Segurança e Dados
As regras RN-01 a RN-10 da especificação de arquitetura MUST ser respeitadas: unicidade, valores não
negativos, soft delete (`ativo`), vínculos Produto-Fornecedor, auditoria (`updated_at`, `updated_by`) e
alteração de preço apenas por Administrador.
- Autenticação via Supabase Auth (JWT) validado na API; autorização por roles (RBAC).
- Consultas de listagem MUST ser paginadas e as de leitura MUST usar `.AsNoTracking()`.
- Segredos MUST NOT ser versionados.

### VIII. Design System e Protótipos de Interface
Toda tela e componente visual do frontend MUST seguir o design system documentado em
`docs/05-design-system/design-systen.md` (cores, tipografia, espaçamento e componentes).
Cada pasta em `docs/05-design-system/<tela>/` contém `screen.png` (print) e `code.html` (protótipo) e é a
referência visual da tela correspondente.
- Antes de criar ou alterar uma tela, o protótipo respectivo MUST ser consultado e a implementação comparada ao print.
- Tela sem protótipo próprio MUST seguir o design system e o protótipo mais próximo.
- Elementos do protótipo sem respaldo nos requisitos de negócio MUST NOT ser implementados sem decisão registrada.
- Valores de cor, fonte e espaçamento MUST vir dos tokens do Tailwind; exceções MUST ser registradas na spec da feature.

## Technology Constraints

- Backend: .NET 8, EF Core 8 + Npgsql, FluentValidation, OpenAPI/Swagger, AutoMapper ou Mapster.
- Frontend: TypeScript, Next.js, Tailwind CSS, Shadcn UI, TanStack Query v5, React Hook Form + Zod, Axios.
- Banco: PostgreSQL (Supabase).
- Idioma: documentação, specs, mensagens ao usuário e termos de domínio (ex.: `Produto`, `Fornecedor`, `Ativo`)
  em português; termos técnicos, nomes de padrões e infraestrutura em inglês, mantendo o padrão já
  existente no código. Novos nomes MUST seguir o padrão do código vizinho.
- Novas tecnologias fora desta lista MUST ser justificadas em ADR.

## Development Workflow & Quality Gates

- Fluxo Spec Kit: constitution -> specify -> clarify -> plan -> tasks -> analyze -> implement,
  uma branch por feature em `specs/NNN-nome/`.
- Todo PR MUST passar no CI (build e testes de `api` e `web`) e verificar conformidade com esta constituição.
- Decisões arquiteturais relevantes MUST virar ADR em `docs/04-decisoes-adr/`.
- Requisitos de negócio ficam em `docs/03-requisitos/` e servem de entrada para `/speckit-specify`.
- Complexidade adicional MUST ser justificada no plano da feature.

## Governance

Esta constituição prevalece sobre outras práticas do projeto. Emendas MUST ser feitas via PR com descrição
da mudança, aprovação de ao menos um mantenedor e, se afetarem código existente, plano de migração.
Versionamento semântico: MAJOR para remoção ou redefinição incompatível de princípios, MINOR para novo princípio
ou ampliação material, PATCH para esclarecimentos. Revisões de PR e o passo `/speckit-analyze` MUST verificar
conformidade; conflitos com a constituição são tratados como CRITICAL.
Fontes de referência: `docs/02-arquitetura/especificacao-de-arquitetura.md` e ADRs em `docs/04-decisoes-adr/`.

**Version**: 1.1.0 | **Ratified**: 2026-10-22 | **Last Amended**: 2026-10-28
