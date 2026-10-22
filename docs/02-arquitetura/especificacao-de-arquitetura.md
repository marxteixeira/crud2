# Especificação de Arquitetura e Requisitos Técnicos
**Sistema de Controle de Produtos e Fornecedores (CRUD)**

---

## 1. Visão Geral da Arquitetura e Estrutura do Repositório

### 1.1 Estrutura de Repositório: Monorepo Modular
Para este projeto, adota-se a abordagem de **Monorepo Modular**.

*   **Justificativa:** Centraliza o código-fonte do Frontend (Next.js) e Backend (.NET 8 Core), compartilhando configurações globais de CI/CD, documentação e contratos/schemas. Permite refatorações transversais eficientes e simplifica o fluxo de desenvolvimento orientado a IA (Prompt-driven Development), mantendo o contexto unificado.

```
/ (Root)
├── .github/workflows/          # Pipelines de CI/CD
├── docs/                       # Documentação e esquemas de arquitetura
├── src/
│   ├── apps/
│   │   └── web/                # Frontend Next.js (App Router)
│   └── services/
│       └── api/                # Backend .NET Core Web API
├── docker-compose.yml          # Ambiente de desenvolvimento local (Supabase local/DB)
└── README.md
```

### 1.2 Diagrama Visual de Arquitetura de Alto Nível
```
+-----------------------------------------------------------------------+
|                              CLIENTES                                 |
|   +---------------------------------+   +-------------------------+   |
|   | Desktop Browser (Admin/Op)      |   | Mobile Browser (Estoque)|   |
|   +---------------------------------+   +-------------------------+   |
+-----------------------------------||----------------------------------+
                                    || HTTPS / JSON
                                    \/
+-----------------------------------------------------------------------+
|                          FRONTEND (NEXT.JS)                           |
|   +---------------------------------------------------------------+   |
|   | App Router (SSR / Client Components)                          |   |
|   | Tailwind CSS / Shadcn UI (Mobile-First)                       |   |
|   | React Query (Cache & State) / Axios Client                    |   |
|   +---------------------------------------------------------------+   |
+-----------------------------------||----------------------------------+
                                    || JWT Bearer Token / REST API
                                    \/
+-----------------------------------------------------------------------+
|                           BACKEND (.NET 8)                            |
|   +---------------------------------------------------------------+   |
|   | API Layer (Controllers, Middleware de Auth/Cors, Rate-Limit)   |   |
|   | Application Layer (Commands/Queries, DTOs, FluentValidation)  |   |
|   | Domain Layer (Entities, Value Objects, Domain Services)       |   |
|   | Infrastructure Layer (EF Core, Supabase Client, Audit Log)   |   |
|   +---------------------------------------------------------------+   |
+-----------------------------------||----------------------------------+
                                    || Npgsql / PostgreSQL Protocol
                                    \/
+-----------------------------------------------------------------------+
|                          PERSISTÊNCIA (SUPABASE)                      |
|   +---------------------------------------------------------------+   |
|   | PostgreSQL (Tables, Constraints, Indexes, Triggers)           |   |
|   | GoTrue (Supabase Auth / JWT Signer)                           |   |
|   | Row Level Security (RLS) & Storage (para backups/imports)     |   |
|   +---------------------------------------------------------------+   |
+-----------------------------------------------------------------------+
```

---

## 2. Stack Tecnológica Frontend: Next.js

### 2.1 Especificação Tecnológica
*   **Framework:** Next.js (versão 14+ com App Router).
*   **Linguagem:** TypeScript (Strict Mode ativado).
*   **Estilização:** Tailwind CSS + Shadcn UI (Componentes acessíveis, responsivos e otimizados para uso mobile no estoque).
*   **Gerenciamento de Estado de Servidor e Cache:** TanStack Query (React Query v5) para consultas, invalidação de cache e mutações otimistas.
*   **Formulários e Validação:** React Hook Form + Zod (para espelhar contratos do DTO e validações no client-side).
*   **Cliente HTTP:** Axios com interceptors configurados para injeção automática de Token Bearer JWT e renovação.

### 2.2 Conceitos Fundamentais Implementados
1.  **App Router & Server/Client Components:**
    *   Uso estratégico de *Server Components (RSC)* para renderização inicial e páginas puramente declarativas (melhorando FCP/LCP e performance em conexões 3G/4G no estoque).
    *   Uso de *Client Components (`"use client"`)* apenas onde interatividade dinâmica é necessária (ex.: formulários complexos, modais e seletores com busca).
