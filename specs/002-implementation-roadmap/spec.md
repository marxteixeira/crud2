# Feature Specification: Fases de implementação do sistema

**Feature Branch**: `002-implementation-roadmap`

**Created**: 2026-10-23

**Status**: Draft

**Input**: User description: "Avalie a atual fase do projeto e o que já foi construído e crie as futuras fases de implementação até finalizar a construção. Ou seja, o foco nessa análise é avaliar todo o contexto e dividir as fases necessárias para que o desenvolvimento fique focado em um contexto delimitado."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Identificar a fase atual do projeto (Priority: P1)

Como pessoa responsável pelo desenvolvimento, quero distinguir o que já foi construído do que ainda falta, para não repetir a fundação nem tratar funcionalidades planejadas como prontas.

**Why this priority**: Uma avaliação correta do ponto de partida é necessária para ordenar o trabalho restante e manter cada implementação focada.

**Independent Test**: Comparar o inventário da fase atual com o repositório, seus testes e os documentos de negócio e arquitetura; cada capacidade existente ou ausente deve ter evidência identificável.

**Acceptance Scenarios**:

1. **Given** a fundação já registrada no projeto, **When** a pessoa responsável consulta o diagnóstico, **Then** encontra separados os recursos existentes e as funcionalidades de negócio ainda não implementadas.
2. **Given** que a base já tem página inicial e verificações de saúde, **When** o diagnóstico é revisado, **Then** esses itens não são apresentados como cadastro de produtos ou fornecedores concluído.

### User Story 2 - Implementar o sistema em fases delimitadas (Priority: P1)

Como pessoa responsável por uma fase de desenvolvimento, quero trabalhar em um conjunto pequeno e ordenado de capacidades relacionadas, para entregar e validar valor sem carregar o contexto de todo o sistema.

**Why this priority**: O pedido central é decompor o trabalho restante em incrementos coerentes que possam ser especificados, desenvolvidos e demonstrados separadamente.

**Independent Test**: Revisar cada fase proposta e confirmar que possui objetivo, capacidades incluídas e excluídas, dependências e condições verificáveis de conclusão.

**Acceptance Scenarios**:

1. **Given** uma fase do roadmap, **When** a equipe inicia seu trabalho, **Then** consegue identificar o resultado esperado e os pré-requisitos sem precisar implementar fases não relacionadas.
2. **Given** duas fases consecutivas, **When** a primeira é concluída, **Then** seus critérios de saída fornecem a base necessária para a seguinte e não duplicam o escopo já entregue.
3. **Given** os requisitos do MVP documentados, **When** são distribuídos pelo roadmap, **Then** cada requisito essencial pertence a uma fase e itens desejáveis ou explicitamente adiados estão identificados como tais.

### User Story 3 - Saber quando a construção está pronta para uso (Priority: P2)

Como responsável pela loja, quero critérios claros de conclusão e entrada em operação, para saber se os cadastros, a migração dos dados e o uso diário estão prontos.

**Why this priority**: A sequência só termina quando os objetivos de negócio e a operação segura forem verificados, e não apenas quando as telas estiverem construídas.

**Independent Test**: Aplicar os critérios finais ao sistema completo e confirmar os resultados de usabilidade, consistência de dados, migração, segurança e recuperação operacional.

**Acceptance Scenarios**:

1. **Given** todas as fases funcionais concluídas, **When** a equipe executa a validação final, **Then** comprova os critérios de aceite do MVP definidos pelo negócio.
2. **Given** a planilha da loja e os dados importados, **When** a reconciliação é concluída, **Then** cada linha está importada ou aparece em um relatório de divergências com motivo.
3. **Given** uma falha que exija recuperação, **When** a equipe segue o procedimento aprovado, **Then** consegue restaurar os dados e verificar a consistência antes de liberar o uso.

### Edge Cases

- A fundação existente inicia, mas o banco de dados ainda não possui o modelo de negócio nem cadastros funcionais.
- Um requisito do documento de negócio não se encaixa sem ambiguidade em uma fase ou depende de outra que ainda não foi concluída.
- A planilha contém nomes duplicados, valores inválidos, localizações inexistentes ou dados incompletos.
- Um produto não possui fornecedor associado; a relação com fornecedor é opcional para produto.
- Um fornecedor tem produtos ativos vinculados quando alguém solicita sua inativação.
- A loja também vende produtos por peso ou comprimento, que estão fora do escopo do MVP definido para esta especificação.
- Uma falha de implantação, credencial ou persistência ocorre durante a validação final.

