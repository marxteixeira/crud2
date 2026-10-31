# Quickstart: validar campos da estante selecionada

## Automatizado

```powershell
npm run test --workspace src/apps/web -- LocationsPage
```

(ou o comando de testes web já usado no repositório, filtrando `tests/web/LocationsPage.test.tsx`).

Esperado: teste de troca de estante passa; sem regressões nos demais testes da tela.

## Manual

1. Ter ao menos duas estantes com códigos e descrições diferentes (uma sem descrição).
2. Abrir "Estantes e prateleiras"; conferir que Código e Descrição correspondem à estante inicial.
3. Trocar a estante no seletor: os campos passam a mostrar os valores da nova estante; a sem descrição mostra o campo vazio.
4. Digitar algo sem salvar e trocar de estante: a digitação é descartada.
5. Cadastrar uma nova estante: ela fica selecionada e os campos mostram seus valores.