2.  **Responsividade e Usabilidade (Regra dos 3 cliques):**
    *   Interface *Mobile-First*, focada em operação em dispositivos móveis (estoquista frente à prateleira).
    *   Tabelas responsivas com fallback para visões de "Cards" em telas menores que 768px.
3.  **Gestão de Estado Distribuído:**
    *   Evita uso de gerenciadores de estado global complexos (como Redux). Usa React Query para sincronização de cache HTTP e estado derivado da URL (QueryParams para buscas e filtros paginados).

---

## 3. Stack Tecnológica Backend: .NET 8

### 3.1 Especificação Tecnológica
*   **Runtime/Framework:** .NET 8 Web API.
*   **Arquitetura:** Arquitetura Limpa (Clean Architecture) / DDD Simplificado (Domain-Driven Design).
*   **ORM:** Entity Framework Core 8 (EF Core) com driver Npgsql para PostgreSQL (Supabase).
*   **Validação de Entrada:** FluentValidation (middleware automático de pipeline).
*   **Documentação da API:** OpenAPI / Swagger UI com suporte a Schemas Bearer JWT.
*   **Mapeamento:** AutoMapper ou Mapster para conversão entre Entidades e DTOs.

### 3.2 Estrutura da Solução em Camadas (.NET Core)
*   `SistemaEstoque.Domain`: Entidades, Enumerações, Interfaces de Repositório, Objetos de Valor e Exceções de Domínio (livre de dependências externas).
*   `SistemaEstoque.Application`: Use Cases/Services, DTOs, Mapeamentos, Validadores (FluentValidation).
*   `SistemaEstoque.Infrastructure`: EF Core `DbContext`, Configurações de Mapeamento Fluent API, Migrações, Clientes de Serviços Externos (Supabase Client).
*   `SistemaEstoque.API`: Controllers, Middlewares (Error Handling, Auth/Cors, Logging), Injeção de Dependências.

### 3.3 Conceitos Fundamentais Implementados
1.  **Domain-Driven Design (DDD) Simplificado:**
    *   Entidades ricas contendo regras de negócio (ex.: validação de preço negativo, regras de inativação com verificação de vínculos).
    *   Uso de *Value Objects* para tipagem forte de CNPJ/CPF e Localização (Estante + Prateleira).
2.  **Global Error Handling Middleware:**
    *   Captura padronizada de exceções traduzidas para o formato **RFC 7807 (Problem Details)**, retornando mensagens amigáveis ao frontend sem expor stack traces.
3.  **Segurança e Performance Backend:**
    *   Aplicações de paginação obrigatória (`PageNumber`, `PageSize`) nas consultas para garantir resposta inferior a 2 segundos (NF-Desempenho).
    *   Consultas read-only otimizadas com `.AsNoTracking()` no EF Core.

---

## 4. Modelagem de Dados Relacional: Supabase (PostgreSQL)

### 4.1 Diagrama Entidade-Relacionamento (ERD - Lógico)

```
+-------------------+       +-----------------------+       +-------------------+
|     ESTANTES      |       |      PRATELEIRAS      |       |     PRODUTOS      |
+-------------------+       +-----------------------+       +-------------------+
| PK id             |<----+ | PK id                 |<----+ | PK id             |
|    codigo (UQ)    |     | | FK estante_id         |     | | FK prateleira_id  |
|    descricao      |     +--|    codigo              |     +--|    nome (UQ)      |
|    ativo          |       |    ativo              |       |    quantidade     |
|    created_at     |       |    created_at         |       |    preco          |
+-------------------+       +-----------------------+       |    ativo          |
                                                            |    created_at     |
                                                            |    updated_at     |
                                                            |    updated_by     |
                                                            +-------------------+
                                                                      |
                                                                      | 1
                                                                      |
                                                            +-------------------+
                                                            | PRODUTO_FORNECEDOR|
                                                            +-------------------+
                                                            | PK,FK produto_id  |
                                                            | PK,FK fornecedor_id|
                                                            +-------------------+
                                                                      |
                                                                      | N
                                                                      |
                                                            +-------------------+
                                                            |    FORNECEDORES   |
                                                            +-------------------+
                                                            | PK id             |
                                                            |    razao_social   |
                                                            |    cnpj_cpf (UQ)  |
                                                            |    telefone       |
                                                            |    email          |
                                                            |    contato_nome   |
                                                            |    observacoes    |
                                                            |    ativo          |
                                                            |    created_at     |
                                                            |    updated_at     |
                                                            |    updated_by     |
                                                            +-------------------+
```



