# Research: Corrigir cadastro de estantes

## 1. Causa do 500

- **Evidência**: `CreateRackAsync` consulta `Estantes` e grava; só `DbUpdateException` de unicidade é tratada. Qualquer outra exceção (ex.: tabela `estantes` inexistente, `42P01`) cai no `GlobalExceptionHandler` → 500. O README e o quickstart da 007 dizem que migrations não rodam no startup e que a migração "não foi aplicada".
- **Decision**: tratar como causa principal a migration não aplicada no banco local. Confirmar no ambiente do usuário com a exceção registrada (`Unhandled ... TraceId`) ou consultando a existência da tabela.
- **Alternatives**: bug de validação/mapeamento (descartado: a validação retornaria 400); autenticação (descartado: retornaria 401/403); CORS (descartado: a requisição chegou ao controller e produziu 500).

## 2. O log `OPTIONS 405 ... 204`

- É o preflight CORS, tratado pelo middleware de CORS (204). O nome "405 HTTP Method Not Supported" é o `DisplayName` do endpoint de fallback do roteamento. Não é causa do erro.
- **Decision**: o `RequestLoggingMiddleware` passa a registrar também `Path` e usa-o quando o endpoint é o fallback de 405; mantém `Route` para os demais.

## 3. Como disponibilizar o esquema

- **Decision**: manter migrations explícitas (decisão da feature 004/README; a role de runtime não tem DDL). Adicionar: comando documentado `dotnet ef database update` com a role de migração, e verificação de migrations pendentes na inicialização (`GetPendingMigrationsAsync`), que registra aviso e marca o health check `ready` como Unhealthy.
- **Alternatives**: `Database.Migrate()` no startup (rejeitada: exige DDL na role de runtime e contraria README); EnsureCreated (rejeitada: ignora migrations).

## 4. Diagnóstico

- O handler global já registra a exceção com `TraceId` e devolve `traceId` no Problem Details. **Decision**: manter; garantir via teste e exibir o `traceId` na mensagem do frontend.
