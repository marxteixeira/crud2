# Feature Specification: Corrigir carregamento dos campos da estante selecionada

**Feature Branch**: `015-corrigir-campos-estante-selecionada`

**Created**: 2026-10-30

**Status**: Draft

**Input**: User description: "na tela de localizações, ao selecionar uma estante, os campos: código e descrição da estante não estão sendo carregados com os valores respectivos da estante selecionada."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ver código e descrição da estante selecionada (Priority: P1)

Como pessoa que organiza as localizações do estoque, quero que, ao selecionar uma estante na tela "Estantes e prateleiras", os campos de código e descrição da estante sejam preenchidos com os valores da estante escolhida, para consultar e editar os dados corretos.

**Why this priority**: Sem os valores corretos, a pessoa não consegue conferir nem alterar a estante selecionada, e corre o risco de salvar dados errados.

**Independent Test**: Com ao menos duas estantes cadastradas com códigos e descrições distintos, selecionar cada uma e verificar que os campos exibem exatamente o código e a descrição da estante escolhida.

**Acceptance Scenarios**:

1. **Given** várias estantes cadastradas, **When** a pessoa seleciona uma estante, **Then** os campos código e descrição exibem os valores dessa estante.
2. **Given** uma estante selecionada, **When** a pessoa seleciona outra estante, **Then** os campos são atualizados para os valores da nova estante, sem restos da anterior.
3. **Given** a tela recém-aberta com estantes existentes, **When** a estante inicial é exibida como selecionada, **Then** os campos já mostram os valores dela.
4. **Given** uma estante recém-cadastrada e selecionada, **When** a tela é atualizada, **Then** os campos mostram o código e a descrição da nova estante.
5. **Given** a pessoa alterou os campos sem salvar, **When** seleciona outra estante, **Then** as alterações não salvas são descartadas e os campos mostram a estante escolhida.

---

### Edge Cases

- Estante com descrição vazia ou ausente: o campo de descrição aparece vazio, sem exibir valores de outra estante.
- Seleção rápida e consecutiva de estantes: os campos refletem sempre a última estante selecionada.
- Estante inativa ou removida por outro motivo: aplica-se o comportamento existente de escolher uma estante disponível, e os campos acompanham essa estante.
- Falha ao salvar alterações: os campos mantêm o que a pessoa digitou, junto da mensagem de erro existente.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Ao selecionar uma estante, o sistema MUST preencher o campo código com o código da estante selecionada.
- **FR-002**: Ao selecionar uma estante, o sistema MUST preencher o campo descrição com a descrição da estante selecionada.
- **FR-003**: Ao trocar a estante selecionada, o sistema MUST substituir os valores dos campos, sem manter valores da estante anterior.
- **FR-004**: O sistema MUST preencher os campos também quando a seleção ocorre automaticamente (abertura da tela e após cadastro de nova estante).
- **FR-005**: O sistema MUST descartar alterações não salvas dos campos ao trocar de estante.
- **FR-006**: O sistema MUST exibir o campo vazio quando a estante não tiver descrição.
- **FR-007**: O comportamento existente de cadastro, edição, ativação/inativação e permissões de estantes MUST permanecer inalterado.
- **FR-008**: Testes automatizados MUST cobrir o preenchimento dos campos ao selecionar estantes distintas, de modo a detectar a regressão relatada.

### Key Entities

- **Estante**: Localização identificada por código e descrição, que pode ser selecionada para consultar e manter suas prateleiras.
- **Estante selecionada**: Estante em foco na tela, cujos código e descrição aparecem nos campos de dados.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% das seleções de estante, os campos código e descrição exibem os valores da estante escolhida.
- **SC-002**: Em 100% das trocas de seleção, nenhum valor da estante anterior permanece nos campos.
- **SC-003**: A pessoa confere os dados de qualquer estante em uma única ação de seleção, sem recarregar a tela nem reabrir o formulário.
- **SC-004**: Nenhuma regressão nos fluxos existentes de cadastro e edição de estantes.

## Assumptions

- A correção complementa as features 013 e 014 e não altera dados persistidos, permissões, validações nem dependências.
- A tela possui campos de código e descrição editáveis (ou de leitura) associados à estante selecionada; a causa será confirmada no planejamento.
- Alterações não salvas são descartadas ao trocar de estante, padrão para formulários de edição.
