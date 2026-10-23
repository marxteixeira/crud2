# Modelo de Dados — Base executável da solução

## Escopo

Esta fundação não cria entidades ou tabelas de negócio. O modelo abaixo descreve apenas os elementos de configuração e infraestrutura necessários para ligar os componentes existentes e validar a prontidão da persistência. As entidades `Produto`, `Fornecedor`, `Estante`, `Prateleira` e `Usuário` pertencem às futuras especificações de negócio.

## Elementos de configuração

### Ambiente da aplicação

- **Representa**: valores não secretos e segredos selecionados para uma execução em desenvolvimento, testes ou produção.
- **Dados**: nome/ambiente de execução, URL permitida para CORS, URL da API (`NEXT_PUBLIC_API_URL`) utilizada pelo web, níveis de logging e `ConnectionStrings:Default` para acesso ao PostgreSQL/Supabase.
- **Regras**:
  - Valores específicos de ambiente são fornecidos sem alteração do código.
  - ASP.NET Core combina arquivos de configuração do ambiente, variáveis de ambiente e User Secrets em desenvolvimento; produção e CI usam valores injetados fora do código.
  - Segredos são lidos de fontes externas ao controle de versão.
  - Ausência ou invalidade de configuração obrigatória deve resultar em erro explícito e diagnóstico sem revelar o valor secreto.
  - Configuração usada em testes é isolada dos ambientes de desenvolvimento e produção.

### Contexto de persistência

- **Representa**: a conexão entre a camada Infrastructure e a instância PostgreSQL aprovada.
- **Dados**: connection string obtida de `ConnectionStrings:Default` e metadados de saúde necessários para determinar conectividade.
- **Regras**:
  - O contexto pertence a Infrastructure; domínio e Application não dependem do provedor de banco.
  - Alterações futuras de esquema são controladas por migrações EF Core.
  - Migrações são aplicadas como operação explícita de implantação, não automaticamente na inicialização da API.
  - Nenhuma tabela de negócio é criada neste escopo.
  - Falha de conexão impede que verificações de prontidão para operações dependentes da persistência reportem estado pronto.

### Estado de serviço

- **Representa**: condição observável do serviço para inicialização e dependências.
- **Dados**: status de saúde, timestamp UTC na resposta já existente, e resultado de conectividade da persistência na verificação de prontidão.
- **Regras**:
  - `GET /health` preserva a verificação de saúde já consumida pelo web.
  - A verificação de prontidão distingue falha da dependência de banco de uma instância saudável.
  - A resposta não contém connection strings, segredos ou stack traces.

### Registro de evento operacional

- **Representa**: informação de diagnóstico produzida para requisições e falhas.
- **Dados**: severidade, mensagem estruturada, contexto de operação/correlação disponível e exceção sanitizada conforme apropriado.
- **Regras**:
  - Eventos são estruturados e não registram credenciais ou segredos.
  - A saída estruturada usa o formatter JSON do console integrado ao runtime aprovado.
  - Falhas tratadas globalmente devem ter registro correspondente para diagnóstico.
  - A resposta HTTP não é fonte de stack trace ou detalhes internos.

## Relações

- Host da API configura os serviços Application e Infrastructure e os disponibiliza via injeção de dependência.
- Infrastructure depende da configuração do ambiente para criar o contexto de persistência; Application usa contratos de domínio sem referenciar Infrastructure.
- As verificações de saúde e prontidão consultam o estado do host e da dependência de persistência sem criar dados de negócio.
- A aplicação web consome o contrato de saúde da API através da integração HTTP existente em `lib/`.

## Estados

| Elemento | Estado | Significado |
|---|---|---|
| Serviço | Saudável | Host disponível para responder à verificação de saúde existente. |
| Persistência | Disponível | PostgreSQL pode ser alcançado pela configuração atual. |
| Prontidão | Pronto | Serviço e dependências obrigatórias para operações persistentes estão disponíveis. |
| Prontidão | Não pronto | A dependência necessária está indisponível; operações dependentes não devem ser anunciadas como prontas. |

Não há transições de estado de entidades de negócio nesta fundação.
