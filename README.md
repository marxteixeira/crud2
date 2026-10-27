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

### Migrations do banco

O esquema (estantes, prateleiras, produtos etc.) só existe depois de aplicar as migrations, o que não acontece no startup. Sem isso, endpoints como `POST /api/locations/racks` retornam `500`. Com a role de migração configurada, aplique-as a partir da raiz do repositório:

```powershell
dotnet ef database update --project src/services/api/SistemaEstoque.Infrastructure --startup-project src/services/api/SistemaEstoque.API
```

Ao iniciar (exceto no ambiente `Testing`), a API registra um aviso com os nomes das migrations pendentes. Quando um erro `500` ocorrer, procure nos logs da API o `TraceId` retornado na resposta (`traceId`) para ver a causa.

### Identidade e perfis

Autenticação usa Supabase Auth. Copie `src/apps/web/.env.example` para `.env.local` e configure `NEXT_PUBLIC_SUPABASE_URL` e `NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY`; ambos são valores de cliente públicos, não credenciais PostgreSQL. Configure `Supabase__Url`, `Supabase__Audience`, `Supabase__AnonKey` e `Supabase__ServiceRoleKey` somente no servidor da API. Configure também `Supabase__RedirectUrl` (URL base do web, padrão `http://localhost:3000`), que deve constar nas URLs de redirecionamento permitidas do Supabase, e ajuste o template "Invite user" para `{{ .RedirectTo }}?token_hash={{ .TokenHash }}&type=invite` (ver [specs/009-corrigir-ativacao-convite/quickstart.md](specs/009-corrigir-ativacao-convite/quickstart.md)). A chave service-role é administrativa e nunca pode ser exposta ao web nem versionada. O procedimento de bootstrap do primeiro Administrador e os cenários locais estão em [specs/003-identidade-perfis/quickstart.md](specs/003-identidade-perfis/quickstart.md).

### Configuração e segredos

A API carrega `appsettings.json`, `appsettings.{Environment}.json`, User Secrets no ambiente Development e variáveis de ambiente. Os arquivos `appsettings*.json` versionados não contêm connection strings. O projeto já habilita User Secrets por meio de um identificador configurado no arquivo de projeto. Armazene `ConnectionStrings:Default` e demais chaves administrativas por um fluxo local que não exponha valores no histórico ou em argumentos de processo. User Secrets são armazenados localmente sem criptografia e são apropriados para desenvolvimento, não como cofre de produção. Em ambientes hospedados, forneça `ConnectionStrings__Default` e as chaves administrativas pelo secret manager do host. Ambientes não locais exigem `SSL Mode=VerifyFull` e validação do certificado; não desative a verificação TLS para contornar erros de conexão.

A API persistente deve usar uma role PostgreSQL de runtime com privilégios mínimos, separada da role administrativa e da role usada para aplicar migrações. A migração de esquema é uma operação explícita de implantação; ela não roda no startup. O CI usa um PostgreSQL descartável e credenciais de teste. Nunca copie uma senha compartilhada em conversa para configuração; rotacione-a antes de qualquer uso.

Consulte o [contrato de configuração e privilégios](specs/004-supabase-database-config/contracts/database-configuration.md) e o [quickstart de provisionamento e validação](specs/004-supabase-database-config/quickstart.md) para escolher endpoint direto ou Supavisor session de acordo com a rede do host, validar readiness e rotacionar credenciais sem alterar o código.

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
