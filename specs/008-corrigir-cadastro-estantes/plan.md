# Implementation Plan: Corrigir cadastro de estantes

**Branch**: `008-corrigir-cadastro-estantes` | **Date**: 2026-10-25 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/008-corrigir-cadastro-estantes/spec.md`

## Summary

O `POST /api/locations/racks` retorna 500 porque, com alta probabilidade, as migrations `AddStorageLocations` e `AddProducts` (tabelas `estantes`, `prateleiras`, `produtos`) não foram aplicadas ao banco local. As migrations são, por decisão (README e feature 004), uma operação explícita e não rodam no startup. A correção mantém essa decisão e adiciona: (1) verificação de migrations pendentes na inicialização, com aviso claro, e readiness degradada; (2) comando documentado para aplicar migrations em desenvolvimento; (3) log de requisições com path útil em vez de `405 HTTP Method Not Supported` em preflight CORS; (4) testes de regressão do cadastro de estante e do tratamento de falha de banco. Detalhes em [research.md](./research.md).

## Technical Context

**Language/Version**: C# / .NET 8; TypeScript (Next.js 14)

**Primary Dependencies**: ASP.NET Core, EF Core 8 + Npgsql, FluentValidation, xUnit

**Storage**: PostgreSQL (Supabase); migrations EF em `SistemaEstoque.Infrastructure/Persistence/Migrations`

**Testing**: xUnit em `tests/api/`, Vitest em `tests/web/`

**Target Platform**: API local (`localhost:5013`) e web local

**Project Type**: web-service + web app (monorepo)

**Performance Goals**: verificação de migrations pendentes no startup < 1 s

**Constraints**: sem DDL automático no startup (role de runtime sem DDL); sem stack trace ao cliente

**Scale/Scope**: correção localizada na fase 007; sem novos endpoints

## Constitution Check

| Princípio | Avaliação |
|-----------|-----------|
| I. Clean Architecture | OK. Verificação de migrations na Infrastructure; API só consome. |
| II. Monorepo | OK. Sem novos diretórios de topo. |
| III. Testes obrigatórios | OK. Testes de integração do endpoint e unitários da verificação. |
| IV. Dependência entre camadas | OK. Sem novas referências. |
| V. Erros / Problem Details | OK. Reforça RFC 7807 com traceId e log. |
| VI. Frontend | OK. Apenas exibir traceId em erro 500, se ausente. |
| VII. Segurança e dados | OK. Mantém role sem DDL; migrations explícitas. |

Resultado: sem violações, antes e depois do design.

## Project Structure

### Documentation (this feature)

```text
specs/008-corrigir-cadastro-estantes/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── tasks.md   (/speckit-tasks)
```

Sem `contracts/`: nenhum contrato de API muda (ver [catalog-api.yaml](../007-localizacao-produtos/contracts/catalog-api.yaml)).

### Source Code (repository root)

```text
src/services/api/
├── SistemaEstoque.API/
│   ├── Program.cs                              # aviso de migrations pendentes; readiness
│   └── Middleware/RequestLoggingMiddleware.cs  # logar path quando endpoint é o fallback de 405
└── SistemaEstoque.Infrastructure/Persistence/  # verificador de migrations pendentes + health check "ready"
tests/api/SistemaEstoque.API.Tests/             # cadastro de estante, falha de banco, log de preflight
src/apps/web/                                   # exibir traceId em erro 500
README.md                                       # passo de migrations para ambiente local
specs/007-localizacao-produtos/quickstart.md    # corrigir pré-requisito de migração
```

**Structure Decision**: mudanças dentro da estrutura existente do ADR-0001.

## Complexity Tracking

Sem violações a justificar.
