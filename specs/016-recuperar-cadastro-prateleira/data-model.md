# Modelo de dados e estado: Recuperar cadastro de prateleira

Esta feature não introduz entidades persistidas nem modifica o esquema. Ela restabelece o uso do cadastro de prateleira já disponível na aplicação.

## Entidades existentes envolvidas

### Estante

- **Representa**: Localização ativa que agrupa prateleiras.
- **Dados relevantes à tela**: Identificador, código, situação e lista de prateleiras.
- **Regras**: O cadastro de uma prateleira requer a estante proprietária existente e ativa.
- **Relação**: Uma estante possui zero ou mais prateleiras.

### Prateleira

- **Representa**: Divisão de uma estante utilizada para localizar produtos.
- **Dados relevantes à tela**: Identificador, identificador da estante proprietária, código e situação.
- **Validação**: Código obrigatório e válido conforme as regras existentes do domínio.
- **Unicidade**: Código normalizado único dentro da mesma estante; códigos iguais podem existir em estantes diferentes.
- **Auditoria**: A criação registra ator e horários conforme a regra existente.
- **Relação**: Pertence a uma única estante e pode ser referenciada por produtos.

### Produto

- **Representa**: Item que pode ser localizado em uma prateleira.
- **Regra relevante**: A prateleira cadastrada deve permanecer disponível somente sob sua estante proprietária nos fluxos de localização.

## Estado de interação da tela

| Estado | Conteúdo | Transições principais |
|---|---|---|
| Estante ativa selecionada | Formulário com código vazio, campo obrigatório e ação de cadastro; lista de prateleiras existente | Enviar dados válidos inicia a criação; trocar estante exibe o formulário para a nova seleção |
| Salvando prateleira | Campo e ação indicam operação pendente; submissões duplicadas ficam impedidas | Sucesso atualiza a lista; falha apresenta mensagem sem descartar o código |
| Cadastro concluído | Nova prateleira aparece na lista da estante proprietária; demais prateleiras permanecem | A pessoa pode usar os fluxos existentes de edição, situação e consulta de produtos |
| Cadastro recusado | Código digitado e lista anterior são preservados; mensagem contextual explica validação ou conflito | A pessoa corrige o valor e tenta novamente |
| Sem estante ativa selecionada | Cadastro não está disponível | A pessoa seleciona uma estante ativa para iniciar |

## Persistência e escopo negativo

- Não há nova entidade, coluna, migração ou mudança de relacionamento.
- Não há novo endpoint; o cadastro usa o endpoint existente de prateleiras.
- O cadastro de uma prateleira não altera os fluxos de manutenção de estantes ou de prateleiras existentes.
