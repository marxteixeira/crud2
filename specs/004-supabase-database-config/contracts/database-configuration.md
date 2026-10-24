# Contrato operacional: configuração do banco Supabase

## 1. Fronteiras de configuração

| Nome | Consumidor | Classificação | Requisito |
|---|---|---|---|
| `ConnectionStrings:Default` / `ConnectionStrings__Default` | API .NET | Segredo | String Npgsql privada, injetada no host servidor por ambiente seguro. |
| `Supabase:Url` / `Supabase__Url` | API .NET | Não secreto por si só | URL do Supabase para os recursos de autenticação já existentes. |
| `Supabase:AnonKey` / `Supabase__AnonKey` | API .NET | Chave de cliente | Credencial pública/anon compatível com o cliente administrativo existente; nunca usar como senha PostgreSQL. |
| `Supabase:ServiceRoleKey` / `Supabase__ServiceRoleKey` | API .NET | Segredo de alto privilégio | Somente no processo servidor e apenas para chamadas administrativas existentes. |
| `NEXT_PUBLIC_SUPABASE_URL` | Web Next.js | Público | URL do projeto usada pelo cliente Supabase existente. |
| `NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY` | Web Next.js | Público | Chave publicável para inicializar o cliente Supabase web; nunca concede conexão PostgreSQL. |
| `NEXT_PUBLIC_API_URL` | Web Next.js | Público | URL pública do backend do ambiente. |

Nenhum valor secreto deve ser colocado em `appsettings*.json`, `src/apps/web/.env.example`, uma variável `NEXT_PUBLIC_*`, argumentos de processo ou saída de diagnóstico. O exemplo versionado contém nomes e valores vazios ou não secretos apenas.

## 2. Seleção do endpoint PostgreSQL

1. Para a API persistente, preferir o endpoint direto fornecido pelo Supabase quando a rede de hospedagem suporta IPv6 ou possui o add-on IPv4.
2. Se a hospedagem for somente IPv4 sem o add-on, usar endpoint Supavisor em modo **session** para o processo persistente.
3. Não selecionar o modo transaction como padrão da API Npgsql/EF Core.
4. Operações de migrations, backup ou manutenção que exigem sessão PostgreSQL usam conexão direta.
5. Os valores específicos de host, porta, database, usuário, senha e CA vêm das configurações do projeto/ambiente, nunca de placeholders gravados no código.

## 3. TLS

- A conexão PostgreSQL exige criptografia TLS e validação da CA e do hostname do servidor (`VerifyFull`).
- Se o host não confiar na CA oficial necessária, provisionar o certificado CA por mecanismo confiável e referenciá-lo pela configuração segura do host.
- Não usar `Trust Server Certificate`, modo `Disable` ou um modo permissivo como correção de conectividade.

## 4. Usuários e privilégio

- A API usa uma role PostgreSQL exclusiva de runtime, sem superuser e sem grants DDL.
- Conceder apenas `CONNECT`, acesso ao schema e permissões DML necessárias às tabelas existentes e às operações efetivamente utilizadas.
- Migrações são executadas fora do startup por role separada, com grants DDL mínimos para o schema da aplicação e conexão direta.
- Nunca usar a senha inicial do role administrativo `postgres` no processo normal da API.
- A senha compartilhada em conversa deve ser rotacionada antes de qualquer uso. Após rotação, atualizar o secret manager e reiniciar/reciclar a API; verificar atividade inesperada da credencial antiga.

## 5. Fontes por ambiente

| Ambiente | Fonte da connection string/segredos |
|---|---|
| Desenvolvimento local | ASP.NET Core User Secrets, sabendo que o arquivo local não é criptografado; variáveis de ambiente temporárias somente por fluxo que não registre o valor. |
| Testes locais/CI | PostgreSQL isolado e credenciais descartáveis já provisionados para o job; não usar banco Supabase de produção. |
| Hospedado | Secret manager do provedor de execução, fornecido ao processo servidor. |

Alterar os segredos não modifica o código. Configuração ausente/inválida ou conexão recusada mantém a readiness como indisponível. Logs e respostas não podem conter valores da configuração.

## 6. Migrações e saúde

- A inicialização normal da API não aplica migrações.
- Um operador autorizado aplica migrações aprovadas como etapa explícita de implantação, usando a identidade de migração.
- `GET /health` preserva a verificação do host sem depender do banco.
- `GET /health/ready` retorna `200` apenas quando a dependência PostgreSQL estiver disponível e `503` quando não estiver; o corpo usa apenas o status e timestamp já contratados, sem detalhe de conexão.

O contrato HTTP detalhado permanece em [health-api.yaml](../../001-solution-foundation/contracts/health-api.yaml).
