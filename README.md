# crud2 – Sistema de Controle de Produtos e Fornecedores

Projeto de aprendizado do fluxo de desenvolvimento assistido por IA.

## Estrutura

| Pasta/arquivo | Para que serve | Conteúdo atual |
|---|---|---|
| `.github/copilot-instructions.md` | Instruções permanentes que a IA lê sempre; aponta para os docs | Criado |
| `.github/instructions/` | Regras por tipo de arquivo (`*.instructions.md` com `applyTo`), ex.: regras só para `.cs` ou `.tsx` | Vazio |
| `.github/prompts/` | Prompts reutilizáveis (`*.prompt.md`), ex.: "criar spec", "revisar" | Vazio |
| `docs/01-negocio/` | **O quê e por quê**: problema, escopo, regras de negócio, critérios de aceite | `documento-de-negocio.md` |
| `docs/02-arquitetura/` | **Como**: stack, modelagem, auth, estrutura do repo, diretrizes para a IA | `especificacao-de-arquitetura.md` |
| `docs/03-requisitos/` | Requisitos refinados/derivados (ex.: backlog, épicos, user stories) | Vazio |
| `docs/04-decisoes-adr/` | Registro de decisões (ADR), uma por arquivo | Vazio |
| `specs/` | Specs e artefatos de planejamento por funcionalidade | Base executável em planejamento |
| `src/apps/web` | Frontend Next.js (App Router, TS strict, Tailwind) | Walking skeleton |
| `src/services/api` | Backend .NET 8 em camadas (`SistemaEstoque.*`) | Walking skeleton (`GET /health`) |
| `tests/api`, `tests/web` | Testes xUnit (API) e Vitest (web) | Testes do skeleton |

## Como rodar
Pré-requisitos: .NET 8 SDK, Node 20 e PostgreSQL para executar os testes de integração de persistência.

```powershell
npm ci
dotnet run --project src/services/api/SistemaEstoque.API --urls http://localhost:5013
Copy-Item src/apps/web/.env.example src/apps/web/.env.local   # opcional
npm run dev                                                   # http://localhost:3000
```

O endpoint `GET /health` verifica o host sem depender do banco. `GET /health/ready` retorna `503` quando o PostgreSQL configurado está indisponível.

### Identidade e perfis

A autenticação da aplicação usa Supabase Auth. Copie `src/apps/web/.env.example` para `.env.local` e configure a URL e a chave pública do projeto Supabase. Configure `Supabase__Url`, `Supabase__Audience`, `Supabase__AnonKey` e `Supabase__ServiceRoleKey` somente no ambiente servidor da API (User Secrets em desenvolvimento); nunca exponha a chave administrativa no web ou em arquivos versionados. O procedimento de bootstrap do primeiro Administrador e os cenários locais estão em [specs/003-identidade-perfis/quickstart.md](specs/003-identidade-perfis/quickstart.md).

### Configuração e segredos

A API carrega `appsettings.json`, `appsettings.{Environment}.json` e variáveis de ambiente. Em desenvolvimento, configure uma conexão PostgreSQL por User Secrets ou variável de ambiente:

```powershell
dotnet user-secrets init --project src/services/api/SistemaEstoque.API
dotnet user-secrets set --project src/services/api/SistemaEstoque.API "ConnectionStrings:Default" "<connection-string-local>"
# Alternativa somente para a sessão atual:
$env:ConnectionStrings__Default = "<connection-string-local>"
```

Em produção, forneça `ConnectionStrings__Default` pelo ambiente de execução ou pelo mecanismo de segredos do host. Não adicione connection strings ou chaves reais a arquivos versionados. O CI usa uma instância PostgreSQL descartável com credenciais de teste.

## Build e testes
```powershell
dotnet build src/services/api/SistemaEstoque.sln
dotnet test  src/services/api/SistemaEstoque.sln
npm run lint
npm test
npm run build
```
Os testes de conectividade Npgsql requerem um PostgreSQL isolado configurado em `ConnectionStrings:Default`. O CI (`.github/workflows/ci.yml`) fornece essa dependência para o job `api` e executa os gates de API e web.

## Fluxo sugerido
1. Negócio + arquitetura (feito) →
2. Quebrar o MVP em funcionalidades (`docs/03-requisitos/`) →
3. Escrever uma spec por funcionalidade (`specs/`) →
4. Pedir à IA que implemente a spec em `src/` com testes em `tests/` →
5. Revisar, e registrar decisões relevantes em ADR.

Arquivos `.gitkeep` apenas mantêm pastas vazias no Git; podem ser removidos quando a pasta tiver conteúdo.
