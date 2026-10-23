# Plano de Implementação: Identidade e perfis

**Branch**: `003-identidade-perfis` | **Data**: 2026-10-23 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificação da funcionalidade em `specs/003-identidade-perfis/spec.md`

## Resumo

Implementar a primeira capacidade de negócio após a fundação técnica: autenticação segura por e-mail e senha com Supabase Auth; gerenciamento de usuários exclusivo do Administrador; e aplicação de permissões tanto na interface web quanto nas operações protegidas da API. O Supabase permanece como autoridade de identidade e credenciais; a persistência PostgreSQL existente guarda o perfil e o estado ativo atuais da aplicação como fonte de autorização. A API valida JWTs do Supabase, consulta a situação e o perfil atuais nas operações protegidas e usa credenciais do provedor disponíveis apenas no servidor para provisionar e administrar identidades. Nunca deve confiar em perfil enviado pelo navegador nem expor credenciais administrativas do provedor à aplicação web.

## Contexto Técnico

**Linguagem/Versão**: C# / .NET 8; TypeScript strict com Next.js 14 App Router; Node.js 20 para validação web.

**Dependências principais**: ASP.NET Core, EF Core 8/Npgsql e FluentValidation existentes; Supabase Auth para identidade por e-mail/senha; `@supabase/supabase-js` e `@supabase/ssr` para Next.js/App Router; `Microsoft.AspNetCore.Authentication.JwtBearer` para validar tokens; TanStack Query v5 para estado do servidor; React Hook Form com `@hookform/resolvers` e Zod para formulários; Axios para chamadas à API da aplicação; xUnit, Vitest e Testing Library.

**Armazenamento**: PostgreSQL existente. Adicionar registro de conta da aplicação vinculado ao identificador da identidade no provedor e marca mínima de revogação da sessão atual para logout imediato; nunca persistir senhas. Identidades e emissão de sessões permanecem sob gestão do Supabase Auth.

**Testes**: Testes unitários e de integração HTTP em xUnit, incluindo autenticação bearer e autorização; Vitest/Testing Library para fluxos de login e administração de usuários; gates existentes de CI para API e web.

**Plataforma alvo**: API .NET 8 e aplicação web Next.js existentes, em desenvolvimento local e no GitHub Actions do repositório.

**Tipo de projeto**: Aplicação web em monorepo modular, com API em camadas.

**Metas de desempenho**: Listagem paginada de usuários pelo Administrador deve responder em até 3 segundos no percentil 95, medida na API após aquecimento e pelo menos 100 requisições, com 1.000 contas em uma única loja e ambiente de teste documentado. A API deve aplicar situação e perfil atuais sem consultar o provedor externo em cada requisição.

**Restrições**: Preservar Supabase Auth, JWT, PostgreSQL, Clean Architecture, monorepo e decisões aprovadas. Validar assinatura, emissor, público e expiração do JWT. Consultar o perfil e a situação da conta na persistência da aplicação, não em metadados de identidade alteráveis pelo usuário. Aplicar mudanças de situação/perfil na próxima requisição protegida, inclusive com token previamente emitido; revogar a sessão corrente no servidor durante o logout. Manter credenciais administrativas do provedor no servidor, fora de logs, bundles de cliente e controle de versão. Provisionar o primeiro Administrador por processo seguro de implantação. Manter consultas de listagem paginadas e leituras sem rastreamento do ORM. Preservar auditoria de alterações (`updated_at`, `updated_by`). Não implementar produtos, fornecedores, localizações nem operações de preço nesta funcionalidade.

**Escala/Escopo**: Uma loja e dois perfis fixos (Administrador, Operador); login/logout, convite e ativação de contas, gerenciamento de usuários pelo Administrador, inativação lógica e autorização na API/web. Sem cadastro público, recuperação de senha, MFA ou funcionalidades do catálogo.

## Verificação da Constituição

**Gate antes da pesquisa: PASS**

