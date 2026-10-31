# Feature Specification: Melhorar a experiência de Localizações

**Feature Branch**: `013-melhoria-localizacoes`

**Created**: 2026-10-30

**Status**: Draft

**Input**: User description: "No meu Localizações, a tela não está intuitiva: transformar a seção 'Cadastrar Nova Estante / Crie uma nova localização para organizar o estoque' em um modal aberto somente pelo botão '+ Cadastrar Nova Estante'; depois de cadastrar, fechar o modal e recarregar a página principal com a estante recém-cadastrada carregada; deixar vazios, sem texto de exemplo, os campos 'Código' e 'Descrição' na seção de prateleiras; remover o código da prateleira e o botão 'Cadastrar prateleira' por serem redundantes."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Cadastrar estante pelo modal (Priority: P1)

Como pessoa que organiza as localizações do estoque, quero abrir o cadastro de estante quando eu solicitar, para manter a página principal mais clara e focada na consulta das localizações.

**Why this priority**: O cadastro de estantes é o principal fluxo de criação de localizações, e sua apresentação permanente ocupa espaço mesmo quando não está sendo usada.

**Independent Test**: Abrir Localizações, verificar que o formulário de cadastro não aparece, acionar o botão de cadastro, preencher os dados e salvar; confirmar que o modal fecha e a nova estante aparece selecionada na página atualizada.

**Acceptance Scenarios**:

1. **Given** a página de Localizações aberta, **When** a pessoa ainda não acionou o botão "+ Cadastrar Nova Estante", **Then** o formulário de cadastro permanece fechado e não ocupa a página principal.
2. **Given** a página de Localizações aberta, **When** a pessoa aciona "+ Cadastrar Nova Estante", **Then** o formulário de cadastro é apresentado em um modal.
3. **Given** o modal aberto com dados válidos, **When** a pessoa confirma o cadastro e a operação é concluída, **Then** o modal fecha e a página principal é atualizada com a nova estante carregada e selecionada.
4. **Given** o modal aberto, **When** a pessoa o fecha sem salvar, **Then** nenhum cadastro é realizado e a página principal continua disponível.

---

### User Story 2 - Consultar prateleiras sem controles redundantes (Priority: P2)

Como pessoa que consulta as prateleiras de uma localização, quero encontrar apenas os controles necessários e ver os campos de Código e Descrição vazios, sem exemplos que possam ser confundidos com dados reais.

**Why this priority**: Remover os controles redundantes simplifica a tela e evita interpretações erradas de valores de exemplo.

**Independent Test**: Abrir o modal de nova estante e verificar que os campos Código e Descrição começam vazios e sem texto de exemplo; depois, abrir uma estante com prateleiras, verificar que a lista continua consultável e que não há campo "Código da prateleira" nem botão "Cadastrar prateleira".

**Acceptance Scenarios**:

1. **Given** o modal de cadastro de estante aberto, **When** a pessoa inicia o preenchimento, **Then** os campos "Código" e "Descrição" estão vazios e não exibem texto de exemplo.
2. **Given** uma estante com prateleiras existentes, **When** a pessoa consulta sua área de prateleiras, **Then** as prateleiras existentes continuam visíveis e os controles redundantes de cadastro de prateleira não são apresentados.

### Edge Cases

- Se o cadastro da estante falhar, o modal permanece aberto, os dados informados não são descartados e a pessoa recebe uma mensagem clara de erro.
- Se a pessoa fechar o modal antes de salvar, os dados não são enviados nem uma estante é criada.
- Se a estante criada ainda não possuir prateleiras, ela aparece selecionada mesmo assim, e a área correspondente informa o estado vazio de maneira compreensível.
- A atualização após o cadastro não deve remover nem ocultar estantes e prateleiras que já estavam disponíveis.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A página principal de Localizações MUST iniciar sem exibir o formulário "Cadastrar Nova Estante".
- **FR-002**: O sistema MUST abrir o formulário de cadastro de estante em um modal somente após a pessoa acionar o botão "+ Cadastrar Nova Estante".
- **FR-003**: O modal MUST permitir que a pessoa feche ou cancele o cadastro sem criar uma estante.
- **FR-004**: Após um cadastro de estante concluído com sucesso, o sistema MUST fechar o modal e atualizar a página principal, exibindo a nova estante já carregada e selecionada.
- **FR-005**: Se o cadastro falhar, o sistema MUST manter o modal aberto, preservar os dados informados e apresentar uma mensagem clara de erro.
- **FR-006**: No formulário de cadastro de estante apresentado no modal, os campos "Código" e "Descrição" MUST iniciar vazios e não MUST exibir texto de exemplo.
- **FR-007**: A área de prateleiras MUST NOT apresentar o campo "Código da prateleira" nem o botão "Cadastrar prateleira".
- **FR-008**: A remoção dos controles de cadastro redundantes MUST preservar a consulta das prateleiras já existentes.

### Key Entities

- **Estante**: Localização cadastrada para organizar o estoque, identificada por código e descrição, que pode ser selecionada na página de Localizações.
- **Prateleira**: Divisão de uma estante que permanece consultável na área de prateleiras.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% das aberturas iniciais da página de Localizações, o formulário de cadastro de estante permanece fechado até que o botão correspondente seja acionado.
- **SC-002**: Em 100% dos cadastros concluídos com sucesso, o modal fecha e a nova estante fica visível e selecionada na página principal atualizada, sem ação manual adicional da pessoa.
- **SC-003**: Em 100% das aberturas do modal de cadastro, os campos Código e Descrição aparecem vazios e sem texto de exemplo; os controles de cadastro removidos não são exibidos na área de prateleiras.
- **SC-004**: Uma pessoa consegue localizar estantes e consultar as prateleiras existentes sem encontrar os controles redundantes removidos.

## Assumptions

- O botão "+ Cadastrar Nova Estante" continua disponível na página principal como entrada única para iniciar esse cadastro.
- O fechamento do modal sem salvar cancela a operação; erros de cadastro são informados sem indicar sucesso.
- A remoção solicitada limita-se ao campo "Código da prateleira" e ao botão "Cadastrar prateleira"; a visualização das prateleiras já existentes continua disponível.
- Os campos Código e Descrição mencionados para iniciar vazios são os do cadastro de nova estante; no editor de uma estante existente, seus valores atuais permanecem preenchidos para permitir a edição.
- Permissões, regras de validação e dados exigidos para cadastrar estantes permanecem conforme o comportamento existente.
