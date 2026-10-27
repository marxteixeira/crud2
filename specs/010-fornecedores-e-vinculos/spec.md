# Feature Specification: Fornecedores e vínculos

**Feature Branch**: `010-fornecedores-e-vinculos`

**Created**: 2026-10-26

**Status**: Draft

**Input**: User description: "verifique a próxima fase para iniciarmos o desenvolvimento."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Manter fornecedores (Priority: P1)

Como Administrador, quero cadastrar, consultar, pesquisar, editar e inativar fornecedores, para manter informações confiáveis de quem fornece os produtos da loja.

**Why this priority**: Os vínculos entre produtos e fornecedores dependem de cadastros identificáveis e atualizados.

**Independent Test**: Cadastrar um fornecedor com identificador válido, localizá-lo pela busca, editar seus dados e inativá-lo quando não houver produtos ativos vinculados.

**Acceptance Scenarios**:

1. **Given** um Administrador autenticado e dados válidos de fornecedor, **When** salva um novo cadastro, **Then** o fornecedor aparece na consulta com os dados informados.
2. **Given** um CPF ou CNPJ inválido ou já cadastrado, **When** o Administrador tenta salvar o fornecedor, **Then** o sistema recusa a operação, informa o campo a corrigir e preserva os cadastros existentes.
3. **Given** um fornecedor cadastrado, **When** o Administrador pesquisa pelo nome ou edita seus dados, **Then** a consulta encontra o fornecedor e as alterações aparecem nas consultas seguintes.
4. **Given** um fornecedor sem produtos ativos vinculados, **When** o Administrador o inativa, **Then** o cadastro deixa de estar disponível para novos vínculos e continua acessível como histórico.
5. **Given** um fornecedor com produto ativo vinculado, **When** o Administrador tenta inativá-lo, **Then** a ação é recusada com orientação para resolver os vínculos primeiro.
6. **Given** um Administrador ou pessoa autorizada que altera um fornecedor, **When** a alteração é concluída, **Then** o histórico identifica quem realizou a ação e quando ela ocorreu.

---

### User Story 2 - Consultar e manter vínculos de fornecimento (Priority: P1)

Como pessoa usuária da loja, quero consultar os fornecedores de um produto e os produtos atendidos por um fornecedor, para saber a origem dos itens e facilitar a reposição.

**Why this priority**: A relação entre os dois cadastros é o valor principal desta fase e responde diretamente à necessidade de saber quem fornece cada produto.

**Independent Test**: Associar um produto a um fornecedor, confirmar que a relação aparece nas consultas dos dois lados, removê-la e verificar que deixa de aparecer sem apagar os cadastros.

**Acceptance Scenarios**:

1. **Given** um produto e um fornecedor ativos, **When** uma pessoa autorizada associa os cadastros, **Then** o vínculo aparece no detalhe do produto e na lista de produtos do fornecedor.
2. **Given** um produto vinculado a um fornecedor, **When** uma pessoa autorizada remove o vínculo, **Then** a associação deixa de aparecer nos dois lados e os cadastros permanecem disponíveis.
3. **Given** um produto sem fornecedor, **When** a pessoa consulta ou mantém o produto, **Then** o sistema permite que ele continue sem vínculo.
4. **Given** um produto com vários fornecedores ou um fornecedor ligado a vários produtos, **When** qualquer lado é consultado, **Then** todos e somente os vínculos existentes são apresentados.
5. **Given** uma conta Administrador ou Operador, **When** consulta produtos e fornecedores, **Then** visualiza os dados de fornecimento permitidos para seu perfil; somente ações autorizadas podem alterar cadastros ou vínculos.
6. **Given** um fornecedor inativo com vínculos históricos, **When** uma pessoa consulta o fornecedor ou tenta criar novo vínculo, **Then** os vínculos existentes continuam consultáveis e a nova associação é recusada.
7. **Given** uma alteração autorizada em vínculo de fornecimento, **When** a alteração é concluída, **Then** o histórico identifica quem realizou a ação e quando ela ocorreu.

---

### User Story 3 - Encontrar produtos por fornecedor (Priority: P2)

Como Administrador ou Operador, quero filtrar o catálogo pelo fornecedor, para localizar rapidamente os produtos que ele fornece.

**Why this priority**: O filtro completa a consulta do catálogo existente e permite usar os vínculos em tarefas diárias sem percorrer manualmente toda a lista.

**Independent Test**: Consultar o catálogo com um fornecedor selecionado, combinar o filtro com busca, localização e situação e confirmar que somente produtos correspondentes são exibidos.

**Acceptance Scenarios**:

1. **Given** produtos vinculados a diferentes fornecedores, **When** a pessoa filtra o catálogo por um fornecedor, **Then** vê apenas os produtos associados a ele.
2. **Given** filtros de fornecedor, nome, localização ou situação combinados, **When** a pessoa consulta o catálogo, **Then** cada resultado corresponde a todos os critérios selecionados.
3. **Given** nenhum produto corresponde aos filtros escolhidos, **When** a consulta termina, **Then** o sistema informa que não há resultados sem indicar falha.

### Edge Cases

