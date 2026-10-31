# Feature Specification: Recuperar cadastro de prateleira

**Feature Branch**: `016-recuperar-cadastro-prateleira`

**Created**: 2026-10-31

**Status**: Draft

**Input**: User description: "Em uma das specs passadas, foi removida a função de cadastrar prateleira. Recupere essa função ou desenvolva novamente."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Cadastrar prateleira na estante selecionada (Priority: P1)

Como pessoa que organiza as localizações do estoque, quero cadastrar uma prateleira na estante selecionada para poder armazenar e localizar produtos em uma divisão específica.

**Why this priority**: Sem o cadastro, uma estante não pode ser subdividida para a localização de produtos, e o fluxo removido na melhoria de Localizações continua indisponível.

**Independent Test**: Selecionar uma estante ativa, informar um código de prateleira válido e confirmar; verificar que a prateleira aparece na lista dessa estante e pode ser escolhida como localização de um produto.

**Acceptance Scenarios**:

1. **Given** uma pessoa autenticada e uma estante ativa selecionada, **When** informa um código válido e confirma o cadastro, **Then** a prateleira é criada e aparece na área de prateleiras da estante selecionada.
2. **Given** uma estante selecionada, **When** a pessoa consulta as prateleiras disponíveis para localizar um produto, **Then** a prateleira recém-cadastrada pode ser selecionada somente junto à sua estante proprietária.
3. **Given** uma estante sem prateleiras, **When** a pessoa cadastra a primeira prateleira, **Then** a nova prateleira aparece e a mensagem de estado vazio deixa de ser apresentada.
4. **Given** uma estante selecionada, **When** a pessoa inicia o cadastro, **Then** encontra o campo de código da prateleira e a ação para confirmar o cadastro.

### User Story 2 - Receber validação clara ao cadastrar (Priority: P2)

Como pessoa que mantém as localizações, quero receber uma indicação compreensível quando o código da prateleira estiver ausente ou já existir naquela estante, para corrigir o cadastro sem criar registros ambíguos.

**Why this priority**: Códigos válidos e únicos por estante permitem distinguir as localizações e evitam que produtos sejam associados a uma prateleira incorreta.

**Independent Test**: Tentar cadastrar um código vazio e, em seguida, repetir um código já cadastrado na mesma estante; confirmar que cada tentativa é recusada com uma mensagem clara e que nenhuma duplicidade é criada.

**Acceptance Scenarios**:

1. **Given** o código da prateleira vazio ou composto apenas por espaços, **When** a pessoa confirma o cadastro, **Then** o sistema recusa a operação e indica que o código é obrigatório.
2. **Given** uma prateleira com determinado código já existente na estante selecionada, **When** a pessoa tenta cadastrar o mesmo código, ignorando diferenças de maiúsculas, minúsculas ou acentos, **Then** o sistema recusa a duplicidade e mantém a lista sem alterações.
3. **Given** que o código já existe em outra estante, **When** a pessoa cadastra esse código na estante selecionada, **Then** o sistema permite o cadastro, desde que não haja duplicidade nessa estante.
4. **Given** uma falha inesperada durante o cadastro, **When** o sistema informa o resultado, **Then** não indica sucesso, preserva o texto digitado e apresenta uma mensagem clara para a pessoa.

### Edge Cases

