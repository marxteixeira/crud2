# Feature Specification: Identidade e perfis

**Feature Branch**: `003-identidade-perfis`

**Created**: 2026-10-23

**Status**: Draft

**Input**: User description: "Verifique qual é a próxima fase a ser construída para essa etapa." O roadmap existente identifica Identidade e perfis como a primeira fase de negócio após a fundação técnica.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Entrar e sair do sistema (Priority: P1)

Como Administrador ou Operador, quero entrar com minhas credenciais e encerrar meu acesso, para usar o sistema de forma individual e protegida.

**Why this priority**: O login é o primeiro requisito de negócio ainda não implementado e é pré-requisito para identificar quem realiza as alterações nas fases seguintes.

**Independent Test**: Com contas ativas dos dois perfis, verificar que cada pessoa consegue iniciar e encerrar sua própria sessão sem acessar os dados de credenciais de outra pessoa.

**Acceptance Scenarios**:

1. **Given** uma conta ativa e credenciais válidas, **When** a pessoa entra no sistema, **Then** passa a acessar as áreas permitidas ao seu perfil.
2. **Given** uma conta ativa e credenciais inválidas, **When** a pessoa tenta entrar, **Then** o acesso é recusado com uma mensagem compreensível e sem revelar qual parte das credenciais estava correta.
3. **Given** uma sessão autenticada, **When** a pessoa encerra a sessão, **Then** as áreas protegidas deixam de estar acessíveis até um novo login.
4. **Given** uma conta inativada, **When** a pessoa tenta iniciar uma sessão, **Then** o acesso é recusado.

### User Story 2 - Administrar usuários e perfis (Priority: P1)

Como Administrador, quero provisionar, consultar, editar e inativar contas e atribuir o perfil apropriado, para controlar quem pode utilizar o sistema.

**Why this priority**: A loja precisa conceder acesso aos operadores sem permitir que eles próprios ampliem seus privilégios.

**Independent Test**: Entrar como Administrador, criar contas Administrador e Operador, consultar e alterar seus dados e inativar uma conta; confirmar que os usuários administrados não recebem acesso de Administrador por padrão.

**Acceptance Scenarios**:

1. **Given** que o Administrador está autenticado, **When** provisiona uma conta com nome, login e perfil válido, **Then** a conta fica disponível para acesso conforme o perfil atribuído.
2. **Given** um login já utilizado, **When** o Administrador tenta provisionar outra conta com esse login, **Then** a operação é recusada e a conta existente permanece inalterada.
3. **Given** que o Administrador está autenticado, **When** consulta ou altera dados e perfil de uma conta, **Then** as alterações são salvas e passam a determinar as permissões da conta.
4. **Given** que o Administrador inativa uma conta, **When** essa conta tenta entrar novamente, **Then** o acesso é recusado sem apagar o registro da conta.
5. **Given** que um Operador está autenticado, **When** tenta acessar o gerenciamento de usuários ou alterar o próprio perfil, **Then** a ação é recusada e nenhum dado de usuário é alterado.
6. **Given** uma conta recém-provisionada, **When** seu titular conclui o estabelecimento inicial de credencial pelo processo seguro de acesso, **Then** consegue entrar sem que o Administrador tenha acesso à senha escolhida.

### User Story 3 - Aplicar permissões por perfil (Priority: P1)

Como responsável pela loja, quero que as permissões sejam verificadas em cada ação protegida, para que a interface não seja a única barreira contra operações não autorizadas.

**Why this priority**: As regras de perfil devem proteger as operações mesmo quando alguém tenta acessá-las por um caminho diferente da navegação normal.

**Independent Test**: Executar ações protegidas com cada perfil e verificar que ações permitidas são concluídas, ações proibidas são recusadas e nenhuma tentativa recusada altera dados.

**Acceptance Scenarios**:

1. **Given** um Administrador autenticado, **When** solicita uma ação reservada ao Administrador, **Then** a ação é permitida.
2. **Given** um Operador autenticado, **When** solicita uma ação reservada ao Administrador, **Then** a ação é recusada e os dados permanecem inalterados.
3. **Given** um Operador autenticado, **When** acessa as capacidades permitidas de consulta, cadastro e edição de produtos e quantidades que forem disponibilizadas nas fases posteriores, **Then** não pode alterar preços nem gerenciar usuários.
4. **Given** uma pessoa sem sessão autenticada, **When** tenta acessar uma área protegida, **Then** o acesso é recusado e a pessoa é direcionada a entrar no sistema.

### Edge Cases

