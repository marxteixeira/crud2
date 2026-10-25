# Quickstart — Validar localização e produtos

Este guia valida as regras de catálogo e localização com a API, web e PostgreSQL descartável do projeto. Não use credenciais ou dados de produção. O modelo e os endpoints detalhados estão em [data-model.md](data-model.md) e [contracts/catalog-api.yaml](contracts/catalog-api.yaml).

## Pré-requisitos

- .NET 8 SDK e Node.js 20.
- Dependências restauradas na raiz (`npm ci` e restore da solução .NET, se necessário).
- PostgreSQL isolado configurado em `ConnectionStrings:Default` para testes de persistência.
- Configuração local da API e do Supabase Auth de desenvolvimento, conforme [README.md](../../README.md), para smoke tests autenticados.
- Uma conta de teste Administrador e uma conta de teste Operador, sem dados de produção.

## Validar automaticamente

Na raiz do repositório:

```powershell
dotnet build src/services/api/SistemaEstoque.sln
dotnet test src/services/api/SistemaEstoque.sln
npm run lint
npm test
npm run build
```

Resultados esperados:

- Regras de nome normalizado, quantidade, preço, localização, situação e auditoria passam nos testes unitários.
- Testes de integração provam que filtros e paginação combinados retornam somente os registros correspondentes.
- Operador não define nem altera preço nem ativa produto sem preço; a resposta é recusada e os dados permanecem inalterados.
- Administrador define preço e ativa um produto inativo válido; produto sem localização ou com preço inválido não é ativado.
- Inativação de estante/prateleira que contenha produto ativo é recusada.
- Testes não usam Supabase de produção ou segredos reais.

## Validar manualmente via web

1. Aplique a migração aprovada pelo processo explícito de implantação/desenvolvimento; as migrações não são executadas automaticamente no startup.
2. Inicie a API e a aplicação web conforme a seção de execução local do [README.md](../../README.md).
3. Como Administrador, crie uma estante e pelo menos duas prateleiras. Confirme que a seleção de prateleira depende da estante.
4. Como Administrador, crie um produto com preço, quantidade inteira e localização. Pesquise pelo nome, filtre por estante/prateleira/situação, abra o detalhe e consulte o conteúdo da localização.
5. Como Operador, crie um produto sem preço. Confirme que é salvo inativo e não aparece na lista de ativos; tente definir ou alterar preço e confirme a recusa sem mudança persistida.
6. Como Administrador, informe um preço válido e ative esse produto. Confirme que passa a aparecer na consulta de ativos.
7. Tente salvar nome duplicado apenas variando caixa ou acento, quantidade negativa/fracionária, preço negativo e localização que não pertença à estante selecionada. Cada caso deve ser recusado com mensagem compreensível.
8. Tente inativar uma prateleira e uma estante ainda usadas por produtos ativos. Confirme a recusa; realoque os produtos e repita para confirmar que a inativação então é possível.
9. Inative e reative um produto. Confirme que seu histórico permanece e que ele só volta a ativo quando preço e localização estão válidos.

## Verificar resultados e desempenho

- Execute buscas com dados de teste representativos, até 5.000 produtos, após aquecer o ambiente e medir pelo menos 100 chamadas.
- Registre distribuição e percentil 95; 95% ou mais das buscas devem terminar em até 2 segundos.
- Em avaliação com participantes, registre somente dados agregados: 90% ou mais devem localizar o produto em menos de 10 segundos, e 90% ou mais devem concluir sem ajuda cadastro e consulta de localização.
- Confirme que as tarefas de cadastro/localização exigem no máximo três cliques desde a área de produtos.
- Não inclua nomes, e-mails, tokens, strings de conexão ou outros dados pessoais em evidências.
