# Pesquisa: Configuração segura do banco Supabase

**Feature**: [spec.md](spec.md)

## 1. Caminho de conexão PostgreSQL

**Decision**: Usar o endpoint PostgreSQL direto para o serviço .NET persistente quando o ambiente tiver conectividade IPv6 ou o add-on IPv4 do Supabase. Para hospedagem exclusivamente IPv4 sem esse add-on, permitir Supavisor em modo session. Não usar modo transaction para esta API sem uma avaliação específica de compatibilidade com recursos de sessão e prepared statements do driver. Executar migrações e outras operações PostgreSQL de sessão única pelo endpoint direto.

**Rationale**: O backend deste projeto é um serviço .NET de longa duração e Npgsql já possui pooling de conexões no cliente. A documentação Supabase recomenda conexão direta para backends persistentes em rede compatível e aponta session mode para serviços persistentes em rede IPv4-only. Transaction mode é destinado a workloads serverless/curtos e não suporta prepared statements. A rede/host de implantação ainda não está definida, então a configuração deve permitir a escolha ambiental sem trocar o modelo de dados.

**Alternatives considered**:
- **Supavisor transaction mode sempre**: rejeitado por ser destinado a conexões curtas/serverless e pelas limitações de prepared statements.
- **Supavisor session mode sempre**: aceitável como fallback IPv4, mas desnecessário para backend persistente em rede compatível e adiciona um pooler compartilhado.
- **Somente conexão direta, sem alternativa**: não cobre hosts IPv4-only sem complemento de conectividade.

