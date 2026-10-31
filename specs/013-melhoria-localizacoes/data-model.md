# Modelo de dados e estado: Melhorar a experiência de Localizações

Esta alteração não introduz entidades persistidas nem modifica o modelo de domínio. Ela reorganiza a apresentação e os estados de interação de entidades já existentes.

## Entidades existentes envolvidas

### Estante

- **Representa**: Uma localização de estoque que agrupa prateleiras.
- **Dados relevantes à tela**: Identificador, código, descrição, situação e lista de prateleiras.
- **Validação**: Reutiliza as regras existentes de criação e atualização. O código é obrigatório; a descrição pode permanecer vazia e ser enviada como nula conforme o comportamento atual.
- **Relação**: Uma estante possui zero ou mais prateleiras.

### Prateleira

- **Representa**: Um nível de armazenamento pertencente a uma estante.
- **Dados relevantes à tela**: Identificador, código e situação.
- **Relação**: Pertence a uma estante e pode ser consultada e mantida pelos fluxos existentes.

## Estado de interação da tela

| Estado | Conteúdo | Transições principais |
|---|---|---|
| Modal de criação fechado | Página de Localizações, lista/seleção de estantes e seção de prateleiras | Acionar "+ Cadastrar Nova Estante" abre o modal |
| Modal de criação aberto | Formulário vazio com Código e Descrição; foco e interação limitados ao diálogo | Cancelar/fechar retorna à página sem persistência; sucesso fecha e atualiza dados; falha mantém modal e valores |
| Salvando estante | Formulário aberto com indicação de operação pendente e ação de envio indisponível | Sucesso segue para fechamento/atualização; falha exibe erro e permite nova tentativa |
| Estante criada selecionada | Página principal atualizada, nova estante visível e selecionada; prateleiras correspondentes podem estar vazias | A pessoa consulta as prateleiras e as ações já existentes |

## Escopo negativo

- Não há mudança de esquema ou persistência.
- A criação de prateleiras deixa de ser iniciada nesta tela; prateleiras já existentes permanecem acessíveis e mantêm os fluxos de edição, situação e consulta já disponíveis.
- Código e Descrição do editor de estante selecionada continuam refletindo valores existentes.