- **Camadas e arquitetura (I, IV)**: Manter fluxos de identidade na Application; integrações com provedor e banco na Infrastructure; autenticação HTTP, autorização e endpoints na API; experiência de navegador no web existente. Não adicionar projetos nem inverter dependências.
- **Monorepo (II)**: Manter implementação e testes em `src/`, `tests/` e `.github/workflows/ci.yml`. Não criar diretório de topo.
- **Testes obrigatórios (III)**: Cobrir regras de domínio/aplicação com testes unitários, endpoints protegidos com integração HTTP e comportamento interativo web com Vitest/Testing Library.
- **Tipagem e erros (V)**: Preservar TypeScript strict e tipos C# explícitos; validar entradas com Zod e FluentValidation; retornar erros RFC 7807 seguros e logs estruturados sem credenciais ou tokens, verificando essa ausência nos testes.
- **Frontend e acessibilidade (VI)**: Usar Next.js App Router, interface mobile-first e acessível, integração via `lib/`, TanStack Query para estado do servidor, e React Hook Form com Zod nos formulários.
- **Segurança e dados (VII)**: Usar Supabase Auth como autoridade de identidade; validar JWT na API; aplicar perfis Administrador/Operador no servidor; não persistir senhas nem segredos de provedor; preservar estado de conta e perfil auditáveis.
- **Limites tecnológicos**: Não adicionar plataforma ou camada arquitetural. SDKs novos para integração com o Supabase, se usados, devem ser justificados pela pesquisa e permanecem dentro da plataforma já aprovada.

Não há violações conhecidas da constituição. A credencial privilegiada do provedor deve estar exclusivamente no ambiente de execução da API.

## Estrutura do projeto

### Documentação desta funcionalidade

```text
specs/003-identidade-perfis/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
    └── user-management-api.yaml
```

### Código-fonte e testes

```text
src/
├── apps/web/
│   ├── app/             # Login, ativação, rotas protegidas e administração de usuários
│   ├── components/      # Formulários e componentes acessíveis
│   └── lib/             # Sessão Supabase e cliente da API
└── services/api/
    ├── SistemaEstoque.API/             # JWT bearer, políticas e endpoints
    ├── SistemaEstoque.Application/     # Fluxos e validações
    ├── SistemaEstoque.Domain/          # Perfis e regras independentes de infraestrutura
    └── SistemaEstoque.Infrastructure/  # Persistência e integração Supabase Admin

tests/
├── api/                 # Testes xUnit unitários e de integração
└── web/                 # Vitest e Testing Library
```

**Decisão de estrutura**: Reutilizar estritamente os projetos e diretórios existentes do monorepo. A persistência de contas e a revogação de sessão são incluídas nos projetos existentes; não criar serviço, camada ou diretório de topo.

## Fase 0: Pesquisa

Pesquisar a integração aprovada de Supabase Auth e JWT com .NET, o convite seguro de contas e o provisionamento do primeiro Administrador, e a aplicação imediata de alterações de perfil/situação mesmo quando o cliente ainda possui um JWT válido. Registrar decisões, justificativas, referências oficiais e alternativas em [research.md](research.md), sem alterar o provedor de identidade ou a arquitetura obrigatória.

## Fase 1: Design e contratos

- Definir o modelo persistido de conta e perfis, vínculo com identidade do provedor, unicidade de e-mail, ciclo de vida e invariantes de autorização em [data-model.md](data-model.md).
- Documentar endpoints protegidos de administração de usuários, revogação de sessão e semântica de erro em [contracts/user-management-api.yaml](contracts/user-management-api.yaml). Login é um fluxo do cliente com o provedor; logout revoga primeiro a sessão corrente na API e depois encerra a sessão no provedor. Nenhum dos fluxos expõe endpoint de senha da aplicação.
- Criar cenários executáveis de configuração e validação ponta a ponta para os dois perfis, ativação de conta, alteração imediata de permissões e falhas em [quickstart.md](quickstart.md).

### Superfícies de implementação previstas

- API: projetos existentes `SistemaEstoque.API`, `SistemaEstoque.Application`, `SistemaEstoque.Domain` e `SistemaEstoque.Infrastructure`, com configuração e migrações nas pastas existentes.
- Web: `src/apps/web/app/`, `src/apps/web/components/` e `src/apps/web/lib/`.
- Testes: projetos existentes em `tests/api/` e `tests/web/`.
- CI: jobs existentes de API e web; testes automatizados não dependem de credenciais de produção ou projeto Supabase ativo.

**Gate após design: PASS** — alterações preservam os projetos e tecnologias estabelecidos, usam PostgreSQL para o estado de autorização da aplicação e deixam credenciais de autenticação sob gestão do Supabase Auth. As tarefas especificam o uso das bibliotecas mandatórias da constituição e a validação de logs estruturados sem dados sensíveis. Testes de integração com o provedor devem ser isolados ou utilizar doubles para não exigir segredos de produção no CI.

## Registro de Complexidade

Não há violações da constituição nem complexidade estrutural adicional proposta.
