# Research: Seleção da nova estante

## Causa raiz

Em `LocationsPageClient.tsx`, `createRackMutation.onSuccess` executa `setSelectedRackId(rack.id)` e só depois `await refresh()`. Entre os dois passos, `racksQuery.data` ainda é a lista antiga. O `useEffect` que valida a seleção (`!racks.some(r => r.id === selectedRackId)`) reseta para `racks[0].id`. Quando a lista nova chega, o id selecionado já é o da primeira estante, então a nova nunca é exibida. Os testes da feature 013 não pegaram o problema porque o mock já devolvia a nova estante no refetch e `findByRole` aguardava.

## Decisão

Antes de selecionar, inserir a nova estante no cache de `["locations", "racks"]` com `queryClient.setQueryData` (respeitando a ordenação existente, sem duplicar o id) e então chamar `setSelectedRackId(rack.id)` e `refresh()`.

**Racional**: a lista em cache passa a conter o id selecionado imediatamente, então o efeito de validação não interfere; o refetch posterior apenas confirma os dados do servidor. Mudança mínima, sem novo estado nem refs.

## Alternativas consideradas

- **Aguardar o refetch antes de selecionar** (`await refresh()` e depois `setSelectedRackId`): simples, mas deixa a tela na estante antiga durante a espera e o efeito pode ainda interferir se o refetch falhar.
- **Estado "seleção pendente" no efeito**: mais código e mais estados a manter; só necessário se a inserção no cache não for viável.

## Teste de regressão

Mock de `createRack` devolvendo a nova estante e `listRacks` com a lista antiga em caso de refetch atrasado (promise controlada): verificar que a nova estante fica selecionada antes e depois da resolução do refetch, e que nunca volta à primeira.