- CPF e CNPJ devem ser validados pelo tipo e formato informado; diferenças de pontuação não devem permitir cadastrar novamente o mesmo identificador.
- Fornecedores com nomes iguais podem existir se seus identificadores forem distintos; a busca deve permitir distingui-los pelos dados de identificação disponíveis.
- Uma tentativa simultânea de cadastrar o mesmo CPF ou CNPJ deve resultar em apenas um cadastro válido e uma mensagem de duplicidade para a outra tentativa.
- Um produto inativo pode manter seus vínculos para preservar o histórico, mas não deve impedir a inativação de um fornecedor por si só.
- A inativação de fornecedor com produtos ativos vinculados deve continuar bloqueada até que os produtos sejam desvinculados ou deixem de estar ativos.
- Fornecedor inativo não pode receber novos vínculos; seus vínculos históricos continuam consultáveis.
- Erros ao salvar, consultar ou alterar vínculos devem ser apresentados claramente, sem confirmar uma operação não concluída nem descartar silenciosamente alterações.
- Uma pessoa sem permissão não pode alterar dados do fornecedor nem criar ou remover vínculos por acesso direto ou pela interface.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST permitir que Administradores cadastrem fornecedores com nome ou razão social e CPF ou CNPJ válido, além de telefone, e-mail, pessoa de contato e observações quando informados.
- **FR-002**: O sistema MUST garantir que cada CPF ou CNPJ válido identifique no máximo um fornecedor, considerando equivalentes as diferentes formas de pontuação do mesmo identificador.
- **FR-003**: O sistema MUST permitir que pessoas autorizadas consultem fornecedores e que Administradores pesquisem fornecedores pelo nome, editem seus dados e consultem seus detalhes.
- **FR-004**: O sistema MUST permitir que Administradores inativem fornecedores sem apagar seus registros e MUST impedir a inativação enquanto houver produtos ativos vinculados.
- **FR-005**: O sistema MUST permitir que pessoas autorizadas associem e desassociem produtos e fornecedores a partir das visões de produto e de fornecedor.
- **FR-006**: O sistema MUST permitir que um produto não tenha fornecedor ou tenha vários fornecedores e que um fornecedor esteja associado a vários produtos.
- **FR-007**: O sistema MUST apresentar no detalhe do produto os fornecedores associados e no detalhe do fornecedor os produtos associados.
- **FR-008**: O sistema MUST impedir novos vínculos com fornecedores inativos e MUST preservar os vínculos existentes para consulta histórica.
- **FR-009**: O sistema MUST permitir filtrar o catálogo de produtos por fornecedor e combinar esse filtro com os filtros já disponíveis de nome, localização e situação.
- **FR-010**: O sistema MUST aplicar os perfis existentes às consultas e alterações de fornecedores e vínculos, recusando ações não autorizadas sem modificar os dados.
- **FR-011**: O sistema MUST registrar quem realizou e quando realizou cada alteração relevante em fornecedores e vínculos.
- **FR-012**: O sistema MUST apresentar mensagens compreensíveis para dados inválidos, identificadores duplicados, resultados vazios e falhas de consulta ou gravação.

### Key Entities *(include if feature involves data)*

- **Fornecedor**: pessoa ou empresa fornecedora, identificada por nome ou razão social e CPF ou CNPJ, com dados de contato, situação e histórico de alterações.
- **Produto**: item existente no catálogo, que pode ser associado a nenhum, um ou vários fornecedores.
- **Vínculo de fornecimento**: associação consultável entre um produto e um fornecedor, mantida independentemente das consultas feitas a partir de cada cadastro.
- **Usuário**: pessoa autenticada com perfil Administrador ou Operador, cujas permissões determinam as ações disponíveis.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos fornecedores salvos possuem nome ou razão social e um CPF ou CNPJ válido e único.
- **SC-002**: Pelo menos 95% das pessoas participantes conseguem cadastrar ou localizar um fornecedor e consultar seus produtos sem ajuda na primeira tentativa.
- **SC-003**: 100% dos vínculos criados ou removidos aparecem de forma consistente nas consultas do produto e do fornecedor.
- **SC-004**: 100% das tentativas de inativar fornecedor com produto ativo vinculado são bloqueadas com orientação clara, sem alterar o cadastro.
- **SC-005**: Pessoas usuárias encontram produtos de um fornecedor usando o filtro do catálogo em até 30 segundos.
- **SC-006**: 100% das tentativas de alteração não autorizadas são recusadas sem mudança nos dados ou vínculos.

## Assumptions

- As fases de identidade e perfis e de localização e produtos estão concluídas; autenticação, autorização, catálogo, localização e auditoria existentes serão reutilizados.
- Administradores podem cadastrar, editar e inativar fornecedores e gerenciar vínculos. Administradores e Operadores podem consultar produtos e informações de fornecimento necessárias ao catálogo; a alteração de vínculos segue as permissões de edição de produto existentes.
- Nome ou razão social e CPF/CNPJ são obrigatórios. Telefone, e-mail, contato e observações são opcionais, pois os requisitos de negócio não declaram esses campos obrigatórios.
- Um produto pode permanecer sem fornecedor, e associações de produtos inativos são preservadas para consulta histórica.
- Fornecedores inativos permanecem consultáveis, mas não podem receber novos vínculos.
- A importação da planilha, a integração automatizada com fornecedores e a exclusão definitiva de cadastros não fazem parte desta fase.
