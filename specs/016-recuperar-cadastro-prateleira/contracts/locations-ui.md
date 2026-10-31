# Contrato de interface: Cadastro de prateleira

## Seção da estante selecionada

- Continuar apresentando o título e a lista de prateleiras da estante selecionada.
- Quando uma estante ativa estiver selecionada, apresentar o campo obrigatório "Código da prateleira" e o botão "Cadastrar prateleira".
- Associar a submissão à estante ativa selecionada no momento do envio.
- Não oferecer o cadastro quando não houver estante ativa selecionada.
- Manter mensagem de estado vazio até a primeira prateleira ser carregada.

## Submissão

- Enviar o código informado por meio da operação existente de criação de prateleira.
- Durante o envio, indicar o estado pendente e impedir submissões repetidas.
- Após sucesso, atualizar a lista da estante selecionada, incluir a nova prateleira e limpar o campo.
- Em erro de validação, duplicidade, estante inválida ou falha inesperada, mostrar mensagem clara, não indicar sucesso e preservar o texto digitado e as prateleiras existentes.
- Se a pessoa mudar a estante enquanto uma operação está pendente, não associar a resposta à estante errada; a nova prateleira deve permanecer identificada pela estante usada no envio.

## Preservação de comportamento e acessibilidade

- Manter edição, ativação/inativação, consulta de produtos vinculados e seleção de localização existentes.
- Fornecer rótulo associado ao campo, botão com nome acessível e feedback de erro anunciável.
- Os controles devem poder ser operados por teclado e seguir os tokens e padrões de responsividade do design system.
