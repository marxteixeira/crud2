# Implementation Plan: Base executável da solução

**Branch**: `001-solution-foundation` | **Date**: 2026-10-23 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-solution-foundation/spec.md`

## Summary

Estabelecer uma base de execução consistente para a solução existente sem implementar funcionalidades do estoque. O plano preserva o monorepo e as quatro camadas .NET, completa a configuração por ambiente e o registro de dependências para PostgreSQL/Supabase, separa saúde de prontidão, usa logging JSON integrado e erros RFC 7807, e mantém testes xUnit, Vitest e os jobs existentes de CI como gates. O job `api` disponibiliza PostgreSQL isolado para os testes de integração que precisam do provedor real. A implementação integra-se ao skeleton existente, sem recriar nem deslocar projetos.

## Technical Context

**Language/Version**: C# / .NET 8; TypeScript em modo estrito com Next.js 14+; Node.js 20 para validação web.

**Primary Dependencies**: ASP.NET Core, EF Core 8, Npgsql, FluentValidation, OpenAPI/Swagger; Next.js App Router, Tailwind CSS, Axios, Vitest e Testing Library. Usar as dependências aprovadas e os pacotes já presentes; AutoMapper ou Mapster somente se necessário à composição existente, sem escolha adicional nesta fundação.

**Storage**: PostgreSQL no Supabase, acessado pelo backend via EF Core 8/Npgsql. A fundação não cria tabelas nem dados de negócio; deixa preparado o contexto e o fluxo versionado de migrações aprovados, sem aplicá-las automaticamente durante a inicialização.

**Testing**: xUnit e `Microsoft.AspNetCore.Mvc.Testing` para testes de unidade/integração da API; Vitest e Testing Library para web; testes de arquitetura existentes em `tests/api/SistemaEstoque.Domain.Tests`.

**Target Platform**: Serviço web .NET 8 e aplicação web Next.js, executáveis localmente e no CI Linux existente, com configurações específicas por ambiente.

**Project Type**: Monorepo modular com frontend web e serviço HTTP em camadas.

**Performance Goals**: Inicialização e validação local em até 10 minutos conforme SC-001; manter o limite arquitetural de buscas de até 2 segundos para catálogos do tamanho descrito nos requisitos de negócio quando funcionalidades de busca forem implementadas. Nenhuma carga de negócio ou benchmark é introduzido nesta fundação.

**Constraints**: Preservar ADR-0001, constitution e documentos de arquitetura; não adicionar tecnologia, diretório de topo ou projeto sem ADR; não versionar segredos; configurações de teste isoladas; `GET /health` continua compatível; `GET /health/ready` distingue conectividade PostgreSQL; erros RFC 7807 não expõem detalhes internos; logging estruturado usa o formatter JSON integrado; manter os contratos e testes existentes.

**Scale/Scope**: Base para o monorepo e quatro projetos .NET existentes, um frontend Next.js e projetos de testes já definidos. Sem CRUD, autenticação/gestão de usuários, importação ou esquema de negócio nesta etapa.

## Constitution Check

**Gate pré-pesquisa: PASS**

- **Camadas e dependências (I, IV)**: usar os projetos existentes `Domain`, `Application`, `Infrastructure` e `API`, sem inversão de dependências; preservar e estender `LayerDependencyTests`.
- **Monorepo (II)**: manter `src/apps/web`, `src/services/api`, `tests/`, `docs/` e o workflow único `.github/workflows/ci.yml` com jobs `api` e `web`. Não há mudança estrutural que exija novo ADR.
- **Testes (III)**: cobrir API e web com as ferramentas já aprovadas; CI continua bloqueando build ou teste falho.
- **Tipagem, validação e erros (V)**: manter TypeScript strict e tipos C# explícitos; validar entradas com FluentValidation/Zod quando houver entradas; erros globais em Problem Details RFC 7807 e logs estruturados sem segredos.
- **Frontend (VI)**: manter Next.js App Router, padrões mobile-first e acessíveis; sem introduzir lógica de negócio ou novos fluxos de UI.
- **Segurança e dados (VII)**: PostgreSQL/Supabase, sem segredos versionados; autenticação e regras de negócio ficam para suas respectivas especificações, sem relaxar as decisões aprovadas.
- **Restrições de tecnologia**: nenhuma dependência ou plataforma nova é proposta.

Não há violações conhecidas nem decisão arquitetural pendente para esta fundação. Os detalhes necessários de implementação e validação foram resolvidos em [research.md](research.md), sem alterar tecnologias ou limites de camada definidos pela constitution, arquitetura e ADR-0001.

## Project Structure

### Documentation (this feature)

```text
specs/001-solution-foundation/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── health-api.yaml
└── tasks.md
```

### Source Code (repository root)

```text
src/
├── apps/
│   └── web/
│       ├── app/                    # Next.js App Router
│       ├── components/             # UI, sem lógica de negócio
│       └── lib/                    # Cliente HTTP e integração com a API
└── services/
    └── api/
        ├── SistemaEstoque.API/          # Host, configuração, DI, middleware e health
        ├── SistemaEstoque.Application/  # Casos de uso, validação e contratos de aplicação
        ├── SistemaEstoque.Domain/       # Domínio independente
        ├── SistemaEstoque.Infrastructure/ # EF Core, Npgsql e persistência
        └── SistemaEstoque.sln

tests/
├── api/
│   ├── SistemaEstoque.API.Tests/       # Testes de integração HTTP
│   └── SistemaEstoque.Domain.Tests/    # Testes unitários e de arquitetura
└── web/                                # Vitest e Testing Library
```

**Structure Decision**: Manter a estrutura modular documentada no ADR-0001 e já representada no repositório. A implementação altera apenas os projetos, testes e documentação existentes; não cria novos projetos, diretórios de topo ou camadas.

## Complexity Tracking

Não há violações da constitution ou complexidade adicional a justificar.

**Gate pós-design: PASS** — o design mantém os mesmos projetos, camadas, padrões de persistência, validação, observabilidade e teste aprovados; o PostgreSQL de CI usa service container do próprio GitHub Actions; os artefatos de contrato descrevem interfaces da solução sem impor tecnologia adicional.
