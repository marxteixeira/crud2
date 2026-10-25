# Tarefas: Localização e produtos

**Entrada**: Documentos de design em `specs/007-localizacao-produtos/`

**Pré-requisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md` e `contracts/catalog-api.yaml`

**Testes**: Incluídos por exigência da Constituição do projeto (testes de domínio, endpoints e componentes com comportamento).

**Organização**: Tarefas agrupadas por história para permitir validação incremental. Todas as histórias dependem da normalização compartilhada; US2 depende das localizações da US1; US3 depende dos cadastros de US1 e US2.

## Fase 1: Setup

**Objetivo**: Preparação dos projetos.

Os projetos .NET, web e testes já existem e as dependências requeridas estão instaladas. Nenhuma alteração de estrutura ou pacote é necessária; não há tarefa de setup pendente.

---

## Fase 2: Fundacional

**Objetivo**: Fornecer a normalização determinística usada para impedir duplicidades em nomes e códigos em todas as histórias.

- [ ] T001 Implementar normalização Unicode, remoção de marcas diacríticas e caixa invariável em `src/services/api/SistemaEstoque.Domain/ValueObjects/ChaveNormalizada.cs`
- [ ] T002 Cobrir texto vazio, espaços externos, diferenças de caixa, acentos e equivalência Unicode em `tests/api/SistemaEstoque.Domain.Tests/ChaveNormalizadaTests.cs`

**Checkpoint**: Concluir a normalização compartilhada antes de iniciar os cadastros.

---

## Fase 3: User Story 1 - Organizar estantes e prateleiras (Priority: P1)

**Objetivo**: Permitir manter estantes e prateleiras autenticadamente e selecionar apenas prateleiras da estante correspondente.

**Teste independente**: Cadastrar e consultar estante/prateleiras; confirmar vínculo correto e impedir códigos duplicados após normalização. A restrição de inativação de localizações usadas será validada quando houver produtos na US2.

### Implementação da User Story 1

- [ ] T003 [US1] Criar `Estante` com `id` UUID; `code` obrigatório e único sem distinção de caixa/acentos; `normalized_code` obrigatório; `description` opcional; `status` Active/Inactive; `created_at`/`updated_at` UTC; `created_by`/`updated_by` UUID em `src/services/api/SistemaEstoque.Domain/Entities/Estante.cs`
- [ ] T004 [US1] Criar `Prateleira` com `id` UUID; `rack_id` obrigatório; `code` obrigatório; `normalized_code` único por estante sem distinção de caixa/acentos; `status` Active/Inactive; auditoria UTC e UUID em `src/services/api/SistemaEstoque.Domain/Entities/Prateleira.cs`
- [ ] T005 [US1] Testar criação, edição, normalização de códigos, vínculo com uma única estante e transições de situação em `tests/api/SistemaEstoque.Domain.Tests/LocalizacaoTests.cs`
- [ ] T006 [US1] Mapear `Estante` e `Prateleira` no EF Core, com chave única de código normalizado, chave composta da prateleira, chaves estrangeiras restritivas e auditoria em `src/services/api/SistemaEstoque.Infrastructure/Persistence/Configurations/EstanteConfiguration.cs`, `src/services/api/SistemaEstoque.Infrastructure/Persistence/Configurations/PrateleiraConfiguration.cs` e `src/services/api/SistemaEstoque.Infrastructure/Persistence/SistemaEstoqueDbContext.cs`
- [ ] T007 [US1] Criar migração explícita para tabelas de estantes e prateleiras, índices normalizados e vínculos restritivos em `src/services/api/SistemaEstoque.Infrastructure/Persistence/Migrations/`
- [ ] T008 [US1] Definir DTOs, operações e validadores de criação, edição e situação de estantes/prateleiras em `src/services/api/SistemaEstoque.Application/Locations/LocationContracts.cs` e `src/services/api/SistemaEstoque.Application/Locations/LocationValidators.cs`
- [ ] T009 [US1] Implementar os casos de uso de consulta e manutenção de estantes/prateleiras, incluindo atribuição de autoria pela conta autenticada e normalização dos códigos, em `src/services/api/SistemaEstoque.Application/Locations/LocationManagementService.cs`
- [ ] T010 [US1] Implementar endpoints protegidos de estantes e prateleiras conforme `contracts/catalog-api.yaml`, incluindo validação de que a prateleira pertence à estante informada, em `src/services/api/SistemaEstoque.API/Controllers/LocationsController.cs`
- [ ] T011 [US1] Cobrir endpoints de localização, autorização, códigos duplicados, vínculo inválido e mudanças de situação com PostgreSQL de teste em `tests/api/SistemaEstoque.API.Tests/LocationEndpointsTests.cs`
- [ ] T012 [P] [US1] Implementar funções tipadas de consulta e manutenção de estantes/prateleiras conforme o contrato HTTP em `src/apps/web/lib/locations.ts`
- [ ] T013 [US1] Criar interface acessível e mobile-first para consultar e manter estantes/prateleiras, com prateleiras dependentes da estante escolhida, em `src/apps/web/app/locations/page.tsx` e `src/apps/web/app/locations/LocationsPageClient.tsx`
- [ ] T014 [US1] Testar manutenção de estantes/prateleiras, vínculo entre seleção de estante e prateleira e mensagens de erro da interface em `tests/web/LocationsPage.test.tsx`

**Checkpoint**: A história é demonstrável sem produtos: pessoas autenticadas mantêm a estrutura e uma prateleira é sempre consultada no contexto da estante proprietária.

---

## Fase 4: User Story 2 - Manter o catálogo de produtos (Priority: P1)

**Objetivo**: Cadastrar, editar, inativar e reativar produtos com regras de dados, auditoria e controle de preço por perfil.

**Teste independente**: Cadastrar e editar produto; verificar unicidade, quantidade, preço, localização, auditoria e preservação do histórico. Operador cadastra sem preço como inativo; Administrador informa preço e ativa. Confirmar também que uma localização usada por produtos ativos não pode ser inativada.

**Dependência**: Fase 2 e US1, pois produto ativo depende de prateleira e estante válidas.

### Implementação da User Story 2

- [ ] T015 [US2] Criar `Produto` com `id` UUID; `name` obrigatório e não vazio; `normalized_name` obrigatório e único inclusive entre inativos; `quantity` inteiro maior ou igual a zero; `price` opcional, não negativo, em reais com precisão de centavos; `shelf_id` opcional apenas enquanto inativo; `status` Active/Inactive; `created_at`/`updated_at` UTC; `created_by`/`updated_by` UUID em `src/services/api/SistemaEstoque.Domain/Entities/Produto.cs`
- [ ] T016 [US2] Implementar invariantes do produto: ativo exige preço e prateleira ativa pertencente a estante ativa; ativação exige preço/localização válidos; inativação preserva cadastro; alterações registram ator e horário UTC em `src/services/api/SistemaEstoque.Domain/Entities/Produto.cs`
- [ ] T017 [US2] Testar nome vazio e duplicado normalizado, quantidade negativa, preço negativo ou com mais de duas casas, transições de situação, localização obrigatória para ativo e auditoria em `tests/api/SistemaEstoque.Domain.Tests/ProdutoTests.cs`
- [ ] T018 [US2] Mapear `Produto` com índice único em `normalized_name`, índices de situação/localização, chave estrangeira restritiva para prateleira e restrições numéricas em `src/services/api/SistemaEstoque.Infrastructure/Persistence/Configurations/ProdutoConfiguration.cs` e `src/services/api/SistemaEstoque.Infrastructure/Persistence/SistemaEstoqueDbContext.cs`
- [ ] T019 [US2] Criar migração explícita da tabela de produtos, índices e restrições de preço/quantidade em `src/services/api/SistemaEstoque.Infrastructure/Persistence/Migrations/`
- [ ] T020 [US2] Definir comandos, DTOs e validadores de produto, preservando preço opcional somente para inativos, em `src/services/api/SistemaEstoque.Application/Products/ProductContracts.cs` e `src/services/api/SistemaEstoque.Application/Products/ProductValidators.cs`
- [ ] T021 [US2] Implementar cadastro, edição, detalhe e transições de situação de produto com autoria; rejeitar campo de preço de Operador no servidor e limitar definição/alteração de preço a Administrador em `src/services/api/SistemaEstoque.Application/Products/ProductManagementService.cs`; estender `src/services/api/SistemaEstoque.Application/Locations/LocationManagementService.cs` para recusar inativação de estante/prateleira que contenha produtos ativos
- [ ] T022 [US2] Implementar endpoints autenticados de criação, detalhe, edição e situação conforme `contracts/catalog-api.yaml`, com respostas Problem Details para validação/conflito e autorização de preço em `src/services/api/SistemaEstoque.API/Controllers/ProductsController.cs`; aplicar a regra de inativação de localizações em `src/services/api/SistemaEstoque.API/Controllers/LocationsController.cs`
- [ ] T023 [US2] Testar cadastro/edição de produto, autorização de preço por Administrador/Operador, ativação sem preço/localização, auditoria e bloqueio de inativação de localização usada em `tests/api/SistemaEstoque.API.Tests/ProductEndpointsTests.cs`
- [ ] T024 [P] [US2] Implementar funções tipadas para cadastro, detalhe, edição e situação de produto conforme o contrato em `src/apps/web/lib/products.ts`
- [ ] T025 [US2] Criar formulário acessível de produto com validação Zod e seleção de estante/prateleira; omitir preço para Operador e apresentar produto sem preço como inativo em `src/apps/web/app/products/new/page.tsx` e `src/apps/web/components/products/ProductForm.tsx`
- [ ] T026 [US2] Criar interface de edição/detalhe e ações de inativar/reativar produto, exibindo preço somente como leitura para Operador e editável apenas para Administrador, em `src/apps/web/app/products/[productId]/page.tsx` e `src/apps/web/components/products/ProductDetails.tsx`
- [ ] T027 [US2] Testar criação e edição com os dois perfis, validações, estados sem preço e recusa visual de alteração de preço pelo Operador em `tests/web/ProductForm.test.tsx`

**Checkpoint**: Produtos válidos são persistidos e auditados; nenhuma operação de Operador define preço; produto sem preço não pode ficar ativo.

---

## Fase 5: User Story 3 - Encontrar produtos e consultar uma localização (Priority: P1)

**Objetivo**: Pesquisar, filtrar e paginar o catálogo e consultar produtos por estante/prateleira e seus detalhes.

**Teste independente**: Com produtos ativos e inativos em diversas localizações, combinar busca, filtros e paginação; verificar conteúdo por localização e detalhe completo.

**Dependência**: US1 para localizações e US2 para catálogo persistido.

### Implementação da User Story 3

- [ ] T028 [US3] Implementar consulta paginada com busca por nome normalizado e filtros combináveis de estante, prateleira e situação usando leituras sem rastreamento em `src/services/api/SistemaEstoque.Application/Products/ProductSearchService.cs`
- [ ] T029 [US3] Implementar listagem paginada de produtos, busca/filtros e consulta paginada do conteúdo de uma prateleira nos endpoints definidos em `contracts/catalog-api.yaml`, validando `page >= 1` e `1 <= pageSize <= 100`, em `src/services/api/SistemaEstoque.API/Controllers/ProductsController.cs` e `src/services/api/SistemaEstoque.API/Controllers/LocationsController.cs`
- [ ] T030 [US3] Testar combinação de busca/filtros, paginação, situação, localização vazia e conteúdo de prateleira em `tests/api/SistemaEstoque.API.Tests/ProductSearchEndpointsTests.cs`
- [ ] T031 [US3] Medir busca em catálogo de 5.000 produtos com pelo menos 100 requisições após aquecimento e verificar p95 de no máximo 2 segundos em `tests/api/SistemaEstoque.API.Tests/ProductSearchPerformanceTests.cs`
- [ ] T032 [US3] Implementar função tipada de listagem e consulta paginada com filtros em `src/apps/web/lib/products.ts`
- [ ] T033 [US3] Criar catálogo com busca, filtros por estante/prateleira/situação, paginação e estado vazio; manter filtros e página na URL em `src/apps/web/app/products/page.tsx` e `src/apps/web/components/products/ProductList.tsx`
- [ ] T034 [US3] Exibir produtos da localização escolhida e link para detalhe, com consulta à API e paginação, em `src/apps/web/app/locations/[rackId]/shelves/[shelfId]/page.tsx`
- [ ] T035 [US3] Testar busca, filtros combinados, paginação na URL, estados vazios e lista de produtos da localização em `tests/web/ProductList.test.tsx`

**Checkpoint**: Usuários encontram produtos e consultam sua localização com filtros combináveis; listagens não carregam o catálogo inteiro.

---

## Fase 6: Polish e preocupações transversais

**Objetivo**: Confirmar gates completos, contrato, desempenho, documentação operacional e critérios de aceite entre histórias.

- [ ] T036 Atualizar a navegação autenticada e os links entre catálogo, detalhe, localização e telas existentes em `src/apps/web/app/layout.tsx`
- [ ] T037 Confirmar que o contrato OpenAPI corresponde aos endpoints e validar seu YAML em `specs/007-localizacao-produtos/contracts/catalog-api.yaml`
- [ ] T038 Executar cenários automatizados e ponta a ponta e registrar resultados esperados em `specs/007-localizacao-produtos/quickstart.md`
- [ ] T039 Executar validação da API, web, lint, builds, busca com 5.000 produtos e gates existentes e corrigir falhas da feature em `specs/007-localizacao-produtos/plan.md`

## Dependências e ordem de execução

### Dependências entre fases

- **Setup (Fase 1)**: já satisfeito pelos projetos e dependências existentes.
- **Fundacional (Fase 2)**: bloqueia US1 e US2, fornecendo regra comum de normalização.
- **US1 (Fase 3)**: depende da Fase 2; entrega estantes e prateleiras persistentes.
- **US2 (Fase 4)**: depende da Fase 2 e US1; produtos ativos precisam de localização válida.
- **US3 (Fase 5)**: depende de US1 e US2; busca e consulta dependem de localizações e produtos persistidos.
- **Polish (Fase 6)**: depende das três histórias implementadas.

### Grafo das histórias

```text
Fase 2: normalização compartilhada
  └── US1: estantes e prateleiras
       └── US2: cadastro e manutenção de produtos
            └── US3: busca, filtros e consulta por localização
                 └── Polish: gates completos e validação ponta a ponta
