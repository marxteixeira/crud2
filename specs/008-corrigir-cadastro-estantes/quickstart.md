# Quickstart: validar a correção

## Pré-requisitos

- PostgreSQL de desenvolvimento acessível, com a configuração de conexão local (user-secrets; sem segredos versionados).
- Role de migração com DDL (ver [quickstart da 004](../004-supabase-database-config/quickstart.md)).

## 1. Reproduzir/diagnosticar

1. Chame `POST /api/locations/racks` e anote o `traceId` da resposta.
2. Procure nos logs da API `Unhandled ... TraceId <id>`; a exceção deve citar a causa (ex.: relação `estantes` inexistente).

## 2. Aplicar migrations

```powershell
dotnet ef database update --project src/services/api/SistemaEstoque.Infrastructure --startup-project src/services/api/SistemaEstoque.API
```

Esperado: `AddStorageLocations` e `AddProducts` aplicadas.

## 3. Validar

1. Inicie a API: sem aviso de migrations pendentes; `/health/ready` saudável.
2. Em Localizações, cadastre a estante `A1`: aparece na lista (SC-001).
3. Cadastre `a1 ` novamente: mensagem de duplicidade (409).
4. Cadastre código vazio: mensagem de validação por campo.
5. Com migration pendente (banco antigo): a API registra aviso claro e `/health/ready` retorna 503.
6. Percorra o roteiro de [007](../007-localizacao-produtos/quickstart.md): sem erro 500.

## 4. Testes automatizados

```powershell
dotnet test src/services/api/SistemaEstoque.sln
npm --prefix tests/web test
```
