# Research: Fechamento da fase de identidade e perfis

## Decision 1: Usar o roadmap e as tarefas existentes como critérios de referência

**Decision**: Usar `specs/002-implementation-roadmap/spec.md` como fonte da sequência e dos critérios de entrada/saída; usar as tarefas e quickstarts das fases 003 e 004 como inventário de validações exigidas.

**Rationale**: O roadmap estabelece localização e produtos como próxima fase de negócio. As tarefas 003 e 004 enumeram validações concretas de identidade, autorização, persistência e fechamento, evitando criar novos critérios incompatíveis.

**Alternatives considered**: Inferir conclusão apenas da presença dos componentes; rejeitado porque não demonstra comportamento correto. Criar um novo conjunto independente de requisitos de negócio; rejeitado por duplicar o roadmap existente.

## Decision 2: Tratar falha de teste como bloqueio até haver diagnóstico atual

**Decision**: Build aprovado não substitui testes aprovados. Falhas de teste devem ser repetidas em ambiente controlado, classificadas como defeito ou problema ambiental e resolvidas antes de marcar a validação como aprovada.

**Rationale**: A constituição proíbe integração com testes falhando, e o cenário atual da API teve 26 falhas em 63 testes. As causas reportadas podem incluir configuração e fixture, mas ainda requerem correção ou comprovação atual.

**Alternatives considered**: Considerar os testes como aprovados por falhas presumidamente ambientais; rejeitado porque contradiz o resultado observado e não é verificável. Ignorar a suíte API por build aprovado; rejeitado porque build não verifica comportamento de runtime.

## Decision 3: Exigir evidência própria para validações não automatizadas

**Decision**: Marcar os cenários manuais, o teste de desempenho e a validação de usabilidade somente após execução nos ambientes e com os participantes definidos; guardar apenas resultados agregados e sem identificação pessoal.

**Rationale**: T036, T039 e T040 continuam pendentes. A existência de testes automatizados não comprova operação com projeto isolado, desempenho medido nem usabilidade com participantes.

**Alternatives considered**: Inferir esses resultados de testes automatizados; rejeitado porque eles medem condições diferentes. Usar dados pessoais para rastrear participantes; rejeitado por não ser necessário para o critério agregado e por ampliar exposição de dados.

## Decision 4: Não criar contrato externo nem persistência para o relatório

**Decision**: Manter o resultado como registro documental da feature, referenciando tarefas, comandos, resultados agregados e decisão. Não criar API, tabela ou ferramenta de relatórios.

**Rationale**: A feature é uma avaliação pontual e interna do projeto; não há consumidor externo nem requisito de armazenamento operacional.

**Alternatives considered**: Criar endpoint ou tabela para estado de prontidão; rejeitado como complexidade desnecessária. Criar contrato de endpoint sem interface pública; rejeitado por não haver sistema integrador.