```

### Oportunidades de paralelismo

- **Fase 2**: T001 e T002 trabalham em arquivos distintos, mas T002 depende da assinatura definida em T001; portanto devem ser executadas em sequência para evitar divergência.
- **US1**: T012 pode avançar em paralelo à implementação do servidor após estabilização do contrato; T011 segue a implementação HTTP; T013 depende de T012.
- **US2**: T024 pode avançar em paralelo à implementação do servidor após estabilização do contrato; T017 e T023 seguem as implementações que validam; T025 e T026 usam componentes distintos, mas compartilham integração visual.
- **US3**: T028 e T029 implementam a consulta no servidor antes de T030, T031 e T032; depois, a interface depende da função em `products.ts`.
- **Entre histórias**: não há paralelismo seguro de implementação porque US2 depende de US1 e US3 depende de US1/US2.

## Exemplos de execução paralela

```text
US1, após estabilização do contrato:
T012 — cliente HTTP web em src/apps/web/lib/locations.ts

US2, após contrato/modelo estabilizados:
T024 — funções HTTP web em src/apps/web/lib/products.ts

US3, após a consulta estar desenhada:
T032 — funções de consulta web em src/apps/web/lib/products.ts
```

## Estratégia de implementação

### MVP primeiro (US1)

1. Concluir a normalização compartilhada da Fase 2.
2. Concluir US1 e validar manutenção de estantes/prateleiras de forma independente.
3. **Parar e validar** o vínculo estante-prateleira e a regra de inativação antes de iniciar o catálogo.

### Entrega incremental

1. Fase 2 + US1: estrutura física selecionável e persistente.
2. US2: cadastro, controle de preço por perfil e histórico dos produtos.
3. US3: busca, filtros, paginação, consulta de localização e medição de desempenho.
4. Polish: gates e critérios ponta a ponta da spec.

## Notas

- `[P]` significa que os arquivos são distintos e a tarefa não depende de tarefa inacabada além dos pré-requisitos indicados na fase.
- Cada tarefa de história inclui o rótulo correspondente e caminho explícito.
- Os limites e regras do [data-model.md](data-model.md) devem ser preservados sem flexibilização durante a implementação.
- Fornecedores, importação de planilha e exclusão definitiva permanecem fora desta fase.
