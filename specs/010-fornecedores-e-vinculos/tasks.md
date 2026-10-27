# Tarefas: Fornecedores e vínculos

**Entrada**: Documentos de design em `specs/010-fornecedores-e-vinculos/`

**Pré-requisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/supplier-api.yaml` e `quickstart.md`

**Testes**: Obrigatórios pela Constituição (Princípio III) e previstos no plano: xUnit para domínio, casos de uso e endpoints; Vitest/Testing Library para a web. Escrever antes ou junto da implementação e validar cada história independentemente.

**Organização**: Tarefas agrupadas por história, seguindo prioridades e dependências da especificação.

## Formato: `[ID] [P?] [Story] Descrição`

- **[P]**: pode ser executada em paralelo, pois trabalha em arquivos distintos e não depende de tarefa incompleta.
- **[Story]**: história relacionada, obrigatória nas fases de histórias.
- Toda tarefa aponta para os caminhos exatos dos arquivos a criar ou alterar.

## Convenções de caminho

Manter a estrutura do monorepo existente: `src/services/api/SistemaEstoque.*`, `src/apps/web/`, `tests/api/` e `tests/web/`. Não criar projeto, camada ou diretório de topo.

## Fase 1: Setup

**Objetivo**: Preparar projetos e dependências.

Os projetos .NET, web e testes já existem; o plano não prevê novos pacotes ou configuração compartilhada. Não há tarefa de setup pendente.

---

## Fase 2: Fundacional

**Objetivo**: Preparar pré-requisitos comuns que bloqueiem todas as histórias.

Autenticação, autorização, PostgreSQL, migrações, validação, logs, auditoria, tratamento Problem Details e padrões de teste já estão disponíveis. Não há mudança transversal bloqueante a executar antes das histórias; o modelo de fornecedor pertence à US1 e o vínculo pertence à US2.

**Checkpoint**: Iniciar US1 reutilizando a fundação existente. As fases anteriores de identidade e catálogo devem estar aplicadas no ambiente de desenvolvimento.

---

## Fase 3: User Story 1 — Manter fornecedores (Priority: P1) 🎯 MVP

**Objetivo**: Administradores cadastram, consultam, pesquisam, editam e inativam fornecedores, com CPF/CNPJ válido e único, auditoria e bloqueio de inativação enquanto existirem produtos ativos vinculados.

**Teste independente**: Como Administrador, cadastrar fornecedor com CPF ou CNPJ válido, encontrá-lo pela busca, editá-lo e inativá-lo quando não tiver produto ativo associado; confirmar rejeição de documento inválido/duplicado e preservação do cadastro histórico.

### Testes da User Story 1

- [X] T001 [P] [US1] Criar testes de domínio de `CpfCnpj` para CPF/CNPJ válido e inválido, entradas pontuadas equivalentes, comprimento de 11/14 dígitos e sequências repetidas em `tests/api/SistemaEstoque.Domain.Tests/CpfCnpjTests.cs`
- [X] T002 [P] [US1] Criar testes de `Fornecedor` para nome obrigatório, dados opcionais, estados Active/Inactive e atualização de auditoria em `tests/api/SistemaEstoque.Domain.Tests/FornecedorTests.cs`
- [X] T003 [P] [US1] Criar testes de integração da API para cadastro, consulta, pesquisa, edição e status de fornecedor, incluindo CPF/CNPJ inválido/duplicado, permissões Admin/Operador, conflito por produto ativo, auditoria e Problem Details em `tests/api/SistemaEstoque.API.Tests/SupplierEndpointsTests.cs`
- [X] T004 [P] [US1] Criar testes web de fluxo básico de cadastro e consulta, campos obrigatórios, campos opcionais e exibição dos dados retornados em `tests/web/SuppliersPage.test.tsx`

### Implementação da User Story 1

- [X] T005 [P] [US1] Implementar o value object `CpfCnpj` em `src/services/api/SistemaEstoque.Domain/ValueObjects/CpfCnpj.cs`, removendo pontuação/espaços, aceitando somente 11 dígitos para CPF ou 14 para CNPJ, verificando os dígitos verificadores e expondo uma representação canônica única
- [X] T006 [US1] Implementar a entidade `Fornecedor` em `src/services/api/SistemaEstoque.Domain/Entities/Fornecedor.cs` com `id` UUID; `name` obrigatório, não vazio após trim e máximo de 200 caracteres; `tax_identifier` obrigatório e canônico, CPF com 11 ou CNPJ com 14 dígitos, válido e único; `phone` opcional com máximo de 30 caracteres; `email` opcional válido com máximo de 254 caracteres; `contact_name` opcional com máximo de 200 caracteres; `notes` opcional com máximo de 2000 caracteres; `status` Active/Inactive; `created_at`/`updated_at` UTC; `created_by`/`updated_by` UUID; incluir transições auditadas de edição e situação
- [X] T007 [US1] Mapear `Fornecedor` no `SistemaEstoqueDbContext` e criar `FornecedorConfiguration` em `src/services/api/SistemaEstoque.Infrastructure/Persistence/SistemaEstoqueDbContext.cs` e `src/services/api/SistemaEstoque.Infrastructure/Persistence/Configurations/FornecedorConfiguration.cs`, incluindo índice único do identificador canônico, restrições dos campos e auditoria
- [X] T008 [US1] Criar e verificar a migração EF Core `AddSuppliers` para a tabela de fornecedores, índice único de CPF/CNPJ canônico, campos opcionais e enum de situação no arquivo gerado `src/services/api/SistemaEstoque.Infrastructure/Persistence/Migrations/*_AddSuppliers.cs` e no snapshot `src/services/api/SistemaEstoque.Infrastructure/Persistence/Migrations/SistemaEstoqueDbContextModelSnapshot.cs`; não aplicar migrações no startup nem inserir dados pessoais
- [X] T009 [US1] Definir requests, DTOs paginados, exceções de conflito e interface de gerenciamento de fornecedores em `src/services/api/SistemaEstoque.Application/Suppliers/SupplierContracts.cs`, seguindo `Supplier`, `PaginatedSuppliers` e `SupplierInput` de `specs/010-fornecedores-e-vinculos/contracts/supplier-api.yaml`
- [X] T010 [US1] Implementar validadores FluentValidation de criação, atualização parcial, identificador e consulta em `src/services/api/SistemaEstoque.Application/Suppliers/SupplierValidators.cs`, aplicando nome obrigatório/máximo 200, telefone máximo 30, e-mail válido/máximo 254, contato máximo 200, observações máximo 2000 e CPF/CNPJ válido
- [X] T011 [US1] Implementar casos de uso de criação, pesquisa paginada, detalhe, edição e alteração de situação em `src/services/api/SistemaEstoque.Infrastructure/Persistence/SupplierManagementService.cs`, usando `.AsNoTracking()` nas leituras, unicidade persistente e auditoria por conta autenticada
- [X] T012 [US1] Implementar `SuppliersController` em `src/services/api/SistemaEstoque.API/Controllers/SuppliersController.cs` com `GET/POST /api/suppliers`, `GET/PATCH /api/suppliers/{supplierId}` e `PUT /api/suppliers/{supplierId}/status`; exigir conta ativa, restringir mutações a Administrador e mapear validações, ausência e conflitos para respostas Problem Details
- [X] T013 [US1] Registrar serviços, validadores e configurações de fornecedor na injeção de dependências em `src/services/api/SistemaEstoque.Infrastructure/DependencyInjection.cs` e `src/services/api/SistemaEstoque.API/Program.cs`
- [X] T014 [US1] Implementar tipos e funções HTTP de fornecedor (listagem paginada, pesquisa, detalhe, criação, edição e situação) em `src/apps/web/lib/suppliers.ts`, usando o cliente autenticado existente em `src/apps/web/lib/api.ts`
- [X] T015 [US1] Criar páginas acessíveis e mobile-first de listagem, pesquisa, cadastro, edição, detalhe e ativação/inativação de fornecedor em `src/apps/web/app/suppliers/page.tsx`, `src/apps/web/app/suppliers/SuppliersPageClient.tsx`, `src/apps/web/app/suppliers/new/page.tsx` e `src/apps/web/app/suppliers/[supplierId]/page.tsx`; validar campos com Zod e manter ações de escrita somente para Administrador
- [ ] T016 [US1] Completar os testes de integração para concorrência de CPF/CNPJ duplicado, limites dos campos, paginação, atualização de auditoria e bloqueio transacional da inativação com produto ativo em `tests/api/SistemaEstoque.API.Tests/SupplierEndpointsTests.cs`
- [X] T017 [US1] Completar os testes web de permissões de Administrador/Operador, erros de validação e conflito, feedback de sucesso/erro e estado vazio em `tests/web/SuppliersPage.test.tsx`

**Checkpoint**: Administradores mantêm fornecedores com documento válido e único; fornecedores sem produtos ativos podem ser inativados sem apagar histórico.

---

## Fase 4: User Story 2 — Consultar e manter vínculos de fornecimento (Priority: P1)

**Objetivo**: Associar e desassociar produtos e fornecedores, consultar a relação a partir dos dois lados e preservar histórico e auditoria.

**Teste independente**: Com produto e fornecedor ativos, associá-los e confirmar o vínculo nas duas consultas; desassociar e confirmar que sai das consultas operacionais sem remover os registros. Verificar relações múltiplas, consulta histórica, autorização e inativação de fornecedor.

**Dependência**: US1 fornece os fornecedores persistidos e seus estados; a fase anterior fornece produtos e permissões de edição do produto.

### Testes da User Story 2

- [X] T018 [P] [US2] Criar testes de domínio de `VinculoProdutoFornecedor` para criação, auditoria, desassociação lógica, reativação e transições de estado em `tests/api/SistemaEstoque.Domain.Tests/VinculoProdutoFornecedorTests.cs`
- [X] T019 [P] [US2] Criar testes de integração dos endpoints de associação e consulta pelos dois lados, incluindo relações múltiplas, novo vínculo com produto/fornecedor inativo, desassociação idempotente/ausente, consulta histórica e autorização de edição de produto em `tests/api/SistemaEstoque.API.Tests/SupplierEndpointsTests.cs`
- [X] T020 [P] [US2] Criar testes web de gerenciamento e consulta de fornecedores por produto e produtos por fornecedor, incluindo vínculos múltiplos, histórico, estados vazios e permissões por perfil em `tests/web/ProductSupplierLinks.test.tsx`

### Implementação da User Story 2

- [X] T021 [US2] Criar a entidade `VinculoProdutoFornecedor` em `src/services/api/SistemaEstoque.Domain/Entities/VinculoProdutoFornecedor.cs` com `id`, `product_id`, `supplier_id`, estado Active/Inactive, auditoria UTC e UUID; impedir duplicação de vínculo ativo por par, preservar `created_at`/`created_by` ao reativar e atualizar `updated_at`/`updated_by` em associação ou desassociação
- [X] T022 [US2] Mapear `VinculoProdutoFornecedor` no `SistemaEstoqueDbContext` e em `src/services/api/SistemaEstoque.Infrastructure/Persistence/Configurations/VinculoProdutoFornecedorConfiguration.cs`, com chaves estrangeiras restritivas, índice único condicional por `(product_id, supplier_id)` para vínculos ativos e índices de consulta por produto, fornecedor e situação
- [X] T023 [US2] Criar a migração EF Core `AddProductSupplierLinks` com chaves estrangeiras, índices, auditoria e restrição de unicidade condicional no arquivo gerado `src/services/api/SistemaEstoque.Infrastructure/Persistence/Migrations/*_AddProductSupplierLinks.cs` e no snapshot `src/services/api/SistemaEstoque.Infrastructure/Persistence/Migrations/SistemaEstoqueDbContextModelSnapshot.cs`; manter tabelas e relações sem exclusão física
- [X] T024 [US2] Definir requests, DTOs paginados, exceções e interface de casos de uso para associação, desassociação e listagem de vínculos em `src/services/api/SistemaEstoque.Application/Suppliers/ProductSupplierContracts.cs`, incluindo os formatos `SupplierLink`, `ProductSupplierLink` e `PaginatedSupplierLinks` do contrato
- [X] T025 [US2] Implementar a consulta de produtos ativos associados e consulta histórica paginada em `src/services/api/SistemaEstoque.Infrastructure/Persistence/SupplierManagementService.cs`, filtrando status do produto e link conforme contrato e usando `.AsNoTracking()`
- [X] T026 [US2] Implementar associação/desassociação e consulta paginada de fornecedores do produto em `src/services/api/SistemaEstoque.Infrastructure/Persistence/ProductSupplierService.cs`; exigir cadastros ativos para associação, reativar vínculo histórico correspondente, persistir autoria e tratar concorrência sem criar pares ativos duplicados
- [X] T027 [US2] Bloquear atomicamente a inativação de fornecedor que tenha vínculo ativo com produto ativo em `src/services/api/SistemaEstoque.Infrastructure/Persistence/SupplierManagementService.cs`, permitindo inativar após desassociação ou inativação do produto e impedindo corrida com nova associação
- [X] T028 [US2] Implementar `GET /api/suppliers/{supplierId}/products` em `src/services/api/SistemaEstoque.API/Controllers/SuppliersController.cs` com paginação, filtros de situação de produto e vínculo e resposta conforme `PaginatedProductSupplierLinks`
- [X] T029 [US2] Implementar `GET /api/products/{productId}/suppliers` e `PUT/DELETE /api/products/{productId}/suppliers/{supplierId}` em `src/services/api/SistemaEstoque.API/Controllers/ProductsController.cs`, aplicando autorização de edição de produto e respostas 401/403/404/409 conforme `contracts/supplier-api.yaml`
- [X] T030 [US2] Registrar casos de uso, entidades e validadores de vínculo na injeção de dependências em `src/services/api/SistemaEstoque.Infrastructure/DependencyInjection.cs`
- [X] T031 [US2] Implementar funções tipadas de leitura de vínculos do produto e consulta paginada de produtos por fornecedor em `src/apps/web/lib/suppliers.ts` e `src/apps/web/lib/products.ts`, mantendo os tipos alinhados a `SupplierLink`, `ProductSupplierLink` e `PaginatedProductSupplierLinks`
- [X] T032 [US2] Exibir, associar e desassociar fornecedores no detalhe de produto em `src/apps/web/app/products/[productId]/page.tsx` e `src/apps/web/components/products/ProductSuppliers.tsx`, usando seleção apenas de fornecedores ativos e controles conforme a permissão de edição do produto
- [X] T033 [US2] Exibir produtos associados, histórico e controles de vínculo na página do fornecedor em `src/apps/web/app/suppliers/[supplierId]/page.tsx` e `src/apps/web/components/suppliers/SupplierProducts.tsx`, permitindo consulta aos dois perfis e mutações somente a quem tem permissão de edição do produto
- [ ] T034 [US2] Completar os testes da API para auditoria do vínculo, múltiplos fornecedores/produtos, histórico de desassociação, vínculo preservado em produto inativo, conflito de inativação e concorrência em `tests/api/SistemaEstoque.API.Tests/ProductSupplierEndpointsTests.cs` e `tests/api/SistemaEstoque.API.Tests/SupplierEndpointsTests.cs`
- [ ] T035 [US2] Completar os testes web de inclusão/remoção, consistência das consultas nos dois lados, histórico e recusa de controles não autorizados em `tests/web/ProductSupplierLinks.test.tsx`

**Checkpoint**: Vínculos muitos-para-muitos aparecem consistentemente nos dois cadastros, são auditados, preservam histórico e obedecem às permissões e estados.

---

## Fase 5: User Story 3 — Encontrar produtos por fornecedor (Priority: P2)

**Objetivo**: Filtrar o catálogo de produtos por fornecedor e combinar o filtro com busca, localização, situação e paginação existentes.

**Teste independente**: Com produtos de fornecedores diferentes, filtrar por um fornecedor, combinar com os demais filtros e verificar que todos os resultados atendem a cada critério e respeitam a paginação; conferir mensagem de resultado vazio.

**Dependência**: US2 precisa fornecer vínculos persistidos para que o filtro identifique os produtos relacionados.

### Testes da User Story 3

- [X] T036 [P] [US3] Criar testes de integração da combinação `supplierId`, busca, `rackId`, `shelfId`, situação e paginação, incluindo fornecedor inexistente/sem vínculos e ausência de resultados em `tests/api/SistemaEstoque.API.Tests/ProductSearchBySupplierTests.cs`
- [X] T037 [P] [US3] Criar testes web para filtro de fornecedor, combinação de filtros, atualização dos parâmetros da URL, paginação e estado sem resultados em `tests/web/ProductList.test.tsx`

### Implementação da User Story 3

- [X] T038 [US3] Estender `ProductFilters` e `listProducts` em `src/apps/web/lib/products.ts` com filtro opcional `supplierId`, serializando-o junto aos filtros e paginação já existentes
- [X] T039 [US3] Estender `IProductManagementService.ListAsync` em `src/services/api/SistemaEstoque.Application/Products/ProductContracts.cs` e `ProductManagementService.ListAsync` em `src/services/api/SistemaEstoque.Infrastructure/Persistence/ProductManagementService.cs` para filtrar por fornecedor via vínculo ativo e compor o critério com busca, estante, prateleira e situação sem duplicar produtos
- [X] T040 [US3] Atualizar `GET /api/products` em `src/services/api/SistemaEstoque.API/Controllers/ProductsController.cs` para validar e propagar `supplierId`, manter paginação entre 1 e 100 e preservar a resposta paginada existente
- [X] T041 [US3] Adicionar seleção de fornecedor e composição de filtros na URL da lista de produtos em `src/apps/web/app/products/page.tsx` e `src/apps/web/app/products/ProductsPageClient.tsx`, incluindo estado vazio claro e acessível
- [X] T042 [US3] Completar os testes da API para filtros combinados, paginação e ausência de duplicidade quando a relação consultada tiver mais de um vínculo relevante em `tests/api/SistemaEstoque.API.Tests/ProductSearchBySupplierTests.cs`
- [X] T043 [US3] Completar os testes web para filtro combinado, navegação/atualização da URL, paginação e estado vazio em `tests/web/ProductList.test.tsx`

**Checkpoint**: Administradores e Operadores localizam produtos por fornecedor sem perder filtros, paginação ou resultados vazios claros.

---

## Fase 6: Polish e preocupações transversais

**Objetivo**: Confirmar consistência entre especificação, contrato, implementação, acessibilidade e cenários ponta a ponta.

- [X] T044 [P] Atualizar navegação autenticada e links entre catálogo, detalhe de produto e fornecedores em `src/apps/web/components/AuthenticatedNavigation.tsx` e `src/apps/web/app/products/ProductsPageClient.tsx`
- [X] T045 [P] Validar que `specs/010-fornecedores-e-vinculos/contracts/supplier-api.yaml` corresponde aos endpoints, esquemas, permissões, paginação e respostas implementados, sem referências locais inválidas
- [ ] T046 Executar todos os cenários do guia e atualizar os pré-requisitos/resultados efetivamente verificados em `specs/010-fornecedores-e-vinculos/quickstart.md`
- [ ] T047 Executar testes xUnit e Vitest, lint/typecheck/build web e build da solução .NET; corrigir regressões relacionadas à feature e registrar os comandos/resultados em `specs/010-fornecedores-e-vinculos/quickstart.md`
- [ ] T048 Confirmar manualmente a validação de tarefas de cadastro, vínculo e filtro pelos dois perfis, a consistência de auditoria e o limite de resposta definido em SC-005 em `specs/010-fornecedores-e-vinculos/quickstart.md`

## Dependências e ordem de execução

### Dependências entre fases

- **Setup (Fase 1)**: já satisfeito pelos projetos e dependências existentes.
- **Fundacional (Fase 2)**: não possui tarefa bloqueante nova; depende das fases anteriores já concluídas no ambiente (identidade/perfis e localização/produtos).
- **US1 (Fase 3)**: independente dentro desta feature; entrega o cadastro e ciclo de vida de fornecedores.
- **US2 (Fase 4)**: depende da US1 para fornecedores e dos produtos da fase 007; implementa a relação e as consultas nos dois lados.
- **US3 (Fase 5)**: depende da US2 para vínculos ativos; estende o filtro do catálogo existente.
- **Polish (Fase 6)**: depende das histórias incluídas na entrega.

### Grafo das histórias

```text
Pré-requisitos existentes: identidade/perfis + catálogo de produtos/localizações
  └── US1: cadastro e manutenção de fornecedores
       └── US2: associações, consultas bilaterais e histórico
            └── US3: filtro do catálogo por fornecedor
                 └── Polish: contrato, gates e quickstart
```

### Oportunidades de paralelismo

- **US1**: T001, T002, T003 e T004 criam testes em arquivos distintos; T005 é independente de testes após definição da regra de documento; a implementação web T014 pode começar após estabilização do contrato, em paralelo à API; T015 depende de T014.
- **US2**: T018, T019 e T020 são testes em arquivos distintos; T021/T022 podem ser desenvolvidos em arquivos diferentes após estabilização do modelo; T031 pode avançar em paralelo aos endpoints após definição dos DTOs; T032 e T033 usam telas/componentes distintos, mas devem integrar as funções de T031.
- **US3**: T036 e T037 são independentes; após definição do parâmetro `supplierId`, T038 e o filtro de consulta no serviço em T039 podem avançar em arquivos distintos; T041 depende de T038.
- **Entre histórias**: US2 depende de US1 para fornecedores persistidos; US3 depende de US2 para relações consultáveis. Não executar essas histórias em paralelo antes dos respectivos checkpoints.

## Exemplos de execução paralela

```text
US1, início da fase de testes:
T001 — testes de CPF/CNPJ em tests/api/SistemaEstoque.Domain.Tests/CpfCnpjTests.cs
T002 — testes da entidade em tests/api/SistemaEstoque.Domain.Tests/FornecedorTests.cs
T003 — testes de endpoints em tests/api/SistemaEstoque.API.Tests/SupplierEndpointsTests.cs
T004 — testes web em tests/web/SuppliersPage.test.tsx

US2, depois de estabilizar o contrato do vínculo:
T018 — testes da entidade em tests/api/SistemaEstoque.Domain.Tests/VinculoProdutoFornecedorTests.cs
T019 — testes de endpoints em tests/api/SistemaEstoque.API.Tests/ProductSupplierEndpointsTests.cs
T020 — testes web em tests/web/ProductSupplierLinks.test.tsx

US3:
T036 — testes de consulta da API em tests/api/SistemaEstoque.API.Tests/ProductSearchBySupplierTests.cs
T037 — testes web de filtro em tests/web/ProductList.test.tsx
```

## Estratégia de implementação

### MVP primeiro (US1)

1. Reutilizar a fundação existente, sem adicionar setup desnecessário.
2. Implementar e validar a entidade e cadastro de fornecedor, CPF/CNPJ, pesquisa, edição, situação e permissões.
3. **Parar e validar** a US1 independentemente, incluindo duplicidade, auditoria e inativação sem vínculos ativos.
4. Demonstrar manutenção de fornecedores antes de prosseguir para associações.

### Entrega incremental

1. US1 → fornecedor gerenciável e auditado.
2. US2 → vínculos bilaterais, histórico e restrições de estado.
3. US3 → descoberta de produtos por fornecedor usando filtros combináveis.
4. Polish → validação final das histórias entregues e atualização do quickstart.

## Notas

- `[P]` identifica tarefas em arquivos distintos sem dependência de tarefa incompleta.
- `[US1]`, `[US2]` e `[US3]` vinculam cada tarefa à história correspondente da especificação.
- A auditoria usa conta autenticada e horário UTC; mutações não registram dados pessoais completos sem necessidade.
- Não executar as migrações automaticamente no startup nem introduzir pacote, projeto ou camada nova.
