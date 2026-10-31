# Pesquisa: Recuperar cadastro de prateleira

## Decisões

### Reutilizar a operação de criação existente

- **Decisão**: A interface deve chamar `createShelf` em `src/apps/web/lib/locations.ts`, que já envia `POST /api/locations/racks/{rackId}/shelves` com `{ code }`.
- **Racional**: A API e o cliente já implementam o cadastro. A feature 013 removeu o formulário e o botão da tela, não a capacidade do sistema.
- **Alternativas consideradas**: Criar outro endpoint ou uma nova função cliente; duplicaria comportamento já existente e ampliaria o escopo sem necessidade.

### Manter regras de domínio e validação no servidor

- **Decisão**: Reutilizar validação de código obrigatório, validação da estante ativa e conflito de código já cadastrado naquela estante. Exibir os erros retornados sem sugerir sucesso.
- **Racional**: `CreateShelfRequestValidator` exige código; o serviço rejeita estante inexistente ou inativa e verifica duplicidade com o código normalizado. A entidade armazena código normalizado e dados de auditoria.
- **Alternativas consideradas**: Reimplementar regras de unicidade no cliente; a validação cliente isolada não evita conflito simultâneo e duplicaria regra de negócio.

### Restaurar o formulário na seção da estante selecionada

- **Decisão**: Apresentar um campo obrigatório de código e ação de cadastro na seção de prateleiras, somente quando uma estante ativa estiver selecionada. Atualizar a lista da localização após o sucesso.
- **Racional**: A operação precisa de `rackId`; manter o formulário junto da lista selecionada esclarece a estante proprietária e se integra ao fluxo já existente de gerenciamento.
- **Alternativas consideradas**: Formulário global sem contexto explícito; aumenta o risco de cadastrar na estante errada. Criar uma página ou diálogo separado; não é necessário para um único campo e desviaria do contexto atual.

### Não alterar o contrato HTTP

- **Decisão**: Não modificar a API nem o contrato OpenAPI nesta feature. A resposta `201` com `ShelfDto`, validação `400`, conflito `409` e ausência de estante `404` já estão definidos em `specs/007-localizacao-produtos/contracts/catalog-api.yaml`.
- **Racional**: A capacidade de backend é implementada e exercitada pelos testes de integração existentes; a lacuna está na experiência web após a remoção dos controles.
- **Alternativas consideradas**: Atualizar endpoints ou DTOs; não há requisito novo de domínio ou dados que justifique mudança.

## Padrões e restrições relevantes

- A tela está em `src/apps/web/app/locations/LocationsPageClient.tsx`; as funções de API estão em `src/apps/web/lib/locations.ts`.
- A tela usa TanStack Query para listar estantes e invalidar consultas após mutações.
- A seção de prateleiras já permite edição, alteração de situação e consulta de produtos. Esses fluxos devem continuar disponíveis.
- O teste focal está em `tests/web/LocationsPage.test.tsx`, e atualmente verifica que os controles de cadastro não aparecem; esse comportamento deve ser substituído por cobertura de cadastro.
- O modelo `Prateleira` contém código, estado, referência à estante e campos de auditoria. Não há descrição de prateleira.
- O endpoint requer pessoa autenticada e estante ativa; códigos duplicados são recusados dentro da mesma estante após normalização.
- O protótipo visual existente é `docs/05-design-system/estantes_e_prateleiras/code.html`, com referência de tela `docs/05-design-system/estantes_e_prateleiras/screen.png`.
