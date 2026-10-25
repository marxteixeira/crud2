# Feature Specification: Localização e produtos

**Feature Branch**: `007-localizacao-produtos`

**Created**: 2026-10-25

**Status**: Draft

**Input**: User description: "A estrutura e fase de construção de gestão de usuário foram concluídas. Avance para a próxima fase mapeada, localização e produtos."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Organizar estantes e prateleiras (Priority: P1)

Como Administrador ou Operador, quero manter as estantes e suas prateleiras para informar a localização dos produtos por seleção, sem depender de texto livre.

**Why this priority**: Uma localização padronizada é necessária para cadastrar produtos ativos e encontrá-los fisicamente.

**Independent Test**: Criar uma estante com prateleiras, consultar sua estrutura e confirmar que as prateleiras disponíveis pertencem à estante escolhida.

**Acceptance Scenarios**:

1. **Given** uma pessoa autenticada, **When** cadastra uma estante e suas prateleiras, **Then** a estrutura fica disponível para seleção nos produtos.
2. **Given** prateleiras de estantes diferentes, **When** a pessoa seleciona uma estante para localizar um produto, **Then** somente prateleiras daquela estante podem ser escolhidas.
3. **Given** uma estante ou prateleira associada a produtos ativos, **When** a pessoa tenta inativá-la, **Then** a operação é recusada até que os produtos sejam realocados.

---

### User Story 2 - Manter o catálogo de produtos (Priority: P1)

Como Administrador ou Operador, quero cadastrar, consultar, editar, inativar e reativar produtos com quantidade e localização, para manter uma fonte confiável para a operação da loja.

**Why this priority**: O catálogo e os dados básicos do produto substituem a parte central da planilha atual.

**Independent Test**: Cadastrar um produto válido, encontrá-lo pela busca, editar seus dados permitidos, inativá-lo e reativá-lo sem perder seu histórico.

**Acceptance Scenarios**:

1. **Given** uma pessoa autenticada e uma localização existente, **When** um Administrador cadastra um produto com nome, quantidade, preço e localização válidos, **Then** o produto ativo aparece no catálogo e no conteúdo da localização.
2. **Given** um nome já usado por outro produto, **When** a pessoa tenta cadastrar ou renomear um produto com esse nome, **Then** a operação é recusada sem criar duplicidade, desconsiderando diferenças de maiúsculas, minúsculas e acentos.
3. **Given** um produto ativo, **When** a pessoa altera seus dados permitidos, **Then** os dados atualizados são apresentados nas consultas seguintes.
4. **Given** um produto inativado, **When** uma pessoa autorizada o reativa com dados válidos e localização existente, **Then** ele volta às consultas de produtos ativos sem perder o histórico.
5. **Given** um Operador autenticado, **When** tenta definir ou alterar o preço, **Then** a ação é recusada e o preço salvo permanece inalterado.
6. **Given** um Operador autenticado que cadastra um produto sem preço, **When** salva os dados válidos restantes, **Then** o produto fica inativo e não aparece como produto disponível no catálogo ativo.
7. **Given** um produto inativo cadastrado sem preço, **When** um Administrador informa um preço válido e o ativa, **Then** o produto passa a aparecer no catálogo ativo.
8. **Given** uma pessoa autorizada que altera um produto, **When** a alteração é concluída, **Then** o histórico identifica quem realizou a ação e quando ela ocorreu.

---

### User Story 3 - Encontrar produtos e consultar uma localização (Priority: P1)

Como Operador ou Administrador, quero localizar produtos pela busca e pelos filtros e consultar o conteúdo de uma estante e prateleira, para encontrar rapidamente um item no estoque.

**Why this priority**: A localização rápida reduz o tempo de procura durante o atendimento e a reposição.

**Independent Test**: Com produtos em diferentes localizações e situações, pesquisar por nome, filtrar por estante, prateleira e situação e consultar os produtos de uma localização específica.

**Acceptance Scenarios**:

1. **Given** produtos ativos e inativos, **When** a pessoa busca por nome ou filtra por estante, prateleira ou situação, **Then** vê somente os produtos correspondentes aos critérios escolhidos.
2. **Given** uma estante e uma prateleira existentes, **When** a pessoa consulta o conteúdo dessa localização, **Then** vê os produtos nela localizados e consegue abrir seus detalhes.
3. **Given** um produto encontrado, **When** a pessoa consulta seu detalhe, **Then** vê nome, quantidade, preço, situação e localização atual.

### Edge Cases

