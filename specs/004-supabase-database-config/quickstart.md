# Quickstart: configurar e validar Supabase PostgreSQL

Este procedimento configura credenciais novas e segregadas por ambiente. A senha PostgreSQL compartilhada na conversa deve ser rotacionada no painel do Supabase antes de ser usada. Não a reutilize nem copie em comando, arquivo, log ou histórico de shell.

## Pré-requisitos

- .NET 8 SDK e Node.js 20.
- Dependências do repositório restauradas.
- Acesso administrativo autorizado ao projeto Supabase para rotacionar a senha e provisionar roles PostgreSQL.
- Uma nova senha exclusiva para a role de runtime, gerada e guardada no secret manager apropriado.
- O host de API escolhido; confirmar IPv6/IPv4 e escolher endpoint segundo [database-configuration.md](contracts/database-configuration.md).

## 1. Proteger e provisionar credenciais

1. No Supabase Dashboard, rotacione imediatamente a senha administrativa compartilhada, sem a copiar para o repositório. Audite atividade recente da credencial antiga e revogue quaisquer acessos não esperados.
2. Crie uma role PostgreSQL específica da API com login e privilégios mínimos para as operações persistentes já implementadas. Não use `postgres` como identidade de runtime e não conceda privilégios de superuser ou DDL à API.
3. Crie uma role distinta para migrações, com DDL somente no schema gerenciado, e guarde sua credencial separadamente. Use-a apenas na operação de implantação de migrations por conexão direta.
4. Habilite/verifique a exigência de SSL do projeto e obtenha a CA oficial quando o ambiente necessitar dela. A conexão deve validar CA e hostname.
5. O projeto API já habilita User Secrets. Grave `ConnectionStrings:Default` e as opções administrativas Supabase no armazenamento local por uma ferramenta/fluxo confiável que não coloque os valores em argumentos de processo ou no histórico do shell. O armazenamento de User Secrets é de conveniência para desenvolvimento e não criptografa os valores; proteja a conta/disco local. Não copie segredos para `appsettings.Development.json` nem para `.env.local`.

6. No web, configure somente os valores públicos necessários em `.env.local`, que permanece ignorado pelo Git:

   ```dotenv
   NEXT_PUBLIC_API_URL=http://localhost:5013
   NEXT_PUBLIC_SUPABASE_URL=
   NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY=
   ```

   Nunca adicione senha PostgreSQL ou service-role key ao arquivo do web. O exemplo versionado em `src/apps/web/.env.example` deve conter somente nomes públicos e valores vazios.

7. Em produção, fornecer `ConnectionStrings__Default` e as chaves administrativas da API pelo secret manager do host. Não usar `NEXT_PUBLIC_*` para nenhum segredo.

## 2. Validar configuração e prontidão

Com o PostgreSQL de teste existente no job CI ou em um banco de desenvolvimento isolado:

Para um banco descartável local (o teste de conectividade só roda com `ConnectionStrings__Default` apontando para `localhost`; sem ela, é ignorado com mensagem explícita):

```powershell
docker run --name crud2-test-db -e POSTGRES_DB=crud2 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=<senha-local-descartavel> -p 5432:5432 -d postgres:16
$env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=crud2;Username=postgres;Password=<senha-local-descartavel>;SSL Mode=Disable"
# ao terminar: docker rm -f crud2-test-db
```

```powershell
dotnet build src/services/api/SistemaEstoque.sln
dotnet test src/services/api/SistemaEstoque.sln
```

Inicie a API com configuração local válida:

```powershell
dotnet run --project src/services/api/SistemaEstoque.API --urls http://localhost:5013
```

Confirme:

- `GET http://localhost:5013/health` retorna `200` quando o host está ativo, independentemente do banco.
- `GET http://localhost:5013/health/ready` retorna `200` com banco disponível e `503` quando a conexão é recusada.
- Configuração ausente ou malformada falha com mensagem segura e sem o valor recebido.
- Logs, respostas de saúde e bundles web não contêm string de conexão, senha, service-role key ou outros segredos.
- A API usa role sem DDL; migrations continuam sendo operação explícita. Não execute migrations contra produção como parte deste quickstart.

Validar o web sem credenciais privadas:

```powershell
npm run lint
npm test
npm run build
```

O cliente Supabase web deve funcionar apenas com URL e chave publicável de ambiente isolado; a senha do banco não participa do bundle.

## 3. Rotação e diagnóstico operacional

1. Gere uma senha nova e atualize a role necessária pelo procedimento administrativo do projeto.
2. Atualize o secret manager do ambiente (ou User Secrets local).
3. Recicle o processo da API para descartar conexões abertas com a credencial anterior.
4. Confirme `/health/ready` e um fluxo de persistência não destrutivo em ambiente isolado.
5. Confirme que a credencial antiga já não autentica e revise atividade de banco/logs por uso inesperado. Não registre senhas nos tickets de operação.

Se a conexão direta não funcionar por limitação IPv4 do host, configure o endpoint Supavisor session, mantendo TLS e as mesmas regras de segredo e privilégio. Não desative validação TLS nem substitua a credencial de banco por uma chave publicável.
