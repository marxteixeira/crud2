# Implementation Plan: Configuração segura do banco Supabase

**Branch**: `004-supabase-database-config` | **Date**: 2026-10-24 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/004-supabase-database-config/spec.md`

## Summary

Configurar a conexão PostgreSQL da API .NET existente para um banco hospedado no Supabase sem alterar a arquitetura de acesso a dados. Manter credenciais locais em ASP.NET Core User Secrets e credenciais de ambientes hospedados no secret manager da plataforma. Usar uma role de runtime dedicada e de privilégio mínimo, TLS com validação do servidor e certificado, e conexão direta para a API persistente quando a rede permitir; usar Supavisor session somente se um host IPv4-only não alcançar o endpoint direto. Executar migrações fora do tráfego normal com identidade de migração separada. A URL Supabase e a chave publicável do web permanecem configuração do cliente e nunca são credenciais do banco.

A senha de banco compartilhada na solicitação é considerada comprometida e deve ser rotacionada antes do uso. Não copiá-la para código, documentação, comandos, logs ou fixtures de teste.

## Technical Context

**Language/Version**: C# / .NET 8; TypeScript strict / Next.js 14+; Node.js 20.

**Primary Dependencies**: Existing ASP.NET Core, EF Core 8, Npgsql, `@supabase/ssr`, `@supabase/supabase-js`, xUnit, Vitest and Testing Library. No new package is required.

**Storage**: Supabase-hosted PostgreSQL accessed through the existing `ConnectionStrings:Default` setting and EF Core `SistemaEstoqueDbContext`; existing migrations remain explicitly applied.

**Testing**: xUnit API/integration tests with the CI PostgreSQL service; Vitest/Testing Library for web configuration consumers; API build and web lint/build. Tests must not depend on a Supabase production project or its credentials.

**Target Platform**: .NET 8 API and Next.js web app, local development on Windows and hosted API environments; PostgreSQL TLS is required.

**Project Type**: Existing modular monorepo with Next.js client, layered .NET API and test projects.

**Performance Goals**: Preserve existing connection behavior and readiness expectations; no feature-specific throughput target. The API must report its existing readiness result promptly according to deployment health-check policy.

**Constraints**: No secrets in version control or client bundles; preserve Clean Architecture and existing configuration keys for backend persistence; no automatic migrations; no new top-level project or package. Direct connectivity depends on host IPv6 egress or the Supabase IPv4 add-on; on IPv4-only hosts, use session-mode pooling rather than transaction mode for the persistent Npgsql/EF Core API.

**Scale/Scope**: One Supabase PostgreSQL project configured per environment; existing API persistence and current Supabase Auth client settings only. No schema redesign, business-table feature, or new public endpoint.

## Constitution Check

**Gate pré-pesquisa: PASS**

- **Camadas e dependências (I, IV)**: manter EF Core/Npgsql e as opções de persistência em Infrastructure e configuração do host API nos projetos existentes; não adicionar dependências ao Domain ou Application.
- **Monorepo (II)**: nenhuma alteração estrutural, novo projeto ou diretório de topo; seguir ADR-0001.
- **Testes (III)**: estender testes existentes de configuração/conectividade e os consumidores web relevantes; usar o PostgreSQL descartável do CI, nunca o Supabase real.
- **Tipagem, validação e erros (V)**: manter tipos e diagnósticos seguros; nenhum erro, log, health check ou fixture pode ecoar credenciais.
- **Frontend (VI)**: somente atualizar o nome e documentação da configuração pública do cliente se necessário; nunca passar acesso ao PostgreSQL pelo web.
- **Segurança e dados (VII)**: segredos externos ao Git, credencial de execução de privilégio mínimo, autenticação Supabase atual preservada, leituras/saúde sem detalhes secretos.
- **Tecnologias aprovadas**: usa EF Core 8, Npgsql, Supabase e ferramentas de teste já existentes; nenhuma tecnologia nova.

Não há violações nem esclarecimentos bloqueantes. As restrições de rede para escolha do endpoint são tratadas por uma configuração por ambiente, sem fixar infraestrutura que não foi informada.

## Project Structure

### Documentation (this feature)

```text
specs/004-supabase-database-config/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── database-configuration.md
└── tasks.md                 # Criado posteriormente por /speckit-tasks
```

### Source Code (repository root)

```text
src/
├── apps/web/
│   ├── .env.example                          # URL e chave publicável, sem segredo de banco
│   ├── middleware.ts                         # Consumidor da configuração pública
│   └── lib/supabase/{client,server}.ts        # Consumidores da configuração pública
└── services/api/
    ├── SistemaEstoque.API/
    │   ├── appsettings*.json                 # Somente defaults não secretos
    │   └── Program.cs                        # Registro existente de Infrastructure
    └── SistemaEstoque.Infrastructure/
        ├── DependencyInjection.cs             # Validação/uso da connection string atual
        └── Persistence/                       # Contexto, health check e migrações existentes

tests/
├── api/SistemaEstoque.API.Tests/              # Configuração, conectividade, readiness e sanitização
└── web/                                       # Configuração pública cliente/middleware

README.md                                      # Instruções operacionais e configuração local
```

**Structure Decision**: Manter os componentes existentes e documentar a interface de configuração como contrato operacional. Não criar endpoints, tabelas, camadas, projetos ou dependências. Reutilizar o contrato de saúde existente em `specs/001-solution-foundation/contracts/health-api.yaml`, mantendo seus formatos compatíveis.

## Constitution Check (post-design)

**Gate pós-design: PASS**

- O design mantém persistência, EF Core e Npgsql em Infrastructure; configuração continua a ser injetada pelo host existente.
- Não são introduzidos novos projetos, frameworks, serviços de dados concorrentes, endpoints, entidades de negócio ou migrações.
- O PostgreSQL descartável atual continua isolando os testes; testes de produção não recebem nem precisam de credenciais Supabase.
- Segredos ficam fora do repositório; a senha compartilhada não é reproduzida nos artefatos. A configuração web contém apenas URL e chave publicável.
- Migrações permanecem explícitas e usam credencial separada da identidade de runtime.
- O endpoint direto é a primeira opção para serviço persistente em rede compatível; a escolha de session pooler em rede IPv4-only é ambiental e não exige alteração de arquitetura.

Não há violações da constitution nem complexidade adicional a justificar.