## 5. Autenticação e Autorização

### 5.1 Arquitetura de Segurança
*   **Provedor de Identidade (IdP):** Supabase Auth (GoTrue).
*   **Mecanismo de Token:** JSON Web Tokens (JWT) assinados por chave assimétrica/simétrica via HMAC-SHA256.
*   **Propagação de Contexto:** O cliente Next.js autentica-se com o Supabase Auth, obtém o `access_token` JWT e envia no cabeçalho `Authorization: Bearer <token>` para a Web API .NET.

### 5.2 Validação no .NET (Backend)
O middleware de autenticação JWT do .NET valida a assinatura, emissor (iss), público (aud) e expiração (exp) do token contra o projeto do Supabase.

### 5.3 Mapeamento de Roles e Perfis (RBAC)
No Supabase, as *User Metadata* ou claims personalizadas contêm a role do usuário.

*   **Administrador:** Permissão total (CRUD de Usuários, Edição de Preços, Inativação de Registros, Exclusão de Registros de Teste/Sem Histórico).
*   **Operador:** Permissão limitada (Consulta Geral, Cadastro/Edição de Produtos e Quantidades, sem acesso ao campo de Preço para alteração).

---

## 6. Mapeamento da Lógica de Negócio para Requisitos Técnicos

| ID Regra | Descrição da Regra | Implementação Técnica (Backend / DB) |
| :--- | :--- | :--- |
| **RN-01** | Nome do produto único (ignora maiúsculas/acentos). | Coluna calculada `nome_normalizado` com índice `UNIQUE` e extensão `unaccent` no PostgreSQL. |
| **RN-02** | Quantidade inteira >= 0. | Constraint Check no DB (`CHECK (quantidade >= 0)`) e FluentValidation `GreaterThanOrEqualTo(0)`. |
| **RN-03** | Preço >= 0 com 2 casas decimais. | Tipo `NUMERIC(10,2)` no DB, constraint Check e validação de casas decimais na DTO. |
| **RN-04** | Produto ativo exige estante e prateleira. | Constraint `FOREIGN KEY` não nula `prateleira_id` vinculada à tabela `prateleiras`. |
| **RN-05** | Relacionamento M:N entre Produto e Fornecedor. | Tabela de associação `produtos_fornecedores` com chaves primárias compostas. |
| **RN-06** | CNPJ/CPF válido e único. | Constraint `UNIQUE` no DB e algoritmo de validação de dígitos verificadores de CNPJ/CPF no FluentValidation. |
| **RN-07** | Exclusão Lógica (Soft Delete). | Flag `ativo DEFAULT TRUE`. Consultas EF Core configuradas com Global Query Filters (`.HasQueryFilter(p => p.Ativo)`). |
| **RN-08** | Bloqueio de inativação de Fornecedor com vínculo ativo. | Service valida se `produtos_fornecedores` possui produtos ativos antes de mudar o status `ativo` para `false`. |
| **RN-09** | Alteração de preço apenas por Administrador. | Atributo `[Authorize(Roles = "Administrador")]` na rota/comando de alteração de preço. |
| **RN-10** | Trilha de Auditoria (quem e quando). | Triggers no DB ou override do `SaveChangesAsync` do EF Core preenchendo automaticamente `updated_at` e `updated_by` extraídos do Claim JWT. |

---


## 7. Diretrizes de Desenvolvimento e Prompt Engineering para IA

Para que o desenvolvimento assistido por IA atinja máxima assertividade, os trechos de código gerados devem seguir as normas abaixo:

1.  **Strict Typing:** Todo código TypeScript e C# deve declarar tipos explicitamente. Uso de `any` ou `dynamic` é estritamente proibido.
2.  **Tratamento de Exceções:** Todo endpoint .NET deve ser coberto por logs estruturados e exceções tipadas tratadas no Middleware.
3.  **Acessibilidade e Componentização:** No Next.js, utilizar componentes acessíveis do Shadcn UI com atributos `aria-label` para leitores e navegação facilitada por teclado em dispositivos móveis.
4.  **Isolamento de Responsabilidades:** Nunca colocar lógica de banco de dados nos Controllers da API ou lógica de negócios nos Server/Client Components do Next.js.