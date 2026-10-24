# Modelo de dados e configuração: Supabase PostgreSQL

## Escopo

Esta feature não cria tabelas de negócio. Ela define as informações de conexão e suas fronteiras de privilégio entre ambientes. O modelo EF Core atual e suas migrações existentes permanecem inalterados.

## Configuração de ambiente

- **Representa**: os parâmetros necessários para executar o backend e o cliente web em um ambiente.
- **Valores do backend**:
  - `ConnectionStrings:Default`: string privada de conexão Npgsql, injetada em runtime pela fonte segura do ambiente.
  - `Supabase:Url`, `Supabase:Audience`, `Supabase:AnonKey` e `Supabase:ServiceRoleKey`: opções existentes de autenticação/administrador Supabase, conforme seus usos atuais; as chaves permanecem somente no servidor, e a service-role é estritamente administrativa.
  - Certificado CA do banco quando necessário para a validação TLS do ambiente.
- **Valores do web**:
  - `NEXT_PUBLIC_SUPABASE_URL`: URL do projeto Supabase.
  - `NEXT_PUBLIC_SUPABASE_PUBLISHABLE_KEY`: chave publicável para o cliente existente de autenticação Supabase.
  - `NEXT_PUBLIC_API_URL`: origem da API conforme ambiente.
- **Regras**:
  - Valores de banco e segredos administrativos são nunca públicos e não são versionados.
  - A presença de valores `NEXT_PUBLIC_*` implica exposição no cliente; apenas URL e chave publicável são apropriadas.
  - Nomes de ambiente isolam configuração de desenvolvimento, teste, staging e produção.
  - Um valor ausente ou inválido produz falha explícita, com diagnóstico sem o conteúdo do segredo.

## Identidade PostgreSQL de runtime

- **Representa**: login exclusivo usado pelas operações normais da API.
- **Atributos**: nome de role exclusivo por serviço, senha externa à configuração versionada, grants mínimos sobre schema, tabelas e sequências requeridas.
- **Regras**:
  - Não é `postgres`, superuser, owner global ou identidade de migração.
  - Recebe somente os privilégios de leitura/escrita necessários aos fluxos persistentes existentes.
  - Não tem privilégios DDL para criar, alterar ou eliminar schema/tabelas.
  - Pode ter `CONNECT` no database e `USAGE` no schema da aplicação; grants aos objetos devem ser explícitos.
  - Não se presume que RLS seja a fronteira de autorização da conexão direta do backend; a API mantém seus controles atuais de autenticação e autorização.
  - Credencial distinta por ambiente; rotação exige atualização da fonte externa e reciclagem das conexões do serviço.

## Identidade de migração

- **Representa**: login operacional para aplicar migrações EF Core explicitamente.
- **Atributos**: role distinta da identidade de runtime, credencial armazenada somente durante a janela de operação e direitos DDL necessários ao schema gerenciado.
- **Regras**:
  - Não é passada ao processo da API em execução normal.
  - Usada em operação controlada contra conexão direta ao PostgreSQL.
  - Não aparece em scripts SQL versionados, logs, histórico de shell ou saída de CI.
  - Direitos devem ser limitados ao schema da aplicação, nunca concedendo privilégios globais sem necessidade.

## Dados do banco Supabase

- **Representa**: PostgreSQL hospedado que mantém as entidades de aplicação existentes.
- **Atributos de conexão**: host, porta, database, username, senha, modo SSL e CA confiável quando aplicável; os valores concretos são fornecidos pelo ambiente, não fixados nos artefatos.
- **Regras**:
  - Conexão direta é preferida para serviço persistente quando houver suporte IPv6/IPv4 add-on.
  - Supavisor em session mode é alternativa para host IPv4-only sem acesso direto.
  - Transaction mode não é assumido como compatível com o serviço atual de longa duração.
  - TLS verifica cadeia e hostname.
  - Migrações são explícitas e fora do startup.

## Estado de prontidão

- **Representa**: disponibilidade do serviço para operações dependentes da persistência.
- **Regras**:
  - `/health` permanece independente do banco.
  - `/health/ready` retorna sucesso somente quando o PostgreSQL estiver acessível.
  - Falhas retornam indisponibilidade sem devolver connection string, hostname com credencial, mensagem bruta do driver ou exceção interna.

## Relacionamentos

- O host da API injeta `ConnectionStrings:Default` no contexto de Infrastructure existente.
- A role de runtime autentica o contexto e realiza somente operações de dados autorizadas.
- A role de migração administra alterações de esquema durante uma operação separada.
- O web chama a API para dados persistentes e usa URL/chave publicável do Supabase apenas nos fluxos existentes de identidade.
- O health check consulta a conectividade pelo contexto persistente sem expor credenciais.

## Estados e transições

| Elemento | Estado inicial | Transição | Estado final |
|---|---|---|---|
| Configuração de ambiente | Não provisionada | Segredos e opções válidas são fornecidos ao ambiente | Configurada |
| Configuração de ambiente | Configurada | Validação de formato ou conexão falha | Inválida/não pronta |
| Credencial de runtime | Ativa | Senha é rotacionada e fonte de configuração atualizada | Nova credencial ativa |
| Schema | Versão existente | Operação explícita aplica migração aprovada | Nova versão do schema |

Nenhuma transição ou criação de dados ocorre automaticamente no startup da API.
