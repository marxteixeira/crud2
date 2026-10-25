# Implementation Plan: Resolver bloqueios do fechamento de Identidade e perfis

**Branch**: `006-resolver-bloqueios-identidade` | **Date**: 2026-10-25 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/006-resolver-bloqueios-identidade/spec.md` e instrução do usuário: "considere os testes sob minha responsabilidade finalizados e corrija os demais T039 e T037".

## Summary

Corrigir a configuração e as fixtures de teste da API para que as 26 falhas desapareçam sem alterar regras de negócio (T037), criar e executar o teste de desempenho com 1.000 contas (T039) e atualizar o relatório de prontidão. Os itens manuais (003 T036, T040 e 004 T018, parte manual) são tratados como concluídos pelo responsável, registrados como "declarado pelo responsável", sem dados pessoais.

## Technical Context

**Language/Version**: C# / .NET 8 (API), TypeScript / Next.js 14 (web)

**Primary Dependencies**: ASP.NET Core, EF Core + Npgsql, xUnit, `WebApplicationFactory`, Vitest

**Storage**: PostgreSQL (descartável nos testes: serviço do CI ou contêiner Docker local)

**Testing**: `dotnet test` (domínio e API), `npm test`/`lint`/`build` na raiz

**Target Platform**: Windows (dev), Linux (CI)

**Project Type**: web-service + web-app (monorepo)

**Performance Goals**: p95 ≤ 3 s com 1.000 contas e ≥ 100 requisições

**Constraints**: sem segredos nos testes; nenhuma regra de negócio alterada; validação real de JWT mantida fora de testes

**Scale/Scope**: ~5 classes de teste ajustadas, 1 classe nova de desempenho, 1 relatório

## Constitution Check

- Testes obrigatórios e suíte verde antes de avançar: atendido (objetivo da feature).
- Sem segredos no repositório: atendido; a string de conexão de teste é uma constante fictícia ou variável de ambiente.
- Clean Architecture preservada: mudanças só em testes e, se necessário, em `Program.cs` (composição).
- Validação de JWT e RBAC preservada: nenhuma relaxação em Production/Development.

Resultado: sem violações. Reavaliado após o desenho: sem violações.

## Project Structure

```text
specs/006-resolver-bloqueios-identidade/  (plan, research, data-model, quickstart, tasks)
specs/005-fechamento-identidade-perfis/readiness-report.md   (atualizado)
tests/api/SistemaEstoque.API.Tests/
├── Infrastructure/AuthenticationTestFactory.cs   (config de teste autossuficiente)
├── RoleAuthorizationTests.cs                     (fábrica JWT sem registro duplicado)
├── PersistenceConnectivityTests.cs               (banco descartável configurável)
└── UserManagementPerformanceTests.cs             (novo, T039)
```

## Design de correções

1. **Grupo A**: as fábricas de teste fornecem `ConnectionStrings:Default` fictícia (ambiente Testing não exige TLS verificado e não conecta ao banco).
2. **Grupo B**: em Development, `Program.cs` já registra Bearer quando `Supabase:Url` existe (por exemplo via User Secrets do desenvolvedor); a fábrica JWT então registrava de novo. A fábrica passa a isolar a configuração (sem User Secrets) e a remover o registro prévio antes de registrar o seu.
3. **Grupo C**: o teste lê `ConnectionStrings__Default` de um banco descartável e é ignorado com mensagem explícita se não houver; o CI mantém o serviço Postgres.
4. **T039**: teste com repositório/serviço em memória com 1.000 contas, ≥ 100 requisições e asserção do p95.