## Requirements *(mandatory)*

### Diagnóstico da fase atual

O projeto está na transição entre a fundação técnica e a primeira funcionalidade de negócio. A especificação `001-solution-foundation` descreve a fundação; o repositório já contém:

- A solução organizada em camadas e a aplicação web inicial.
- Verificações de saúde e prontidão, configuração, injeção de dependências, registros operacionais e tratamento global de erros na API.
- Um contexto de persistência ainda sem entidades ou modelo de negócio.
- Testes de configuração, saúde, erros, logging, conectividade e dependências entre camadas, além do teste do estado de saúde da web.

A página web atual apenas consulta e apresenta a saúde da API. Ainda não há fluxos de negócio de autenticação, cadastro de produtos, fornecedores ou localizações, associação entre produtos e fornecedores, nem importação da planilha. Portanto, o ponto de partida do roadmap é a fundação existente, e não um MVP de estoque já funcional.

### Functional Requirements

- **FR-001**: O diagnóstico MUST identificar a fundação já presente no repositório: estrutura da solução, execução local, página inicial, verificação de saúde, configuração, tratamento de erros, registros operacionais, preparação de persistência e testes existentes, distinguindo preparação técnica de funcionalidades de negócio concluídas.
- **FR-002**: O diagnóstico MUST identificar como ainda não implementados os fluxos de negócio de login/perfis, cadastros de produtos, fornecedores e localizações, vínculos entre produtos e fornecedores e importação da planilha, salvo evidência de implementação posterior no repositório.
- **FR-003**: O roadmap MUST organizar o trabalho posterior à fundação nas fases ordenadas: identidade e perfis; localização e produtos; fornecedores e vínculos; importação e reconciliação; preparação operacional e entrada em produção.
- **FR-004**: Cada fase MUST declarar objetivo, capacidades incluídas, exclusões relevantes, dependências de fases anteriores e critérios de entrada e saída verificáveis.
- **FR-005**: A fase de identidade e perfis MUST cobrir autenticação, acesso dos perfis Administrador e Operador e as permissões de alteração de preço e gerenciamento de usuários definidas pelo negócio.
- **FR-006**: A fase de localização e produtos MUST cobrir o cadastro de estantes e prateleiras, cadastro/consulta/edição/inativação e reativação de produtos, busca, localização e validações de dados previstas nos requisitos de negócio.
- **FR-007**: A fase de fornecedores MUST cobrir cadastro, consulta, edição, inativação, validação de identificador e associação e consulta dos produtos fornecidos, respeitando as regras de vínculo.
- **FR-008**: A fase de importação MUST cobrir a transferência dos dados da planilha para os cadastros do sistema e a reconciliação explícita de registros importados, rejeitados ou pendentes, sem ocultar divergências.
- **FR-009**: A fase operacional MUST verificar os critérios de segurança, privacidade, desempenho, uso móvel, backup e restauração, documentação de operação e prontidão para entrada em produção definidos nos documentos do projeto.
- **FR-010**: O roadmap MUST mapear todos os requisitos funcionais essenciais do documento de negócio a uma fase e MUST marcar claramente requisitos desejáveis ou adiados, sem apresentá-los como concluídos.
- **FR-011**: O roadmap MUST preservar as regras de negócio e decisões aprovadas nos documentos existentes; qualquer requisito conflitante ou decisão arquitetural nova MUST ser apontada como pendência, não resolvida por suposição dentro das fases.
- **FR-012**: O roadmap MUST manter a construção fora do escopo de vendas, caixa, emissão fiscal, compras automatizadas, financeiro, múltiplas lojas e comércio eletrônico, conforme definido pelo negócio.
- **FR-013**: A fase de localização e produtos MUST limitar a quantidade a unidades inteiras no MVP; produtos vendidos por peso ou comprimento ficam fora do escopo inicial.

### Sequência de fases proposta

#### Fase 1 — Identidade e perfis

