# Contrato de interface: Localizações

## Entrada para cadastro de estante

- **Controle**: Botão "+ Cadastrar Nova Estante".
- **Pré-condição**: Página de Localizações aberta.
- **Resultado**: Abre o modal de cadastro de estante; o formulário não é exibido na página antes dessa ação.
- **Acessibilidade**: Botão acionável por teclado e modal com título associado, fechamento por Escape e rótulos para todos os campos.

## Modal de cadastro

- **Título**: "Cadastrar Nova Estante".
- **Campos**: Código da estante (obrigatório) e Descrição (opcional), inicialmente vazios e sem texto de exemplo.
- **Ações**: Cadastrar estante e cancelar/fechar.
- **Sucesso**: Fechar o modal, limpar o formulário e atualizar a lista com a estante criada selecionada.
- **Falha**: Manter o modal e os campos; mostrar a mensagem de erro existente sem indicar sucesso.
- **Cancelamento**: Fechar sem enviar nem criar estante.

## Seção de prateleiras

- Continuar apresentando as prateleiras existentes e seus controles de edição, situação e consulta de produtos vinculados.
- Não apresentar o campo "Código da prateleira" nem o botão "Cadastrar prateleira".
- Manter mensagem de estado vazio quando a estante selecionada não tiver prateleiras.