- Nome vazio, nome duplicado após normalização de maiúsculas/minúsculas ou acentos, quantidade negativa ou fracionária e preço negativo são recusados com mensagem compreensível.
- Um produto ativo não pode ser salvo sem preço ou sem uma prateleira existente pertencente à estante selecionada.
- Uma prateleira ou estante que ainda tenha produtos ativos não pode ser inativada; os produtos precisam ser realocados primeiro.
- Uma prateleira é removida ou inativada enquanto um formulário de produto está aberto; ao salvar, o sistema deve recusar a localização inválida e preservar os dados anteriores.
- Uma busca sem resultados informa que nenhum produto corresponde aos critérios; filtros combinados não devem incluir resultados que violem qualquer filtro selecionado.
- Falha ao salvar ou consultar dados deve ser informada sem apresentar sucesso nem descartar silenciosamente alterações.
- Tentativa do Operador de alterar preço por qualquer fluxo deve ser recusada sem alterar o valor registrado.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST permitir que Administradores e Operadores autenticados consultem e mantenham estantes e suas prateleiras, mantendo cada prateleira associada a uma única estante.
- **FR-002**: O sistema MUST permitir selecionar uma estante e, em seguida, somente uma prateleira pertencente a ela ao definir a localização de um produto.
- **FR-003**: O sistema MUST permitir cadastrar, consultar, editar, inativar e reativar produtos com nome, quantidade, preço, localização e situação. Operadores MUST poder cadastrar produtos inativos sem preço; somente um Administrador pode informar o preço obrigatório e ativar esses produtos.
- **FR-004**: O sistema MUST exigir nome, quantidade inteira maior ou igual a zero, preço em reais maior ou igual a zero com duas casas decimais e localização válida para todo produto ativo.
- **FR-005**: O sistema MUST impedir nomes de produto duplicados sem distinguir maiúsculas, minúsculas ou acentos.
- **FR-006**: O sistema MUST permitir somente a Administradores definir ou alterar preços e MUST recusar tentativas de Operadores de definir ou alterar preços, preservando o valor salvo.
- **FR-007**: O sistema MUST oferecer busca de produtos por nome e filtros por estante, prateleira e situação, permitindo combinar filtros.
- **FR-008**: O sistema MUST apresentar o detalhe de um produto com sua quantidade, preço, situação e localização, e MUST permitir consultar os produtos de uma estante e prateleira.
- **FR-009**: O sistema MUST preservar o cadastro e seu histórico ao inativar um produto e MUST permitir sua reativação quando seus dados e localização forem válidos.
- **FR-010**: O sistema MUST impedir a inativação de uma estante ou prateleira enquanto houver produtos ativos localizados nela.
- **FR-011**: O sistema MUST registrar quem realizou e quando realizou cada alteração relevante em produtos e localizações.
- **FR-012**: O sistema MUST apresentar mensagens claras para dados inválidos, resultados vazios e falhas de consulta ou gravação, sem indicar sucesso quando a operação não for concluída.
- **FR-013**: O sistema MUST restringir as operações a pessoas autenticadas e respeitar os perfis Administrador e Operador concluídos na fase anterior.

### Key Entities *(include if feature involves data)*

- **Produto**: item do catálogo com nome único, quantidade inteira, preço, situação e localização.
- **Estante**: estrutura de armazenamento identificada por código ou nome, que contém prateleiras.
- **Prateleira**: nível de armazenamento pertencente a uma única estante e selecionável como parte da localização de um produto.
- **Localização**: combinação de estante e prateleira válida para um produto ativo.
- **Registro de alteração**: informação de quem alterou um produto ou localização e quando a alteração ocorreu.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos produtos ativos salvos têm nome único, quantidade inteira não negativa, preço não negativo e localização válida.
- **SC-002**: Pessoas participantes encontram um produto e identificam sua localização física em menos de 10 segundos em pelo menos 90% das tentativas.
- **SC-003**: Em um catálogo de até 5.000 produtos, pelo menos 95% das buscas apresentam resultados em até 2 segundos.
- **SC-004**: Pelo menos 90% das pessoas participantes concluem sem ajuda as tarefas de cadastrar um produto e consultar o conteúdo de uma localização.
- **SC-005**: 100% das tentativas de Operador de alterar preços são recusadas sem mudança no preço registrado.
- **SC-006**: As tarefas principais de cadastrar ou localizar um produto exigem no máximo três cliques a partir da área de produtos.

## Assumptions

- A fase de identidade e gestão de usuários foi concluída; autenticação e perfis Administrador e Operador já estão disponíveis e são reutilizados.
- O MVP controla produtos vendidos em unidades inteiras; produtos vendidos por peso ou comprimento permanecem fora do escopo.
- Produto pode existir sem fornecedor associado nesta fase; cadastro de fornecedores e vínculos ficam para a fase seguinte.
- Importação da planilha e exclusão definitiva de produtos não fazem parte desta fase.
- Nomes de produtos permanecem únicos mesmo quando o produto está inativo, para evitar duplicidade ao reativar ou consultar o histórico.
- Estantes e prateleiras podem ser mantidas por ambos os perfis autenticados; sua inativação depende de não haver produtos ativos associados.
- O preço é obrigatório para um produto ativo. Operadores podem cadastrar produtos sem preço, que permanecem inativos até um Administrador informar o preço e ativá-los.
