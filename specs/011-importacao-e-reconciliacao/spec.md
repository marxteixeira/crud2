# Feature Specification: Importação e reconciliação da planilha

**Feature Branch**: `011-importacao-e-reconciliacao`

**Created**: 2026-10-27

**Status**: Draft

**Input**: User description: "verifique, planeje e implemente a próxima fase."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Validar a planilha antes da importação (Priority: P1)

Como Administrador, quero verificar a planilha de origem antes de gravar seus dados, para corrigir problemas e evitar introduzir cadastros inválidos ou duplicados.

**Why this priority**: A loja precisa confiar nos dados migrados, e os problemas de origem devem ser conhecidos antes de qualquer alteração no catálogo.

**Independent Test**: Enviar uma planilha com dados válidos e inválidos de produtos, localizações e fornecedores; verificar que a prévia identifica cada linha, apresenta seu resultado esperado e explica cada impedimento sem gravar os registros.

**Acceptance Scenarios**:

1. **Given** uma planilha de origem aprovada, **When** o Administrador inicia a validação, **Then** o sistema apresenta a quantidade de linhas reconhecidas e a prévia dos dados que serão considerados.
2. **Given** uma linha com campo obrigatório ausente, valor inválido ou referência inexistente, **When** a validação termina, **Then** a linha é identificada com o motivo e não é apresentada como pronta para importação.
3. **Given** linhas que violam regras de unicidade ou regras de negócio de produto, localização, fornecedor ou vínculo, **When** a validação termina, **Then** cada conflito é apresentado antes de qualquer gravação.
4. **Given** uma planilha que não pode ser lida ou cujas colunas não podem ser identificadas, **When** o Administrador solicita a validação, **Then** o sistema informa o problema e não inicia a importação.

### User Story 2 - Importar os registros válidos (Priority: P1)

Como Administrador, quero confirmar a importação dos registros que passaram pela validação, para transferir os dados aprovados da planilha para o sistema sem perder os registros com problemas.

**Why this priority**: A migração precisa criar os cadastros necessários para substituir a planilha, mantendo explícita qualquer linha que não possa ser aproveitada.

**Independent Test**: Confirmar uma planilha com registros válidos e inválidos; verificar que os válidos são criados uma única vez, que cada linha tem um resultado e que as inválidas não geram registros incompletos.

**Acceptance Scenarios**:

1. **Given** uma prévia concluída com linhas válidas, **When** o Administrador confirma a importação, **Then** o sistema cria os registros permitidos de produtos, estantes, prateleiras, fornecedores e vínculos representados na origem.
2. **Given** uma linha inválida ou conflitante, **When** o Administrador importa as linhas válidas da mesma planilha, **Then** a linha inválida não é gravada e permanece identificada com seu motivo.
3. **Given** produto sem preço ou localização exigidos para estar ativo, **When** o sistema valida a linha, **Then** não cria um produto ativo inválido e informa o dado ausente ou incorreto.
4. **Given** linhas que referenciam uma estante, prateleira, produto ou fornecedor que não pôde ser reconhecido, **When** o sistema importa os demais registros válidos, **Then** não cria vínculos incompletos e informa as linhas dependentes que precisam de correção.
5. **Given** uma importação confirmada, **When** os registros são gravados, **Then** a autoria e o horário da operação são preservados conforme as regras de auditoria existentes.

### User Story 3 - Reconciliar e corrigir divergências (Priority: P1)

Como Administrador, quero consultar o resultado completo da importação e identificar as linhas que precisam de correção, para confirmar que nenhum dado foi perdido ou aceito sem validação.

**Why this priority**: A loja só pode substituir a planilha quando conseguir explicar o destino de cada linha e verificar as divergências restantes.

**Independent Test**: Concluir uma importação com linhas aceitas e rejeitadas; consultar o relatório e confirmar que cada linha da origem aparece exatamente uma vez com seu resultado e, quando aplicável, um motivo compreensível.

**Acceptance Scenarios**:

1. **Given** uma importação concluída, **When** o Administrador consulta seu resumo, **Then** vê as quantidades de linhas importadas, rejeitadas e ainda pendentes, sem totais que omitam linhas da origem.
2. **Given** uma linha rejeitada, **When** o Administrador consulta seus detalhes, **Then** identifica a planilha e a linha de origem, o registro relacionado quando disponível e o motivo da rejeição, sem exposição desnecessária de dados pessoais.
3. **Given** uma planilha corrigida após uma importação parcial, **When** o Administrador reprocessa os registros pendentes, **Then** os registros já importados não são duplicados nem sobrescritos e cada linha atualiza seu resultado de forma rastreável.
4. **Given** que a importação seja interrompida antes da conclusão, **When** o Administrador retorna ao resultado da operação, **Then** consegue distinguir registros concluídos dos pendentes e continuar sem criar duplicidades.
5. **Given** uma conta sem permissão de Administrador, **When** tenta iniciar, confirmar ou reprocessar uma importação, **Then** a operação é recusada sem alteração dos dados.

### Edge Cases

