# Feature Specification: Corrigir cadastro de estantes e erros da fase de Produtos e Localizações

**Feature Branch**: `008-corrigir-cadastro-estantes`

**Created**: 2026-10-25

**Status**: Draft

**Input**: User description: "Em teste manual da fase de produtos e localizações, ao cadastrar uma estante na página de Localizações, a requisição de criação de estante retorna 500 Internal Server Error. O log da API mostra apenas uma requisição de verificação prévia (OPTIONS) respondida com 204. Analisar com foco na resolução."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Cadastrar uma estante com sucesso (Priority: P1)

Um usuário com permissão de gestão de localizações abre a página de Localizações, informa código e descrição de uma nova estante e confirma. A estante deve ser criada e aparecer na listagem, sem erro.

**Why this priority**: É o defeito bloqueante relatado; sem estantes não há prateleiras nem produtos localizados.

**Independent Test**: Cadastrar uma estante com código inédito pela página de Localizações e verificar que ela aparece na lista após confirmar.

**Acceptance Scenarios**:

1. **Given** um usuário autenticado com permissão e um código de estante inexistente, **When** ele envia o cadastro, **Then** a estante é criada, a lista é atualizada e uma confirmação é exibida.
2. **Given** uma estante já existente com o mesmo código (ignorando maiúsculas/minúsculas e espaços), **When** o usuário tenta cadastrar, **Then** vê mensagem clara de duplicidade, e não um erro genérico.
3. **Given** dados inválidos (código vazio ou acima do limite), **When** o usuário envia, **Then** vê mensagens de validação por campo.

---

### User Story 2 - Falhas inesperadas serem diagnosticáveis (Priority: P2)

Quando ocorrer um erro interno em qualquer operação de produtos e localizações, o usuário vê uma mensagem amigável com identificador de rastreio, e a equipe consegue localizar a causa real nos registros da API pelo mesmo identificador.

**Why this priority**: O log do relato mostrou só a verificação prévia, sem a causa do 500, o que atrasou o diagnóstico.

**Independent Test**: Provocar uma falha controlada em uma operação e confirmar que o registro contém a causa e o identificador exibido ao usuário.

**Acceptance Scenarios**:

1. **Given** uma falha interna ao cadastrar, **When** a resposta de erro é exibida, **Then** a mensagem é amigável e inclui o identificador de rastreio.
2. **Given** o mesmo identificador, **When** a equipe consulta os registros, **Then** encontra a causa detalhada da falha.

---

### User Story 3 - Demais fluxos de Produtos e Localizações funcionarem em ambiente novo (Priority: P2)

Um testador percorre todos os fluxos da fase (estantes, prateleiras, posições, produtos, vínculo de produto à localização, inativação) em ambiente local recém-configurado, sem erros inesperados.

**Why this priority**: A causa provável (estrutura de dados da fase não aplicada ao banco do ambiente) afeta todos os fluxos, não só estantes.

**Independent Test**: Seguir o roteiro de validação da fase 007 em ambiente local limpo e concluir todos os passos sem erro 500.

**Acceptance Scenarios**:

1. **Given** um ambiente local configurado conforme o guia, **When** a API é iniciada, **Then** a estrutura de dados necessária à fase está disponível ou o guia indica claramente o passo para disponibilizá-la.
2. **Given** a estrutura de dados ausente ou desatualizada, **When** a API é iniciada, **Then** a equipe recebe um aviso claro em vez de falhas só em tempo de uso.

---

### Edge Cases

- Cadastros simultâneos do mesmo código de estante: apenas um é criado; o outro recebe mensagem de duplicidade.
- Usuário sem permissão: recebe resposta de acesso negado, não erro interno.
- Sessão expirada durante o cadastro: usuário é direcionado a autenticar novamente.
- Verificação prévia de origem cruzada (OPTIONS) deve continuar respondendo sem registrar falso erro; o 405 exibido no log não é a causa do 500.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST permitir cadastrar estante com código e descrição válidos e exibi-la na listagem.
- **FR-002**: O sistema MUST rejeitar código duplicado com mensagem de conflito clara.
- **FR-003**: O sistema MUST validar campos e retornar mensagens por campo.
- **FR-004**: Erros internos MUST retornar mensagem amigável com identificador de rastreio, sem expor detalhes técnicos.
- **FR-005**: Todo erro interno MUST ser registrado com a causa detalhada e o mesmo identificador de rastreio.
- **FR-006**: O sistema MUST garantir que a estrutura de dados da fase de Produtos e Localizações esteja disponível no ambiente, ou sinalizar claramente na inicialização quando não estiver.
- **FR-007**: A documentação de configuração local MUST descrever como preparar o ambiente para a fase.
- **FR-008**: Todos os fluxos de estantes, prateleiras, posições e produtos MUST ser percorridos em teste manual sem erro interno.
- **FR-009**: O registro de requisições MUST exibir a rota real e não confundir verificações prévias com falhas.
- **FR-010**: Testes automatizados MUST cobrir o cadastro de estante (sucesso, duplicidade, validação) para evitar regressão.

### Key Entities

- **Estante**: agrupador de localização com código único e descrição; possui prateleiras.
- **Prateleira / Posição**: subdivisões da estante onde produtos são armazenados.
- **Registro de erro**: ocorrência interna identificada por rastreio, com causa detalhada.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos cadastros de estante válidos concluem com sucesso na primeira tentativa.
- **SC-002**: Cadastro de estante concluído em menos de 1 minuto pelo usuário.
- **SC-003**: Roteiro manual completo da fase conclui sem nenhum erro interno em ambiente novo.
- **SC-004**: 100% dos erros internos podem ser ligados à sua causa pelo identificador de rastreio exibido.
- **SC-005**: Um membro novo da equipe prepara o ambiente e cadastra a primeira estante em até 15 minutos seguindo o guia.

## Assumptions

- A causa mais provável do 500 é a estrutura de dados da fase não aplicada ao banco local (migrações criadas, mas sem aplicação documentada/automática); deve ser confirmada na análise do plano.
- O 405/OPTIONS do log é verificação prévia de origem cruzada e não é a causa.
- Escopo limitado à fase 007; sem novas funcionalidades.
- Permissões e perfis existentes são mantidos.
