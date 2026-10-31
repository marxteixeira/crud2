# Quickstart: validação

## Automatizada

```powershell
npm run test -w @crud2/web-tests -- LocationsPage.test.tsx
npm run lint -w @crud2/web
```

Esperado: o novo teste de regressão (refetch atrasado) passa e antes da correção falha; os demais testes continuam passando.

## Manual

1. Abrir "Estantes e prateleiras" com ao menos duas estantes.
2. Clicar em "+ Cadastrar Nova Estante", preencher e cadastrar.
3. Esperado: modal fecha e a nova estante fica selecionada, com sua área de prateleiras, sem voltar à primeira.
4. Trocar manualmente de estante: a escolha é mantida.