- **Objetivo**: permitir acesso autenticado e aplicar as permissões de Administrador e Operador desde o primeiro fluxo de negócio.
- **Inclui**: entrar e sair do sistema; provisionar e gerenciar usuários; atribuir os dois perfis; restringir gerenciamento de usuários e alteração de preço ao Administrador; validar a permissão no serviço e na interface.
- **Não inclui**: cadastros de produtos, fornecedores ou localizações.
- **Dependências**: fundação técnica existente.
- **Critérios de entrada**: execução local e serviços-base disponíveis; regras dos dois perfis confirmadas nos documentos de negócio.
- **Critérios de saída**: ambos os perfis conseguem entrar; ações permitidas e negadas correspondem às regras aprovadas; tentativas não autorizadas não alteram dados.
- **Requisitos cobertos**: RF-30 e regras de perfil do documento de negócio; estabelece a identidade necessária à auditoria das fases seguintes.

#### Fase 2 — Localização e produtos

- **Objetivo**: substituir a planilha para localizar e manter o catálogo básico da loja.
- **Inclui**: manter estantes e prateleiras; cadastrar, consultar, pesquisar, filtrar por estante, prateleira e situação, editar, inativar e reativar produtos; registrar quantidade inteira, preço e localização; mostrar detalhe do produto; aplicar unicidade de nome sem distinção de maiúsculas e acentos, valores não negativos, localização obrigatória para produto ativo e trilha de quem alterou e quando.
- **Não inclui**: cadastro e associação de fornecedores, importação da planilha e exclusão definitiva de produtos.
- **Dependências**: Fase 1 para autorização de ações e identificação do usuário responsável; fundação técnica existente.
- **Critérios de entrada**: critérios de saída da Fase 1 atendidos; quantidade do MVP definida como unidades inteiras.
- **Critérios de saída**: Administrador e Operador conseguem executar apenas as operações autorizadas; produtos ativos sempre têm preço e localização válidos; busca, detalhe e consulta de conteúdo de uma localização refletem os dados salvos; inativação preserva o cadastro e permite reativação.
- **Requisitos cobertos**: RF-01 a RF-05, RF-07, RF-20 a RF-22 e regras RN-01 a RN-04, RN-07, RN-09 e RN-10. O filtro por fornecedor de RF-03 é concluído na Fase 3.

#### Fase 3 — Fornecedores e vínculos

- **Objetivo**: manter os dados de fornecedores e permitir que a loja saiba quem fornece cada produto.
- **Inclui**: cadastrar, consultar, editar e inativar fornecedores; validar CPF/CNPJ; ver os produtos de um fornecedor; associar e desassociar produtos pelas visões de produto e fornecedor; filtrar produtos por fornecedor; aplicar regras de vínculo e auditoria.
- **Não inclui**: importação da planilha e integração automatizada com sistemas de fornecedores.
- **Dependências**: Fases 1 e 2, para identidade, autorização, produtos e localizações.
- **Critérios de entrada**: produtos e localizações podem ser mantidos e consultados; perfis e permissões estão ativos.
- **Critérios de saída**: vínculos de produto-fornecedor são consultáveis pelos dois lados; identificadores inválidos ou duplicados são rejeitados; fornecedor com produtos ativos vinculados não pode ser inativado sem resolver os vínculos; produto pode existir sem fornecedor.
- **Requisitos cobertos**: RF-06, RF-10 a RF-15, filtro de fornecedor de RF-03 e regras RN-05, RN-06, RN-08 e RN-10.

#### Fase 4 — Importação e reconciliação da planilha

- **Objetivo**: migrar os dados atuais sem perder registros nem esconder problemas de qualidade.
- **Inclui**: analisar e preparar a planilha; validar dados contra as regras do sistema; apresentar erros e duplicidades para correção ou decisão; importar produtos, localizações e fornecedores; mostrar resumo e relatório de divergências.
- **Não inclui**: sincronização contínua com a planilha nem integrações de compras.
- **Dependências**: Fases 2 e 3 concluídas e planilha de origem fornecida pela loja.
- **Critérios de entrada**: cadastros e regras de validação finais disponíveis; cópia da planilha aprovada para migração.
- **Critérios de saída**: cada linha da planilha está importada ou consta em relatório com resultado e motivo; totais e amostras foram reconciliados com a loja antes da troca da fonte oficial.
- **Requisitos cobertos**: importação listada nas telas do documento de negócio e critério de aceite de migração completa com divergências identificadas.

#### Fase 5 — Preparação operacional e entrada em produção

