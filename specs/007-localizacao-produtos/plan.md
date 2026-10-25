# Plano de Implementação: Localização e produtos

**Branch**: `007-localizacao-produtos` | **Date**: 2026-10-25 | **Spec**: [spec.md](spec.md)

**Input**: Especificação em `specs/007-localizacao-produtos/spec.md`

## Resumo

Entregar o catálogo de produtos e o cadastro de estantes/prateleiras, com busca, filtros, consulta da localização física e permissões coerentes com Administrador e Operador. Reutilizar a aplicação web Next.js, a API .NET em camadas, EF Core/PostgreSQL, autorização por perfil e auditoria já estabelecidos. Produtos sem preço cadastrados por Operadores permanecem inativos; somente Administradores podem definir preços e ativar esses registros. O modelo de dados, contratos HTTP e guia de validação estão descritos nos artefatos desta fase.

## Contexto técnico

**Linguagem/Versão**: C# / .NET 8; TypeScript strict / Next.js 14 App Router; Node.js 20.

**Dependências principais**: ASP.NET Core, EF Core 8/Npgsql, FluentValidation; Next.js, TanStack Query v5, React Hook Form, Zod e Axios; xUnit e Vitest/Testing Library. Reutilizar as dependências existentes; nenhuma dependência nova prevista.

**Armazenamento**: PostgreSQL/Supabase pelo `SistemaEstoqueDbContext` existente. Persistir produtos, estantes, prateleiras e auditoria em novas tabelas e migração explícita.

**Testes**: Testes unitários de domínio e aplicação, testes HTTP/integrados da API com PostgreSQL descartável e identidade simulada, e testes web com Vitest/Testing Library. Usar os gates de build, testes, lint e build web existentes.

**Plataforma alvo**: Aplicação web responsiva e API existentes, em desenvolvimento local e CI.

**Tipo de projeto**: Monorepo existente com web Next.js e API ASP.NET Core em Clean Architecture.

**Metas de desempenho**: Pelo menos 95% das buscas devem retornar em até 2 segundos para catálogo de até 5.000 produtos. O tempo de localização física deve ficar abaixo de 10 segundos em pelo menos 90% das tentativas de avaliação.

**Restrições**: Preservar autenticação e perfis concluídos; autorização no servidor, não apenas na interface; Operadores não podem definir nem alterar preços; nenhum produto ativo sem preço e localização válida; nomes únicos sem diferenciar caixa ou acentos; quantidade em unidades inteiras; preservar histórico por inativação, sem exclusão definitiva; auditoria de autor e horário; paginação de consultas; migrações explícitas; não incluir fornecedores, importação ou exclusão permanente.

**Escala/Escopo**: Uma loja, catálogo de referência de até 5.000 produtos, perfis fixos Administrador e Operador, cadastro de localização e fluxos de produto. Fornecedores e vínculos ficam para a próxima fase.

## Verificação da Constituição

**Gate antes da pesquisa: PASS**

- **Camadas e dependências (I, IV)**: regras invariantes no Domain; casos de uso e validação na Application; EF Core e migrações na Infrastructure; autorização HTTP e endpoints na API; componentes e acesso a dados da interface nos diretórios web existentes. Sem dependências invertidas ou projetos novos.
- **Monorepo (II)**: reutilizar `src/apps/web`, os projetos sob `src/services/api` e `tests/`; nenhuma nova pasta de topo.
- **Testes obrigatórios (III)**: cobrir regras de domínio/aplicação, endpoints e autorização com xUnit; cobrir busca, formulários e comportamento de perfil com Vitest/Testing Library.
- **Tipagem e erros (V)**: manter C# tipado e TypeScript strict; validar entradas com FluentValidation/Zod; propagar erros com o tratamento RFC 7807 existente, sem falha silenciosa.
- **Frontend e acessibilidade (VI)**: manter App Router, abordagem mobile-first, acessibilidade, estado remoto via TanStack Query, filtros/paginação na URL e acesso HTTP através de `lib/`.
- **Segurança e dados (VII)**: autorizar no servidor a partir da identidade/perfil atuais; preço somente para Administrador; respeitar regras de catálogo, auditoria, paginação, leituras sem rastreamento e segredos externos ao repositório.
- **Limites tecnológicos**: usar PostgreSQL/Supabase e dependências já aprovadas. Não há nova tecnologia ou desvio arquitetural proposto.

Não há violações nem decisões bloqueantes para pesquisa ou design.

## Estrutura do projeto

### Documentação desta funcionalidade

```text
specs/007-localizacao-produtos/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── catalog-api.yaml
└── tasks.md                 # Gerado posteriormente por /speckit-tasks
```

### Código-fonte e testes

```text
src/
├── apps/web/
│   ├── app/                 # Catálogo, detalhe/formulário e gestão de estantes
│   ├── components/          # Componentes acessíveis e responsivos
│   └── lib/                 # Cliente API e funções de acesso ao catálogo
└── services/api/
    ├── SistemaEstoque.Domain/         # Produto, Estante, Prateleira e regras
    ├── SistemaEstoque.Application/    # Casos de uso, DTOs e validação
    ├── SistemaEstoque.Infrastructure/ # EF Core, configurações e migração
    └── SistemaEstoque.API/            # Endpoints protegidos e respostas HTTP

tests/
├── api/                     # Domínio, regras, integração e autorização
└── web/                     # Vitest e Testing Library
```

**Decisão de estrutura**: Manter os projetos e diretórios existentes, seguindo os padrões da fase de identidade. Não criar nova camada, serviço, projeto ou dependência.

## Fase 0: Pesquisa

Consultar [research.md](research.md). As decisões aproveitam o modelo de autenticação/autorização existente, EF Core com PostgreSQL e os critérios já registrados no documento de negócio. A normalização explícita do nome para unicidade e o padrão de preço opcional apenas em produto inativo são decisões desta feature, justificadas e delimitadas nesse artefato.

## Fase 1: Design e contratos

- Definir entidades, chaves, campos de auditoria, invariantes, transições de situação e índices em [data-model.md](data-model.md).
- Descrever endpoints de produto e localização, permissões, paginação e respostas de erro em [contracts/catalog-api.yaml](contracts/catalog-api.yaml).
- Documentar passos reproduzíveis de validação automatizada e ponta a ponta em [quickstart.md](quickstart.md).

## Verificação da Constituição (pós-design)

**Gate pós-design: PASS**

- O desenho mantém as regras do catálogo no Domain/Application, persistência em Infrastructure e interface HTTP na API.
- A autorização do Operador/Admin é aplicada na API e refletida na interface; o envio de preço pelo Operador é recusado sem alteração.
- Dados ativos respeitam preço, localização e regras de unicidade; auditoria e inativação preservam histórico.
- Consultas são paginadas e indexadas; leituras não alteram estado.
- Nenhum projeto, dependência ou diretório de topo adicional foi introduzido.
- Os testes planejados cobrem regras, endpoints, integração persistente, autorização e comportamento web.

Não há violações da constituição ou complexidade adicional a justificar.
