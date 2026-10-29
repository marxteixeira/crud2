# Quickstart: Validação da adoção do Design System

## Pré-requisitos

- Dependências instaladas e `.env.local` configurado em `src/apps/web`.
- Prints de referência em `docs/05-design-system/<pasta>/screen.png`.

## Executar

```powershell
cd src\apps\web
npm run dev        # http://localhost:3000
```

## Validação por fase

Para cada tela da fase (ver [contracts/ui-contract.md](./contracts/ui-contract.md)):

1. Abrir a rota em janela de 1280 px e capturar a tela.
2. Comparar lado a lado com o `screen.png`: layout, hierarquia, cores, tipografia, rótulos, ações. Registrar diferenças aceitas.
3. Executar o fluxo funcional (entrar, convidar, cadastrar, filtrar, editar, inativar) e confirmar o mesmo resultado de antes.
4. Provocar carregamento, lista vazia e erro; conferir o padrão visual.
5. Reduzir para ~375 px e navegar apenas por teclado (Tab/Enter/Esc).
6. Rodar os testes e lint:

```powershell
cd src\apps\web
npm run lint
cd ..\..\..
npm test
```

7. Atualizar [execution-control.md](./execution-control.md) (status, datas, evidência, pendências).

## Resultado esperado

- Todos os testes passam; nenhuma regressão funcional.
- Checklist da tela marcado integralmente; fase só passa a Concluída com evidência registrada.