- **Objetivo**: demonstrar que a solução pode ser usada diariamente e que a loja consegue operá-la e recuperar seus dados.
- **Inclui**: validar fluxos em computador e celular; confirmar a regra de até três cliques nas tarefas principais; medir busca em catálogo de até 5.000 produtos; validar requisitos de segurança e privacidade, inclusive tratamento de dados pessoais de fornecedores; executar e documentar backup e restauração; preparar instruções operacionais e treinar os funcionários; confirmar que os funcionários usam o sistema no dia a dia em até duas semanas após a entrada em produção; realizar validação final e entrada em produção.
- **Não inclui**: vendas, caixa, emissão fiscal, financeiro, múltiplas lojas, comércio eletrônico ou compras automatizadas.
- **Dependências**: Fases 1 a 4 concluídas e dados reconciliados.
- **Critérios de entrada**: aceite do negócio para os fluxos funcionais e relatório de migração.
- **Critérios de saída**: busca atende ao limite de 2 segundos no catálogo de referência; produto pode ser encontrado em menos de 10 segundos; tarefas principais seguem o limite de três cliques; restauração é demonstrada; equipe consegue usar o sistema; entrada em produção é aprovada.
- **Requisitos cobertos**: requisitos não funcionais e critérios de aceite do MVP, incluindo uso móvel, desempenho, privacidade e backup/restauração.
- **Item desejável separado do MVP**: RF-08 (exclusão definitiva de produto criado por engano) não bloqueia a entrada em produção. Sua inclusão exige decisão explícita do negócio e salvaguardas que preservem o histórico.

### Key Entities *(include if feature involves data)*

- **Fase de implementação**: incremento de trabalho com objetivo, escopo, dependências, critérios de entrada e saída e requisitos associados.
- **Requisito de negócio**: capacidade ou regra aprovada que precisa ser atribuída a uma fase ou explicitamente adiada.
- **Produto**: item controlado pela loja, com quantidade, preço, situação e localização.
- **Fornecedor**: pessoa ou empresa que fornece produtos e possui dados cadastrais próprios.
- **Localização**: combinação de estante e prateleira onde um produto pode ser encontrado.
- **Usuário e perfil**: pessoa autenticada com permissões de Administrador ou Operador.
- **Registro de importação**: linha da planilha e seu resultado de importação ou motivo de divergência.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos requisitos funcionais essenciais documentados estão associados a uma fase futura específica; todo item desejável ou adiado está rotulado e não fica implicitamente no escopo.
- **SC-002**: 100% das fases futuras apresentam objetivo, limites de escopo, dependências e critérios de saída observáveis.
- **SC-003**: O diagnóstico distingue explicitamente as capacidades existentes de todas as funcionalidades de negócio pendentes, sem classificar uma preparação técnica como fluxo de negócio pronto.
- **SC-004**: No sistema concluído, uma pessoa usuária encontra um produto em menos de 10 segundos e realiza tarefas principais em até três cliques, conforme os objetivos do negócio.
- **SC-005**: No sistema concluído, 100% dos produtos ativos possuem preço e localização válidos; 100% das linhas da planilha estão importadas ou registradas em uma lista de divergências.
- **SC-006**: No sistema concluído, buscas do catálogo retornam resultados em até 2 segundos para catálogos de até 5.000 produtos, e a equipe demonstra uma restauração de dados antes da entrada em produção.

## Assumptions

- A especificação de negócio, a especificação de arquitetura, a constituição do projeto, o ADR-0001 e a especificação `001-solution-foundation` são as fontes de verdade; este roadmap não altera suas decisões.
- O repositório atualmente representa a fundação técnica descrita em `001-solution-foundation`; o diagnóstico da fundação não equivale a uma aprovação formal dos gates dessa especificação.
- O MVP é destinado a uma única loja, aos perfis Administrador e Operador e ao uso em navegador de computador ou celular.
- O desenvolvimento de funcionalidades futuras deve continuar usando as fases como unidades independentes de especificação, implementação, teste e validação.
- Produtos vendidos por peso ou comprimento ficam fora do MVP; a quantidade no escopo inicial é inteira, conforme a decisão desta especificação.
- A exclusão definitiva de produtos criados por engano é desejável, não essencial, e só será considerada após validação explícita do escopo e das salvaguardas de histórico.
