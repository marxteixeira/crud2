# Guia de validação: Melhorar a experiência de Localizações

## Pré-requisitos

- Node.js e dependências do monorepo instaladas.
- Configuração local do frontend e serviços existentes disponível para validação manual do fluxo integrado.

## Testes automatizados

Na raiz do repositório, executar os testes web:

```powershell
npm test
```

Para validar somente o comportamento da página de Localizações, a partir da raiz:

```powershell
npm run test -w @crud2/web-tests -- LocationsPage.test.tsx
```

## Cenários de validação manual

1. Acessar a página de Localizações e confirmar que o formulário para criar estante não é exibido inicialmente.
2. Acionar "+ Cadastrar Nova Estante" e confirmar que o diálogo apresenta os campos Código e Descrição vazios, sem texto de exemplo.
3. Cancelar o diálogo; confirmar que ele fecha sem criar uma estante.
4. Reabrir o diálogo, informar dados válidos e cadastrar; confirmar que o diálogo fecha, a lista é atualizada e a estante criada fica selecionada.
5. Simular uma falha de cadastro; confirmar que o diálogo permanece aberto, os valores digitados são preservados e uma mensagem de erro é apresentada.
6. Selecionar uma estante com prateleiras existentes; confirmar que elas continuam visíveis e que os controles "Código da prateleira" e "Cadastrar prateleira" não aparecem.
7. Selecionar uma estante sem prateleiras e confirmar que a mensagem de estado vazio continua visível.

## Resultado esperado

Todos os cenários automatizados passam e os passos manuais confirmam os requisitos do contrato em [contracts/locations-ui.md](contracts/locations-ui.md), sem alterar o comportamento dos dados persistidos conforme [data-model.md](data-model.md).