- Uma planilha vazia, ilegível, protegida ou sem linhas de dados deve ser recusada com uma mensagem compreensível e sem criar uma importação apresentada como concluída.
- Cabeçalhos ausentes, repetidos ou não reconhecidos devem ser identificados antes da confirmação; o sistema não deve inferir silenciosamente o significado de colunas ambíguas.
- Linhas repetidas na própria planilha ou conflitantes com registros existentes devem ser reportadas como divergências, sem mesclagem, sobrescrita ou criação duplicada automática.
- Duplicidades devem respeitar as regras existentes de normalização de nomes de produtos e localizações e de unicidade de CPF/CNPJ de fornecedores.
- Uma estante ou prateleira inválida deve tornar explícitas as linhas de produtos que dependem dessa localização.
- Um produto ativo sem preço ou localização válida não pode ser importado como ativo.
- Uma falha durante a gravação de uma linha não pode ser reportada como sucesso; os resultados já persistidos devem continuar consultáveis.
- Falhas de leitura ou gravação devem ser informadas sem descartar silenciosamente o arquivo, os resultados anteriores ou as linhas ainda pendentes.
- Relatórios e mensagens não devem expor CPF, CNPJ, telefone ou e-mail completos quando tais dados não forem necessários para entender a divergência.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST permitir que somente Administradores iniciem, confirmem, consultem e reprocessem uma importação.
- **FR-002**: O sistema MUST aceitar a planilha de origem aprovada pela loja e identificar as linhas e colunas necessárias para produtos, estantes, prateleiras, fornecedores e vínculos de fornecimento quando presentes.
- **FR-003**: O sistema MUST validar os dados de origem contra as regras de negócio vigentes antes de gravar qualquer registro e apresentar uma prévia dos resultados.
- **FR-004**: O sistema MUST informar, por linha inválida ou conflitante, o campo ou referência a corrigir e um motivo compreensível, sem apresentar a linha como pronta para importação.
- **FR-005**: O sistema MUST exigir confirmação explícita do Administrador antes de gravar os registros validados.
- **FR-006**: O sistema MUST importar apenas os registros válidos e manter os registros inválidos ou conflitantes identificados para correção e reconciliação.
- **FR-007**: O sistema MUST aplicar as regras existentes para nomes únicos, CPF/CNPJ válido e único, quantidades inteiras não negativas, valores monetários não negativos, localizações válidas, situações e vínculos entre produtos e fornecedores.
- **FR-008**: O sistema MUST preservar as relações entre produtos, estantes, prateleiras e fornecedores quando essas relações estiverem representadas na origem e os registros referenciados forem válidos.
- **FR-009**: O sistema MUST impedir que uma importação crie registros duplicados ou sobrescreva cadastros existentes sem uma ação explícita de correção fora desta fase.
- **FR-010**: O sistema MUST manter um resultado rastreável por linha de origem, identificando-a como importada, rejeitada ou pendente, e atualizar o resultado quando uma linha pendente for reprocessada.
- **FR-011**: O sistema MUST disponibilizar resumo e relatório que contabilizem todas as linhas da origem e apresentem motivo para cada rejeição, sem expor dados pessoais além do necessário.
- **FR-012**: O sistema MUST permitir reprocessar linhas pendentes ou corrigidas sem duplicar nem sobrescrever registros já importados.
- **FR-013**: O sistema MUST preservar o resultado das linhas concluídas e identificar linhas ainda não concluídas quando uma importação for interrompida.
- **FR-014**: O sistema MUST registrar quem confirmou a importação e quando, e preservar a auditoria exigida para os registros criados.
- **FR-015**: O sistema MUST informar falhas de leitura, validação ou gravação sem marcar como importadas as linhas não persistidas.

### Key Entities *(include if feature involves data)*

- **Importação**: operação identificada, iniciada por um Administrador, associada a uma planilha de origem e a um resultado geral consultável.
- **Linha de importação**: linha da origem com dados reconhecidos, estado (importada, rejeitada ou pendente), referência ao registro criado quando aplicável e motivo de divergência quando necessário.
- **Produto, estante e prateleira**: cadastros da loja a serem criados a partir da origem, sujeitos às regras e relações existentes.
- **Fornecedor**: pessoa ou empresa fornecedora importada conforme validação e unicidade de CPF/CNPJ.
- **Vínculo de fornecimento**: associação entre produto e fornecedor importada somente quando estiver representada na origem e ambos os registros puderem ser identificados.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das linhas de dados reconhecidas terminam identificadas como importadas, rejeitadas ou pendentes; nenhuma linha fica sem resultado no resumo da operação.
- **SC-002**: 100% das linhas rejeitadas apresentam um motivo específico suficiente para que o Administrador identifique o dado ou a referência a corrigir.
- **SC-003**: 0 registros duplicados ou sobrescritos são criados por confirmação repetida ou reprocessamento da mesma importação.
- **SC-004**: 100% dos cadastros e vínculos importados respeitam as regras de validação e consistência já aplicadas nos fluxos normais do sistema.
- **SC-005**: Pelo menos 95% dos Administradores participantes conseguem identificar, a partir do relatório, o resultado e a próxima ação necessária para uma linha divergente sem ajuda.
- **SC-006**: Antes da substituição da planilha como fonte oficial, a loja consegue reconciliar os totais da origem com o resumo e revisar todas as divergências ainda pendentes.

## Assumptions

- Esta é uma migração pontual da planilha existente, não uma sincronização contínua.
- A loja fornecerá uma cópia aprovada da planilha e confirmará que ela representa a fonte de dados a migrar.
- Os dados a importar incluem produtos, estantes, prateleiras e fornecedores; relações entre produto e fornecedor serão importadas somente se estiverem representadas de forma identificável na origem.
- A importação cria cadastros novos; correção, mesclagem e atualização automática de registros já existentes estão fora do escopo.
- As regras de negócio e auditoria das fases de localização e produtos e de fornecedores e vínculos são a referência para validar os dados importados.
- O processamento pode ser parcial: linhas válidas podem ser importadas enquanto as inválidas permanecem pendentes de correção, desde que cada resultado seja explicitamente reconciliável.
- A fase depende da conclusão e validação das fases 2 e 3 do roadmap, incluindo os testes e o guia de execução ainda pendentes em `010-fornecedores-e-vinculos`.
- A entrada em produção, treinamento, backup e restauração pertencem à fase operacional seguinte e não são objetivos desta especificação.
