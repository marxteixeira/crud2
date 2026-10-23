# Pesquisa — Base executável da solução

**Escopo**: resolver decisões de desenho e validação da fundação usando somente as tecnologias e camadas aprovadas na constitution, na arquitetura, no ADR-0001 e no skeleton atual. Não se introduz novo provedor de logs, framework de arquitetura de testes ou serviço de configuração.

## 1. Saúde e prontidão

**Decision**: Manter `GET /health` como verificação de disponibilidade do host e adicionar `GET /health/ready` para avaliar a dependência PostgreSQL necessária às operações persistentes. A prontidão retorna sucesso somente com o banco disponível; em caso contrário retorna `503`. Ambas preservam o corpo `status` e `timestamp` já consumido pelo frontend, sem expor mensagens de conexão.

**Rationale**: A spec exige que indisponibilidade de persistência seja distinguível de serviço saudável. Dois propósitos de verificação evitam que uma falha do banco faça o processo parecer encerrado e mantêm compatibilidade com o endpoint atual.

**Alternatives considered**: Fazer `/health` depender do banco; usar um endpoint com filtragem interna sem rota explícita. Não selecionadas porque acoplam a verificação já consumida pelo web à persistência ou deixam menos claro o contrato de prontidão. Painéis ou pacotes de health UI estão fora da stack aprovada.

**Source**: [ASP.NET Core health checks](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0), incluindo probes de liveness/readiness e filtragem por tags.

## 2. Erros globais e Problem Details

**Decision**: Evoluir o tratamento já registrado no host (`AddProblemDetails`, `UseExceptionHandler` e `UseStatusCodePages`) para mapear falhas esperadas e inesperadas a respostas RFC 7807, sem stack trace ou dados sensíveis. Erros de validação permanecem respostas de cliente apropriadas e distinguíveis de falhas inesperadas.

**Rationale**: Reutiliza o mecanismo ASP.NET Core já presente e atende à constitution sem um middleware ou formato concorrente. A resposta segura deve permitir localizar o evento correspondente pelos dados de correlação disponíveis, sem copiar detalhes internos para o cliente.

**Alternatives considered**: Middleware próprio independente ou uma biblioteca de tratamento de erros. Não selecionados: o primeiro duplicaria o tratamento já iniciado e a segunda adicionaria tecnologia não aprovada.

**Source**: [ASP.NET Core error handling](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling?view=aspnetcore-8.0), [ASP.NET Core Problem Details](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-8.0) e [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807).

## 3. Logging estruturado

**Decision**: Usar `Microsoft.Extensions.Logging` e `ILogger<T>` com templates de mensagem estruturados e o formatter JSON do console integrado ao .NET. Configurar níveis por ambiente com o mecanismo de logging e as seções `Logging` já existentes. Não registrar credenciais, connection strings nem conteúdo sensível; erros de cliente não incluem stack trace.

**Rationale**: O logger padrão do host atende ao requisito sem introduzir um provider fora da constitution. O formatter JSON facilita consumo automatizado, mantendo os logs independentes de plataforma.

**Alternatives considered**: Saída de texto sem formato estruturado; Serilog, NLog ou middleware de logging HTTP adicional. Texto estruturado é menos interoperável; os providers e middleware não estão aprovados e exigiriam nova dependência/decisão.