- A pessoa informa credenciais inválidas, deixa campos obrigatórios vazios ou tenta entrar com uma conta inativada.
- O titular tenta usar instruções inválidas ou expiradas para estabelecer a credencial inicial; o acesso não é concedido.
- O Administrador tenta reutilizar um login já cadastrado ou atribuir um perfil que não seja Administrador ou Operador.
- Um Operador tenta alterar o próprio perfil, gerenciar outra conta ou executar uma ação de Administrador por acesso direto.
- Uma conta é inativada enquanto sua pessoa titular ainda possui uma sessão aberta; a sessão não pode continuar autorizando novas ações protegidas.
- A única conta Administrador ainda disponível é selecionada para inativação; o sistema deve impedir que essa operação deixe a loja sem qualquer Administrador ativo.
- O sistema não consegue concluir uma operação de login ou gerenciamento; deve informar a falha sem aparentar sucesso nem expor credenciais ou dados internos.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST permitir que pessoas com contas ativas entrem usando login e senha e encerrem suas próprias sessões.
- **FR-002**: O sistema MUST recusar login para credenciais inválidas ou contas inativadas e MUST apresentar uma mensagem que não revele se o login existe.
- **FR-003**: O sistema MUST permitir que Administradores provisionem e consultem contas contendo nome, login e perfil.
- **FR-004**: O sistema MUST permitir que Administradores editem os dados e o perfil de uma conta e inativem contas sem apagar seus registros.
- **FR-005**: O sistema MUST garantir que cada login seja único e que os únicos perfis atribuíveis sejam Administrador e Operador.
- **FR-006**: O sistema MUST permitir gerenciamento de usuários apenas a Administradores; um Operador não pode criar contas, editar perfis, inativar contas ou alterar seu próprio perfil.
- **FR-007**: O sistema MUST permitir alteração de preços somente a Administradores quando essa capacidade for disponibilizada, sem incluir nesta fase o cadastro ou a edição de produtos.
- **FR-008**: O sistema MUST verificar a sessão e a permissão exigida para cada ação protegida, recusando operações não autorizadas sem alterar dados.
- **FR-009**: O sistema MUST impedir que uma conta inativada ou uma sessão encerrada continue autorizando novas ações protegidas.
- **FR-010**: O sistema MUST impedir a inativação da última conta Administrador ativa.
- **FR-011**: O sistema MUST proteger credenciais contra leitura por outros usuários e não deve exibir senhas em consultas ou mensagens.
- **FR-012**: O sistema MUST permitir que o titular de uma conta recém-provisionada estabeleça sua própria senha por um processo seguro, sem revelar a senha ao Administrador.

### Key Entities

- **Usuário**: pessoa com nome, login, credencial protegida, perfil e situação ativa ou inativa.
- **Perfil de acesso**: conjunto de permissões associado a uma conta; nesta fase, Administrador ou Operador.
- **Sessão autenticada**: acesso temporário associado a uma pessoa identificada; deixa de autorizar operações após encerramento ou inativação da conta.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das tentativas de login com credenciais válidas de contas ativas dos dois perfis são concluídas com o perfil correto; 100% das tentativas inválidas ou de contas inativas são recusadas.
- **SC-002**: Em todos os testes de permissão, Operadores não conseguem gerenciar usuários nem alterar preços, e tentativas recusadas não modificam dados.
- **SC-003**: Um Administrador consegue provisionar, localizar, editar e inativar uma conta em até 3 minutos, sem assistência técnica.
- **SC-004**: Em uma validação com pelo menos cinco pessoas representando os perfis previstos, todas conseguem identificar como entrar e encerrar a sessão; pelo menos 90% concluem essas tarefas sem ajuda.
- **SC-005**: Após encerramento da sessão ou inativação da conta, 100% das novas tentativas de acessar áreas protegidas são recusadas.

## Assumptions

- A fase sucede a fundação técnica já existente e precede localização e produtos, conforme a sequência registrada em `specs/002-implementation-roadmap/spec.md`.
- A loja opera inicialmente com uma unidade e somente dois perfis: Administrador e Operador.
- Não há cadastro público: contas são provisionadas por um Administrador. A primeira conta Administrador será disponibilizada por um procedimento inicial seguro de implantação.
- Após o provisionamento, o titular estabelece a própria senha por um processo seguro de ativação; os detalhes operacionais desse processo serão definidos no planejamento.
- O gerenciamento de usuários cobre nome, login, perfil e situação da conta; não inclui recuperação automatizada de senha ou autenticação multifator nesta fase.
- As telas e operações de produtos ainda não fazem parte desta fase. As restrições de preço definem a política que será aplicada quando essas operações forem construídas.
- A inativação é reversível por um Administrador e preserva o registro para manter a rastreabilidade.