- Se nenhuma estante estiver selecionada, o cadastro de prateleira não pode ser iniciado.
- Se a estante selecionada estiver inativa ou deixar de existir durante o cadastro, a operação é recusada e a pessoa recebe uma mensagem compreensível.
- Cadastros simultâneos do mesmo código para a mesma estante não podem criar duplicidades.
- Se o cadastro falhar, as prateleiras existentes permanecem visíveis e os dados informados não são descartados.
- Se a operação for concluída, mas a atualização da lista falhar, o sistema não deve comunicar que a prateleira não foi criada nem apresentar uma segunda tentativa como se a primeira não tivesse sido persistida.
- Prateleiras já cadastradas e seus fluxos existentes de consulta, edição, situação e consulta de produtos vinculados permanecem disponíveis.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST permitir que Administradores e Operadores autenticados iniciem o cadastro de uma prateleira quando uma estante ativa estiver selecionada.
- **FR-002**: O sistema MUST apresentar um campo para informar o código da prateleira e uma ação para confirmar seu cadastro na área de prateleiras da estante selecionada.
- **FR-003**: O sistema MUST exigir um código de prateleira não vazio após a remoção de espaços em branco no início e no fim.
- **FR-004**: O sistema MUST associar cada prateleira criada à estante ativa selecionada no momento do cadastro.
- **FR-005**: O sistema MUST impedir códigos duplicados na mesma estante, desconsiderando diferenças de maiúsculas, minúsculas e acentos, e MUST permitir o mesmo código em estantes diferentes.
- **FR-006**: Após a confirmação bem-sucedida, o sistema MUST atualizar a área da estante selecionada para apresentar a nova prateleira sem ocultar as demais prateleiras.
- **FR-007**: O sistema MUST disponibilizar a prateleira criada para os fluxos existentes de seleção de localização de produtos, respeitando a relação com sua estante.
- **FR-008**: O sistema MUST informar validações, duplicidades e falhas inesperadas sem indicar sucesso quando o cadastro não tiver sido concluído.
- **FR-009**: Em caso de falha, o sistema MUST preservar o código informado e as prateleiras previamente apresentadas.
- **FR-010**: O sistema MUST manter os fluxos existentes de consulta, edição, ativação/inativação e consulta de produtos vinculados às prateleiras.
- **FR-011**: O sistema MUST respeitar as permissões e regras existentes de estantes, prateleiras e produtos, incluindo a restrição de seleção de prateleiras à estante correspondente.
- **FR-012**: Testes automatizados MUST cobrir cadastro bem-sucedido, validação do código obrigatório, duplicidade na mesma estante e reutilização do código em outra estante.

### Key Entities *(include if feature involves data)*

- **Estante**: Localização ativa que agrupa zero ou mais prateleiras.
- **Prateleira**: Divisão identificada por código, pertencente a uma única estante e disponível para localização de produtos.
- **Produto**: Item que pode ser associado a uma prateleira pertencente à estante escolhida.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% dos cadastros válidos, a pessoa consegue concluir o registro de uma prateleira em até 1 minuto.
- **SC-002**: Em 100% dos cadastros concluídos com sucesso, a prateleira fica visível na estante correta sem ação manual adicional de atualização pela pessoa.
- **SC-003**: Em 100% das tentativas de código vazio ou duplicado na mesma estante, o cadastro é recusado com uma mensagem compreensível e sem criar uma segunda prateleira.
- **SC-004**: Em 100% dos cadastros, uma prateleira só fica disponível para produtos junto à estante à qual pertence.
- **SC-005**: A reintrodução do cadastro não impede a consulta nem a manutenção das prateleiras já existentes.

## Assumptions

- A solicitação refere-se ao cadastro de uma prateleira por vez na tela de Localizações, junto à área de prateleiras da estante selecionada; não inclui cadastro em lote.
- O código continua sendo o único dado preenchido no cadastro da prateleira, pois a entidade existente não define descrição para prateleiras.
- As regras de unicidade e normalização já estabelecidas para prateleiras são mantidas: código único dentro da mesma estante, sem distinção de caixa ou acentos.
- Os perfis Administrador e Operador mantêm as permissões de gerenciamento de localizações descritas na feature 007.
- A criação da prateleira não altera o cadastro, a seleção ou a manutenção das estantes.
- Esta feature complementa a simplificação da interface definida na feature 013, restabelecendo somente o cadastro de prateleira removido; os demais comportamentos dessa feature permanecem inalterados.
