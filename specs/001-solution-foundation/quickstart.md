# Quickstart — Validar a base executável

Este guia valida o skeleton e os critérios desta feature sem pressupor implementação de funcionalidades de negócio.

## Pré-requisitos

- .NET 8 SDK.
- Node.js 20 e npm.
- Acesso a uma instância PostgreSQL isolada para executar localmente testes que validam conectividade/persistência; usar credenciais de teste e mantê-las fora dos arquivos versionados.
- Clone do repositório com dependências restauradas.

## Configurar e executar localmente

1. Configure os valores locais de API/web e, quando aplicável, a conexão PostgreSQL por variáveis de ambiente ou User Secrets:

   ```powershell
   dotnet user-secrets init --project src/services/api/SistemaEstoque.API
   dotnet user-secrets set --project src/services/api/SistemaEstoque.API "ConnectionStrings:Default" "<connection-string-local>"
   # Alternativa somente para a sessão atual:
   $env:ConnectionStrings__Default = "<connection-string-de-desenvolvimento>"
   ```

   Não copie credenciais reais para arquivos commitados.
2. Na raiz do repositório, instale as dependências do workspace:

   ```powershell
   npm ci
   ```

3. Inicie a API:

   ```powershell
   dotnet run --project src/services/api/SistemaEstoque.API --urls http://localhost:5013
   ```

4. Em outro terminal, inicie o frontend:

   ```powershell
   npm run dev
   ```

5. Abra `http://localhost:3000` e confirme que a página inicial reporta a API saudável. Confirme também a resposta HTTP:

   ```powershell
   Invoke-RestMethod http://localhost:5013/health
   ```

O comportamento esperado de `/health` e de prontidão está descrito em [contracts/health-api.yaml](contracts/health-api.yaml).
`/health` deve responder `200` com o host disponível. `/health/ready` responde `200` quando o PostgreSQL pode ser alcançado e `503` quando a dependência está indisponível.

## Validar build e testes

Na raiz do repositório, com PostgreSQL de teste local isolado disponível e configurado em `ConnectionStrings:Default`:

```powershell
dotnet build src/services/api/SistemaEstoque.sln
dotnet test src/services/api/SistemaEstoque.sln
npm run lint
npm test
npm run build
```

Resultado esperado: todos os comandos terminam com sucesso; os testes de API cobrem saúde, respostas de erro e prontidão de acordo com os cenários implementados; os testes de arquitetura continuam verdes; testes web isolados não dependem de segredos ou banco de produção. O job `api` do CI fornece uma instância PostgreSQL descartável para testes de integração do provedor Npgsql.

## Verificar ambientes e segredos

- Executar os testes com a configuração de teste e confirmar que não requer credenciais de desenvolvimento ou produção.
- Iniciar o serviço com uma configuração obrigatória ausente ou inválida e confirmar mensagem explícita sem revelar segredos.
- Confirmar que os arquivos alterados/novos não contêm credenciais.

## Verificar erro e indisponibilidade

- Provocar uma exceção controlada em um teste de integração e verificar que a resposta é RFC 7807 sem stack trace e que o evento é registrado.
- Executar o teste de prontidão com a persistência indisponível e confirmar que não retorna estado pronto.
- Confirmar que falha de chamada do web à API é apresentada como indisponibilidade, não como estado saudável.

Os valores e resultados exatos dos contratos estão definidos em [contracts/health-api.yaml](contracts/health-api.yaml); os elementos de configuração estão descritos em [data-model.md](data-model.md).
