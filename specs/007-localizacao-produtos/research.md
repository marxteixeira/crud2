# Pesquisa: Localização e produtos

**Feature**: [spec.md](spec.md)

## 1. Reutilização da arquitetura e autorização

**Decision**: Implementar a funcionalidade nos projetos existentes: regras e entidades no Domain, casos de uso/DTOs na Application, persistência EF Core na Infrastructure, endpoints autenticados na API e interface no web. Aplicar as políticas de Administrador/Operador existentes também na API; a interface não constitui uma fronteira de autorização.

**Rationale**: A constituição exige Clean Architecture, autorização server-side e preservação do monorepo. A fase de identidade já implementa autenticação, perfil atual da conta e política de Administrador, que devem ser reutilizados em vez de duplicados.

**Alternatives considered**:
- Criar projeto ou camada específica de catálogo: rejeitado por não haver necessidade que justifique estrutura adicional.
- Restringir preço somente pela interface: rejeitado porque chamadas diretas à API poderiam alterar o preço.
- Usar uma fonte de perfil enviada pelo navegador: rejeitado por não ser autoritativa.

## 2. Unicidade de nomes sem distinção de caixa ou acento

**Decision**: Manter o nome informado para exibição e persistir uma chave normalizada de comparação, calculada de forma determinística ao salvar. Normalizar Unicode, remover marcas diacríticas, aplicar caixa invariável e impor unicidade da chave no banco. Aplicar a mesma regra de comparação a códigos de estantes e prateleiras dentro de seus respectivos escopos.

**Rationale**: A regra de negócio requer que, por exemplo, nomes que diferem apenas em caixa ou acentos sejam tratados como duplicados. Um índice único no banco evita duplicidade também em gravações concorrentes, enquanto a chave separada preserva a forma digitada para apresentação.

**Alternatives considered**:
- Validar duplicidade apenas em uma consulta da aplicação: rejeitado porque duas gravações simultâneas poderiam passar pela validação.
- Depender exclusivamente de collation ou extensão de banco: rejeitado por tornar a regra dependente de configuração específica do ambiente e dificultar sua cobertura unitária.
- Exibir somente a forma normalizada: rejeitado porque altera o nome apresentado em relação ao informado pelo usuário.

## 3. Produtos sem preço cadastrados por Operadores

**Decision**: O preço é anulável somente enquanto o produto estiver inativo. Um Operador pode criar produto inativo sem preço e não pode enviar nem alterar o preço. Um Administrador pode definir o preço e ativar o produto. Qualquer tentativa de ativar produto sem preço válido ou localização válida é recusada; produto ativo sempre tem preço e localização.

**Rationale**: Isso implementa a decisão explícita da especificação sem permitir que Operadores alterem preço e mantém a regra de que todo produto ativo tem preço e localização.

**Alternatives considered**:
- Permitir ao Operador informar o preço na criação: rejeitado por contradizer a restrição de alteração de preço.
- Exigir que Operadores solicitem que um Administrador crie todos os produtos: rejeitado por descartar o fluxo de cadastro pelo Operador previsto na spec.
- Armazenar valor sentinela como preço desconhecido: rejeitado por confundir um preço real de zero com ausência de preço.

## 4. Persistência, auditoria e leitura de catálogo

**Decision**: Usar o `SistemaEstoqueDbContext` e PostgreSQL existentes, com chaves estrangeiras explícitas, índices nos campos de busca/filtro, consultas paginadas e migração aplicada explicitamente. Armazenar autor como identificador da conta autenticada e horários UTC nos registros de produto e localização. Impedir inativação de localização que ainda contenha produto ativo.

**Rationale**: O projeto já usa EF Core/Npgsql, migrations explícitas, PostgreSQL e autoria/horário nas entidades de identidade. Índices, paginação e leitura sem rastreamento atendem à constituição e suportam os 5.000 produtos previstos.

**Alternatives considered**:
- Persistir em uma base separada ou introduzir outro ORM: rejeitado por duplicar infraestrutura.
- Excluir fisicamente produtos/locais na rotina normal: rejeitado por contrariar a regra de preservação do histórico.
- Carregar o catálogo inteiro no navegador e filtrar localmente: rejeitado por não suportar paginação nem a meta de desempenho.

## 5. Limite de desempenho e validação

**Decision**: Verificar buscas com catálogo de referência de até 5.000 produtos, medindo pelo menos 100 requisições após aquecimento e reportando percentil 95; o limite de aceitação permanece o critério da spec: pelo menos 95% em até 2 segundos. Usar PostgreSQL descartável e dados de teste, nunca Supabase de produção.

**Rationale**: O critério é mensurável e tecnologia-agnóstico; o ambiente de teste isolado mantém os gates reproduzíveis e sem credenciais reais.

**Alternatives considered**:
- Medir apenas uma chamada isolada ou uma base vazia: rejeitado porque não valida o volume-alvo.
- Executar teste de carga contra banco real de produção: rejeitado por risco operacional e dependência de dados/segredos externos.
