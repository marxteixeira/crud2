# Validação: Fornecedores e vínculos

**Feature**: [spec.md](spec.md)  
**Contrato**: [supplier-api.yaml](contracts/supplier-api.yaml)  
**Modelo**: [data-model.md](data-model.md)

Este roteiro valida cadastro e pesquisa de fornecedores, CPF/CNPJ, vínculos nos dois sentidos, filtro do catálogo, permissões e inativação. Execute em ambiente local de teste com dados descartáveis.

## Pré-requisitos

- .NET 8 SDK, Node.js 20, dependências do monorepo instaladas.
- PostgreSQL descartável configurado para os testes de integração da API.
- Web e API configurados conforme [README.md](../../README.md), incluindo Supabase Auth local/de teste e contas ativas de Administrador e Operador.
- Migrações aplicadas ao banco de teste pela rotina de setup já existente.

## Validação automatizada

Na raiz do repositório, executar:

```powershell
dotnet test src/services/api/SistemaEstoque.sln
```

Em `tests/web`:

```powershell
npm test
```

Resultados esperados: os testes de domínio cobrem CPF/CNPJ válido, inválido e normalização; os testes de aplicação cobrem unicidade, vínculos, inativação e auditoria; os testes de integração cobrem endpoints e autorização; os testes web cobrem cadastro, consulta, filtro, associação, desassociação e mensagens acessíveis.

## Resultados verificados nesta implementação

- `dotnet test tests/api/SistemaEstoque.API.Tests/SistemaEstoque.API.Tests.csproj --configuration FeatureBuild`: 122 aprovados, 1 ignorado por depender de conectividade PostgreSQL.
- `dotnet test tests/api/SistemaEstoque.Domain.Tests/SistemaEstoque.Domain.Tests.csproj --configuration FeatureBuild`: 48 aprovados.
- `npm run test -w @crud2/web-tests`: 56 testes aprovados.
- `npx tsc --noEmit -p src/apps/web/tsconfig.json`: concluído sem diagnósticos.
- `npm run lint -w @crud2/web`: concluído sem avisos ou erros.
- O contrato OpenAPI foi analisado com `js-yaml` e passou no parse.
- `npm run build -w @crud2/web` foi tentado duas vezes, mas o processo Next.js ficou sem progresso durante a inicialização e precisou ser encerrado após o limite de espera; o build de produção permanece não verificado.
- As migrações foram geradas, mas não aplicadas. Os cenários manuais ponta a ponta com PostgreSQL e contas reais de Administrador/Operador não foram executados.

## Validação ponta a ponta

1. Iniciar API e aplicação web conforme o README e autenticar como Administrador.
2. Cadastrar fornecedor com nome e CPF válido; confirmar que aparece na busca.
3. Tentar cadastrar o mesmo CPF com pontuação diferente; esperar rejeição de duplicidade sem novo registro.
4. Tentar CPF com dígitos verificadores inválidos e CNPJ inválido; esperar erro de validação no campo, sem persistência.
5. Cadastrar fornecedor com CNPJ válido e dados opcionais; consultar e editar telefone, e-mail, contato e observações.
6. Cadastrar ou selecionar produto ativo e associá-lo ao fornecedor pela visão de produto; confirmar o vínculo no detalhe do produto e na lista do fornecedor.
7. Associar outro produto e outro fornecedor; confirmar que o relacionamento muitos-para-muitos aparece corretamente nos dois lados.
8. Desassociar um par; confirmar que o vínculo deixa de aparecer nas consultas operacionais e que seus cadastros e histórico permanecem.
9. Na lista de produtos, filtrar por fornecedor e combinar com nome, estante, prateleira e situação; confirmar que cada resultado satisfaz todos os critérios e que a ausência de resultados é informada normalmente.
10. Confirmar que um vínculo histórico de produto inativo permanece consultável e não bloqueia a inativação do fornecedor; confirmar também que não é possível criar novo vínculo quando fornecedor ou produto está inativo.
11. Com um vínculo ativo a produto ativo, tentar inativar o fornecedor; esperar conflito claro, sem mudança de situação. Depois desassociar ou inativar o produto e repetir; a inativação deve concluir e os dados históricos continuar consultáveis.
12. Autenticar como Operador: consultar fornecedor, vínculos e catálogo; tentar alterações administrativas de fornecedor e tentativas não autorizadas de associação; confirmar que são recusadas no servidor e não alteram dados.
13. Verificar no registro de auditoria a conta e o horário UTC das alterações de fornecedor e associação; confirmar que logs não expõem dados pessoais completos sem necessidade.

## Resultado esperado

- CPF/CNPJ válido e normalizado é único; entradas inválidas não são persistidas.
- A associação e desassociação é consistente nas consultas feitas pelo produto e fornecedor.
- O filtro por fornecedor compõe com os demais filtros e paginação do catálogo.
- Restrições de situação e autorização são aplicadas pela API, e não somente pela interface.
- A inativação não apaga cadastros nem trilha de vínculos e não é permitida enquanto houver produto ativo associado.
