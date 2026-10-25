# Quickstart: validar o fechamento da fase

## Pré-requisitos
- .NET 8 SDK, Node.js, Docker (opcional, para o teste de conectividade).

## 1. Banco descartável (opcional local)
```powershell
docker run --name crud2-test-db -e POSTGRES_DB=crud2 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=<senha-local-descartavel> -p 5432:5432 -d postgres:16
$env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=crud2;Username=postgres;Password=<senha-local-descartavel>"
```
Sem o banco, o teste de conectividade é ignorado com mensagem explícita.

## 2. Gates (sequenciais, nunca em paralelo)
```powershell
dotnet build src/services/api/SistemaEstoque.sln
dotnet test src/services/api/SistemaEstoque.sln --no-build
npm run lint; npm test; npm run build
```
**Esperado**: 0 falhas; teste de desempenho com p95 ≤ 3 s.

## 3. Limpeza
```powershell
docker rm -f crud2-test-db
```

## 4. Relatório
Atualizar `specs/005-fechamento-identidade-perfis/readiness-report.md` com os resultados e a decisão.
