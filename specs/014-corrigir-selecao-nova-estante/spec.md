# Feature Specification: Corrigir seleção da nova estante após o cadastro

**Feature Branch**: `014-corrigir-selecao-nova-estante`

**Created**: 2026-10-30

**Status**: Draft

**Input**: User description: "Ao cadastrar nova estante e fechar o modal, a tela Estante e prateleira não está fazendo o reload direcionando para a nova estante cadastrada. Está mantendo a primeira estante da lista."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ver a nova estante selecionada após cadastrá-la (Priority: P1)

Como pessoa que organiza as localizações do estoque, quero que, ao cadastrar uma nova estante, a tela "Estantes e prateleiras" seja atualizada e já exiba a estante recém-cadastrada como a estante em foco, para começar a trabalhar nela sem procurá-la na lista.

**Why this priority**: É o defeito relatado após a entrega da feature 013: o modal fecha, mas a tela continua na primeira estante da lista, contrariando o comportamento esperado (FR-004 e SC-002 da feature 013).

**Independent Test**: Com ao menos uma estante já cadastrada, cadastrar uma nova estante pelo modal e verificar que, ao fechar, a estante selecionada e a área de prateleiras correspondem à estante recém-criada, e não à primeira da lista.

**Acceptance Scenarios**:

1. **Given** uma ou mais estantes já cadastradas e a primeira da lista selecionada, **When** a pessoa cadastra uma nova estante com dados válidos, **Then** o modal fecha e a tela passa a exibir a nova estante como selecionada, com sua área de prateleiras.
2. **Given** a nova estante recém-cadastrada e selecionada, **When** a pessoa consulta a lista de estantes, **Then** a nova estante está presente na lista e as estantes anteriores continuam disponíveis.
3. **Given** nenhuma estante cadastrada anteriormente, **When** a pessoa cadastra a primeira estante, **Then** ela aparece selecionada após o fechamento do modal.
4. **Given** a nova estante já selecionada, **When** a pessoa altera manualmente a estante selecionada, **Then** a seleção escolhida pela pessoa é respeitada e não é revertida para a nova estante nem para a primeira da lista.

---

### Edge Cases

- Se o cadastro falhar, a seleção atual não muda, o modal permanece aberto e a mensagem de erro é exibida.
- Se a lista atualizada demorar a ser carregada, a pessoa não deve ver a seleção retornar à primeira estante antes de a nova estante aparecer; ao concluir a atualização, a nova estante deve estar selecionada.
- Se a estante selecionada deixar de existir ou ficar inativa por outro motivo, o comportamento existente de escolher uma estante disponível continua valendo.
- Cadastros consecutivos: a cada cadastro bem-sucedido, a estante recém-criada passa a ser a selecionada.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Após um cadastro de estante concluído com sucesso, o sistema MUST exibir a estante recém-cadastrada como a estante selecionada na tela "Estantes e prateleiras".
- **FR-002**: A tela MUST apresentar a área de prateleiras da estante recém-cadastrada, incluindo o estado vazio quando ela ainda não tiver prateleiras.
- **FR-003**: O sistema MUST NOT retornar a seleção para a primeira estante da lista enquanto a lista é atualizada após o cadastro.
- **FR-004**: A nova estante MUST constar na lista de estantes selecionáveis após a atualização, sem remover ou ocultar as estantes anteriores.
- **FR-005**: O sistema MUST respeitar a escolha manual de estante feita pela pessoa depois do cadastro.
- **FR-006**: Se o cadastro falhar, o sistema MUST manter a estante atualmente selecionada inalterada.
- **FR-007**: O comportamento existente do modal (abertura pelo botão, fechamento após sucesso e preservação de dados em caso de falha) MUST permanecer inalterado.
- **FR-008**: Testes automatizados MUST cobrir a seleção da estante recém-criada quando a lista já contém outras estantes, de forma a detectar a regressão relatada.

### Key Entities

- **Estante**: Localização identificada por código e descrição, que pode ser selecionada na tela para consultar e manter suas prateleiras.
- **Estante selecionada**: Estante em foco na tela, cujas prateleiras são exibidas.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% dos cadastros de estante concluídos com sucesso, quando já existem outras estantes, a estante recém-criada é a selecionada ao término da atualização da tela.
- **SC-002**: Em 100% dos cadastros bem-sucedidos, a pessoa vê a área de prateleiras da nova estante sem nenhuma ação manual adicional.
- **SC-003**: Em nenhum momento após o cadastro bem-sucedido a tela exibe a primeira estante da lista como selecionada, a menos que ela seja a própria estante criada.
- **SC-004**: Em 100% das falhas de cadastro, a estante selecionada antes da tentativa permanece selecionada.

## Assumptions

- A correção complementa a feature 013 (modal de cadastro) e não altera permissões, validações ou dados do cadastro de estantes.
- A causa provável é a seleção da nova estante ser sobrescrita antes de a lista atualizada já incluí-la, fazendo a tela voltar à primeira estante; deve ser confirmada na etapa de planejamento.
- Nenhuma mudança de dados persistidos, API ou dependências é necessária.
- A escolha manual posterior da pessoa tem prioridade sobre a seleção automática da estante recém-criada.