**Source**: [ASP.NET Core logging](https://learn.microsoft.com/aspnet/core/fundamentals/logging?view=aspnetcore-8.0) e [console log formatting](https://learn.microsoft.com/dotnet/core/extensions/console-log-formatter).

## 4. Persistência PostgreSQL/Supabase

**Decision**: Manter `DbContext`, configurações EF Core e migrações em `SistemaEstoque.Infrastructure`, usando EF Core 8 e Npgsql conforme arquitetura. A configuração da conexão usa a chave `ConnectionStrings:Default`; nenhum schema de negócio é criado nesta feature. A aplicação não aplica migrações automaticamente na inicialização.

**Rationale**: Segue as responsabilidades de camada aprovadas e deixa o versionamento do schema explícito/revisável. O repositório não especifica ambiente de hospedagem nem modo de pooler Supabase, então esta fundação usa a connection string provida pelo ambiente, sem fixar endpoint, porta ou política de pool.

**Alternatives considered**: Acesso SQL direto ou ORM diferente, ambos contrários à arquitetura; aplicar migrações automaticamente em toda inicialização, o que acoplaria implantação e mudança de schema sem decisão aprovada. Tabelas de negócio são postergadas para suas próprias specs.

**Source**: [EF Core connection strings](https://learn.microsoft.com/ef/core/miscellaneous/connection-strings), [EF Core migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/) e [Npgsql EF Core provider](https://www.npgsql.org/efcore/).

## 5. Configuração por ambiente e injeção de dependência

**Decision**: Usar a configuração padrão ASP.NET Core (`appsettings.json`, `appsettings.{Environment}.json`, variáveis de ambiente e User Secrets apenas em desenvolvimento). Valores secretos de CI/produção vêm do ambiente de execução ou do mecanismo de segredos do host; nenhum arquivo de produção com credenciais é versionado. Agrupar e validar configurações obrigatórias no início do serviço. A API permanece composition root e cada camada registra seus serviços sem inverter dependências. O web mantém `NEXT_PUBLIC_API_URL` e o carregamento nativo de ambiente do Next.js.

**Rationale**: São mecanismos nativos das plataformas já aprovadas e correspondem aos arquivos e à variável web já existentes. A validação antecipada evita falhas tardias e mensagens ambíguas.

**Alternatives considered**: Serviço externo de configuração/segredos ou configuração própria. Não selecionados por exigirem tecnologia e operação sem aprovação documental.

**Source**: [ASP.NET Core configuration](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0), [ASP.NET Core options pattern](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options?view=aspnetcore-8.0), [safe storage of app secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets?view=aspnetcore-8.0) e [Next.js environment variables](https://nextjs.org/docs/app/building-your-application/configuring/environment-variables).

## 6. Testes e CI

**Decision**: Preservar xUnit, `WebApplicationFactory`, Vitest e Testing Library. Estender o teste de dependências por inspeção/reflection dos assemblies, sem pacote adicional. Para testes que exercitam conectividade real Npgsql, usar PostgreSQL como service container no job `api` do workflow GitHub Actions já existente; testes unitários e de web permanecem isolados e não dependem de credenciais Supabase.

**Rationale**: Os frameworks e os dois jobs já existem e são obrigatórios pela constitution. O service container é capacidade do CI já aprovado e fornece dependência PostgreSQL descartável para os testes de integração sem acoplar os testes ao banco real.

**Alternatives considered**: EF InMemory ou SQLite para validar integração Npgsql, que não exercitam o provedor relacional aprovado; Testcontainers ou uma biblioteca de arquitetura, que adicionariam pacotes. Um banco Supabase compartilhado em CI introduziria segredos e estado externo desnecessários.

**Source**: [ASP.NET Core integration tests](https://learn.microsoft.com/aspnet/core/test/integration-tests?view=aspnetcore-8.0), [GitHub Actions service containers](https://docs.github.com/actions/using-containerized-services/about-service-containers), [Vitest configuration](https://vitest.dev/config/) e [Testing Library](https://testing-library.com/docs/react-testing-library/intro/).

## Decisões de escopo resolvidas

- `docs/03-requisitos/` contém apenas `.gitkeep`; portanto, nenhum requisito de negócio adicional é inferido nesta fundação.
- `GET /health/ready` é o nome escolhido para o contrato de readiness, enquanto `GET /health` mantém o contrato já existente.
- O teste arquitetural segue a estratégia reflection-based existente; nenhuma biblioteca adicional é necessária.
- O job `api` disponibiliza PostgreSQL isolado para testes de integração do provedor; testes web e unitários não requerem esse banco.
- User Secrets é exclusivo para desenvolvimento local. CI usa a configuração do service container e produção recebe valores por ambiente/segredos do host.
- O formato escolhido para logging estruturado é JSON no console integrado ao .NET.
- Nenhum driver de pooler ou topologia de hospedagem Supabase é fixado sem requisito de implantação; a connection string é responsabilidade da configuração de ambiente.
