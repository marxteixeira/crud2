# Plano de Implementação: Fornecedores e vínculos

**Branch**: `010-fornecedores-e-vinculos` | **Data**: 2026-10-26 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificação em `specs/010-fornecedores-e-vinculos/spec.md`

## Resumo

Entregar cadastro e consulta de fornecedores, vínculos muitos-para-muitos com produtos, filtro do catálogo por fornecedor e regras de identificação, autorização e auditoria. Reutilizar o catálogo e a identidade existentes na aplicação web Next.js e API .NET em camadas, persistindo fornecedores e vínculos no PostgreSQL existente por migração explícita. CPF/CNPJ será validado e armazenado em forma canônica, evitando duplicidade por pontuação. Inativação lógica mantém o histórico; fornecedores inativos não recebem novos vínculos e fornecedores com produtos ativos associados não podem ser inativados.

## Contexto técnico

**Linguagem/Versão**: C# / .NET 8; TypeScript strict / Next.js 14 App Router; Node.js 20.

**Dependências principais**: ASP.NET Core, EF Core 8/Npgsql, FluentValidation, PostgreSQL/Supabase; Next.js, TanStack Query v5, React Hook Form, Zod e Axios; xUnit e Vitest/Testing Library. Reutilizar dependências existentes; nenhuma dependência nova prevista.

**Armazenamento**: PostgreSQL/Supabase pelo `SistemaEstoqueDbContext` existente. Adicionar tabelas de fornecedores e vínculos, índices, restrições e migração explícita.

**Testes**: Testes de domínio e aplicação, integração de API com PostgreSQL de teste e identidade simulada, testes Vitest/Testing Library de formulários, consultas, filtros e permissões. Executar os gates existentes da API e web.

**Plataforma alvo**: Aplicação web responsiva e API existentes, em desenvolvimento local e CI.

**Tipo de projeto**: Monorepo existente com web Next.js e API ASP.NET Core em Clean Architecture.

**Metas de desempenho**: Para catálogo de até 5.000 produtos, pelo menos 95% das consultas por fornecedor retornam em até 2 segundos. Cadastro ou consulta de fornecedor e produtos deve poder ser concluída em até 30 segundos conforme SC-005.

**Restrições**: Preservar autenticação, perfis, catálogo e arquitetura existentes; autorização na API; validação de CPF/CNPJ; unicidade de identificadores; vínculos muitos-para-muitos; permitir produto sem fornecedor; impedir novos vínculos a fornecedor inativo; impedir inativação de fornecedor com produtos ativos vinculados; manter trilha de alterações; consultas paginadas e leituras sem rastreamento; migração explícita; sem integração externa ou importação de planilha.

**Escala/Escopo**: Uma loja, perfis Administrador e Operador, fornecedores e vínculos do catálogo existente, com até 5.000 produtos como catálogo de referência. Importação, integração automatizada com fornecedores e exclusão definitiva permanecem fora desta fase.

## Verificação da Constituição

**Gate antes da pesquisa: PASS**

- **Camadas e arquitetura (I, IV)**: Regras e entidades em Domain; comandos, DTOs e validadores em Application; persistência, configurações e migração em Infrastructure; autorização e HTTP em API; integração web via `lib/`. Nenhuma dependência invertida ou projeto novo.
- **Monorepo (II)**: Reutilizar `src/apps/web`, projetos sob `src/services/api` e testes em `tests/`; sem novos diretórios de topo.
- **Testes obrigatórios (III)**: Testar regras de documentos e vínculos no domínio/aplicação, endpoints e autorização via xUnit e comportamento interativo web via Vitest/Testing Library.
- **Tipagem e erros (V)**: C# explícito e TypeScript strict; FluentValidation/Zod; erros RFC 7807 e logs estruturados, sem falha silenciosa.
- **Frontend e acessibilidade (VI)**: Next.js App Router, mobile-first, controles acessíveis, estado remoto com TanStack Query, filtros/paginação na URL e chamadas de API em `lib/`.
- **Segurança e dados (VII)**: Reutilizar sessão JWT e perfis atuais; validar autorização no servidor; preservar auditoria, inativação lógica, paginação e `.AsNoTracking()` para consultas.
- **Limites tecnológicos**: Usar tecnologias e dependências aprovadas. A validação determinística de CPF/CNPJ não requer dependência adicional.

Não há violações conhecidas nem bloqueios para pesquisa e design.

## Estrutura do projeto

### Documentação desta funcionalidade

```text
specs/010-fornecedores-e-vinculos/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── supplier-api.yaml
└── tasks.md                 # Gerado posteriormente por /speckit-tasks
```

### Código-fonte e testes

```text
src/
├── apps/web/
│   ├── app/suppliers/       # Consulta, detalhe e manutenção de fornecedores
│   ├── app/products/        # Exibição de vínculos e filtro do catálogo
│   └── lib/                 # Funções tipadas de acesso à API
└── services/api/
    ├── SistemaEstoque.Domain/          # Fornecedor e vínculo de fornecimento
    ├── SistemaEstoque.Application/     # Casos de uso, contratos e validação
    ├── SistemaEstoque.Infrastructure/  # EF Core, configurações e migração
    └── SistemaEstoque.API/             # Endpoints protegidos

tests/
├── api/                     # Domínio, casos de uso, integração e autorização
└── web/                     # Vitest e Testing Library
```

**Decisão de estrutura**: Manter projetos e camadas existentes. A API de vínculo será centrada na relação produto-fornecedor, com leitura por ambos os lados e operações de associação/desassociação reutilizáveis pelas duas telas; nenhuma nova camada, serviço ou dependência.

## Fase 0: Pesquisa

Consultar [research.md](research.md). A pesquisa confirmou padrões de auditoria, persistência, paginação e autorização nas fases anteriores. A validação do identificador será determinística e sem dependência externa; a trilha do vínculo será preservada com inativação lógica da associação.

## Fase 1: Design e contratos

- Definir campos, normalização de CPF/CNPJ, validações, estados, relações, auditoria, regras de exclusão e índices em [data-model.md](data-model.md).
- Especificar operações paginadas de fornecedor, visões de produtos por fornecedor e fornecedores por produto, associação/desassociação, filtro do catálogo e respostas de erro em [contracts/supplier-api.yaml](contracts/supplier-api.yaml).
- Descrever preparação do ambiente e cenários repetíveis de validação de cadastro, vínculo, filtro, autorização e inativação em [quickstart.md](quickstart.md).

## Verificação da Constituição (pós-design)

**Gate pós-design: PASS**

- Entidades e regras permanecem no Domain/Application; persistência e migração em Infrastructure; endpoints e políticas na API.
- Documentos de fornecedor são validados no servidor; a unicidade é garantida também pela persistência.
- Vínculos mantêm auditoria e podem ser desativados sem apagar seu histórico; leituras refletem somente associações ativas, salvo consulta histórica.
- A inativação de fornecedor verifica produtos ativos associados; a verificação e a mudança de situação devem ser atômicas.
- Listagens paginadas, autorização por perfil, erros Problem Details e testes automatizados estão previstos.
- Nenhum novo projeto, diretório de topo, tecnologia ou desvio arquitetural foi introduzido.

Não há violações da constituição ou complexidade adicional a justificar.