**Sources**:
- Supabase, [Connecting to Postgres](https://supabase.com/docs/guides/database/connecting-to-postgres)
- Supabase, [Connection pooling](https://supabase.com/docs/guides/database/connecting-to-postgres#connection-pooling-with-supavisor)
- Supabase, [Pooling and connection limits](https://supabase.com/docs/guides/database/connecting-to-postgres#pooling-and-connection-limits)

## 2. TLS e validação do servidor

**Decision**: Exigir TLS com validação do certificado e do hostname (`VerifyFull`) para a conexão da API. Quando a cadeia do ambiente não confiar na CA do Supabase, instalar a CA oficial em local protegido e apontar a conexão a ela; não desabilitar a validação do certificado como solução de conectividade.

**Rationale**: TLS evita trânsito em claro, enquanto validação de CA e hostname evita aceitar um servidor não autenticado. A opção deve ser compatível com Npgsql e com o provisionamento de CA do host.

**Alternatives considered**:
- TLS sem validar hostname/cadeia: rejeitado por não autenticar adequadamente o servidor.
- Desabilitar TLS: rejeitado por expor credenciais e dados.

**Sources**:
- Supabase, [SSL enforcement](https://supabase.com/docs/guides/platform/ssl-enforcement)
- Npgsql, [Security and SSL](https://www.npgsql.org/doc/security.html)

## 3. Credenciais, privilégios e rotação

**Decision**: Usar User Secrets para configuração local e o secret manager do ambiente de hospedagem para execução hospedada. A API recebe uma credencial PostgreSQL exclusiva de runtime com privilégios apenas sobre o schema/objetos necessários. Migrações são executadas fora do tráfego normal com uma credencial distinta com direitos DDL, nunca incluída na configuração de runtime. Rotacionar a senha PostgreSQL compartilhada antes de utilizá-la; atualizar a configuração externa e reiniciar/reciclar o serviço para que conexões Npgsql sejam reabertas com o novo valor.

**Rationale**: A credencial inicial `postgres` é privilegiada; credenciais de serviço distintas facilitam isolamento e auditoria. User Secrets e secret managers evitam versionar segredos, e separação de runtime/migração reduz o impacto de comprometimento da API. Atualização de senha de serviços externos exige atualizar a configuração da aplicação.

**Alternatives considered**:
- Usar `postgres` como identidade da API: rejeitado por privilégio excessivo.
- Armazenar senha em `appsettings*.json`, `.env.example` ou argumentos de linha de comando: rejeitado por risco de versionamento ou exposição em processo/histórico.
- Usar identidade de runtime para migrations: rejeitado por conceder DDL à aplicação em operação normal.

**Sources**:
- Supabase, [Database roles](https://supabase.com/docs/guides/database/postgres/roles)
- Supabase, [Going into production](https://supabase.com/docs/guides/deployment/going-into-prod)
- Microsoft, [Safe storage of app secrets in development in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/app-secrets)

## 4. Resposta a credencial compartilhada

**Decision**: Tratar a senha enviada em conversa como comprometida e rotacioná-la no painel Supabase antes de qualquer uso. Atualizar qualquer serviço consumidor depois da rotação e verificar a atividade da credencial afetada. Não reimprimir ou copiar a senha para artefatos, testes, terminal ou configuração versionada.

**Rationale**: Um segredo em mensagem não é um canal de armazenamento de credenciais de produção. Rotação impede uso futuro do valor exposto; auditoria ajuda a identificar uso não esperado antes da rotação.

**Alternatives considered**: Reutilizar ou mascarar a senha em exemplo: rejeitado; mascaramento não torna o segredo seguro nem necessário para a configuração.

**Sources**:
- Supabase, [Database roles](https://supabase.com/docs/guides/database/postgres/roles) (gestão de senha e efeito em serviços externos)
- Supabase, [Network restrictions](https://supabase.com/docs/guides/platform/network-restrictions) (defesa em profundidade por allowlist de endereços de saída)

## 5. Configuração pública do cliente Supabase

**Decision**: Manter banco acessível somente pela API. Para o web, fornecer apenas URL do projeto e chave publicável; harmonizar o nome do valor web com `NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY` nos consumidores e no exemplo local. A chave pública não substitui nem concede acesso PostgreSQL. Não adicionar pacotes: os clientes SSR e Supabase JS já existem.

**Rationale**: O browser precisa de configuração pública para a autenticação já implementada, mas credenciais do PostgreSQL e chaves administrativas pertencem somente ao servidor. A chave publicável é o tipo atual de chave de cliente Supabase; sua autorização continua limitada pelas políticas de acesso configuradas.

**Alternatives considered**:
- Conectar o browser diretamente ao PostgreSQL: rejeitado por expor credenciais e ignorar a API existente.
- Adicionar novamente `@supabase/supabase-js` e `@supabase/ssr`: rejeitado porque já estão nas dependências.
- Colocar senha de banco ou chave administrativa em `NEXT_PUBLIC_*`: rejeitado porque Next.js publica essas variáveis nos artefatos do cliente.

**Sources**:
- Supabase, [API keys](https://supabase.com/docs/guides/api/api-keys)
- Supabase, [Creating a client](https://supabase.com/docs/reference/javascript/initializing)

## 6. Testes, prontidão e migrações

**Decision**: Preservar `ConnectionStrings:Default`, o health check `/health/ready`, os testes de conectividade Npgsql e o PostgreSQL descartável do CI. Validar erros de configuração e prontidão sem ecoar credenciais. Manter migrações explícitas e sem aplicação automática no startup.

**Rationale**: São padrões já implementados no repositório e mantêm ambientes isolados sem credenciais externas. `/health` e `/health/ready` já expõem o contrato apropriado para liveness e readiness.

**Alternatives considered**:
- Testar conectividade com a base Supabase de produção: rejeitado por depender de segredo/estado externo.
- Aplicar migrações no startup: rejeitado por misturar operação privilegiada com runtime e gerar alterações não controladas.
- Criar outro endpoint de configuração: rejeitado por não haver nova interface pública necessária.

**Sources**:
- Entity Framework Core, [Connection strings](https://learn.microsoft.com/ef/core/miscellaneous/connection-strings)
- Entity Framework Core, [Applying migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying)
- ASP.NET Core, [Health checks](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)

## Decisões fechadas

- **Database endpoint**: direct por padrão condicionado à rede; Supavisor session como fallback IPv4-only.
- **TLS**: obrigatório, validando CA e hostname.
- **Credential sources**: User Secrets localmente e secret manager do host em ambientes hospedados.
- **Runtime privileges**: role exclusiva sem DDL/superuser; role separada para migrations.
- **Web integration**: URL e chave publicável apenas; nenhuma senha do banco no navegador.
- **Testing/migrations**: manter PostgreSQL CI isolado, prontidão existente e migrations fora do startup.

Não há decisão bloqueada por informações desconhecidas. O provedor de hospedagem e o suporte IPv6 da implantação devem ser selecionados no ambiente; a alternativa de endpoint está documentada no contrato.
