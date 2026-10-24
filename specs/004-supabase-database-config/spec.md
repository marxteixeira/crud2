# Feature Specification: Configuração segura do banco Supabase

**Feature Branch**: `[004-supabase-database-config]`

**Created**: 2026-10-24

**Status**: Draft

**Input**: User description: Configurar a conexão do sistema ao banco PostgreSQL no Supabase, determinar qual das opções fornecidas é adequada ao projeto e proteger os dados confidenciais.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Conectar o sistema ao banco hospedado (Priority: P1)

Como responsável pela operação, quero configurar o sistema para acessar o banco PostgreSQL hospedado no Supabase, para que as funcionalidades que dependem de dados persistidos operem no ambiente correto.

**Why this priority**: Sem uma conexão válida com o banco, o sistema não consegue persistir nem consultar os dados necessários.

**Independent Test**: Configurar um ambiente de teste isolado com credenciais válidas e verificar que a aplicação consegue alcançar o banco sem expor credenciais.

**Acceptance Scenarios**:

1. **Given** um ambiente com configuração válida e banco disponível, **When** o sistema inicia e verifica sua prontidão, **Then** a dependência de persistência é indicada como disponível.
2. **Given** uma aplicação web em uso, **When** uma operação de dados é realizada, **Then** ela usa o serviço de backend e não requer nem recebe a senha do banco.
3. **Given** uma alteração de esquema aprovada, **When** o responsável executa a operação de implantação correspondente, **Then** a alteração é aplicada fora da inicialização normal do serviço.
4. **Given** a identidade de execução do sistema, **When** ela tenta uma operação necessária à aplicação e outra fora das permissões concedidas, **Then** a operação necessária é permitida e a operação não autorizada é recusada.

### User Story 2 - Proteger e manter as credenciais (Priority: P1)

Como responsável pela operação, quero manter credenciais fora do código, dos arquivos versionados e do navegador, para reduzir o risco de acesso não autorizado e permitir a rotação por ambiente.

**Why this priority**: A senha de banco concede acesso a dados e não pode ser tratada como configuração pública.

**Independent Test**: Verificar o processo de configuração e rotação em ambiente isolado, e inspecionar repositório, respostas públicas e logs para confirmar que nenhum segredo aparece.

**Acceptance Scenarios**:

1. **Given** credenciais configuradas em uma fonte segura apropriada ao ambiente, **When** o backend inicia, **Then** consegue utilizá-las sem que sejam incluídas em arquivos versionados ou artefatos públicos.
2. **Given** que uma credencial esteja ausente ou inválida, **When** o sistema tenta iniciar ou verificar a prontidão, **Then** sinaliza a falha com diagnóstico seguro e não exibe a credencial.
3. **Given** que uma credencial precise ser rotacionada, **When** o responsável atualiza a fonte de segredos e reinicia ou atualiza o ambiente conforme o procedimento operacional, **Then** o sistema pode voltar a conectar sem alteração de código.
4. **Given** a execução de testes automatizados, **When** os testes exercitam a persistência, **Then** usam um banco isolado e não tentam conectar ao ambiente de produção.

### User Story 3 - Distinguir conexão de banco de configuração de cliente (Priority: P2)

Como responsável pela implantação, quero saber quais valores configuram a persistência e quais são destinados ao cliente Supabase do navegador, para evitar usar uma chave pública como senha de banco ou expor uma credencial administrativa.

**Why this priority**: As opções fornecidas atendem a propósitos diferentes; distingui-las evita falhas de configuração e exposição de privilégios.

**Independent Test**: Revisar a configuração documentada e confirmar que o banco é acessado pelo backend, enquanto valores publicáveis do cliente não são aceitos como credenciais de banco.

**Acceptance Scenarios**:

1. **Given** que o responsável configure a persistência, **When** seleciona entre as opções fornecidas, **Then** usa a credencial de conexão PostgreSQL no ambiente servidor e não a chave publicável do cliente como credencial do banco.
2. **Given** que o cliente web use recursos Supabase existentes, **When** recebe configuração pública, **Then** essa configuração contém somente valores publicáveis necessários ao cliente e nunca a senha do banco nem uma chave administrativa.

### Edge Cases

- O banco hospedado está indisponível, a conexão é recusada ou a rede do ambiente não alcança o endpoint configurado: a verificação de prontidão indica indisponibilidade e o diagnóstico não contém credenciais.
- A credencial está ausente, expirada, incorreta ou contém caracteres especiais: a configuração falha explicitamente ou conecta com o valor interpretado corretamente, sem truncamento nem exposição.
- A string de conexão direta não é compatível com a rede do ambiente de execução: a implantação exige uma alternativa de conexão suportada e aprovada para esse ambiente, sem recorrer a uma chave de cliente como substituto.
- O ambiente de teste não deve se conectar ao banco de produção nem depender de credenciais reais.
- Uma migração necessária falha: o processo informa a falha sem aplicar alterações parciais silenciosas e sem executar migrações automaticamente durante a inicialização normal do serviço.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST permitir que cada ambiente configure separadamente o acesso do backend ao banco PostgreSQL hospedado.
- **FR-002**: A credencial de banco MUST ser fornecida somente ao processo servidor por uma fonte apropriada ao ambiente; MUST NOT ser gravada em arquivos versionados, enviada ao navegador ou incluída em artefatos públicos.
- **FR-003**: A identidade usada em operações regulares MUST ter somente os privilégios necessários para essas operações; uma credencial administrativa MUST NOT ser usada como credencial regular da aplicação.
- **FR-004**: O sistema MUST validar a disponibilidade da persistência e MUST NOT indicar prontidão quando a configuração estiver ausente, inválida ou sem conectividade.
- **FR-005**: Mensagens de erro, logs e respostas de saúde MUST NOT revelar senha, string de conexão, chave administrativa ou outros segredos.
- **FR-006**: A configuração MUST permitir rotação de credenciais sem alteração de código ou inclusão de novos segredos no repositório.
- **FR-007**: Credenciais e valores de teste MUST ser isolados por ambiente; testes automatizados MUST NOT depender de credenciais reais nem acessar a base de produção.
- **FR-008**: Alterações de esquema necessárias MUST ser aplicadas por uma operação de implantação explícita e controlada, não automaticamente na inicialização normal da aplicação.
- **FR-009**: A configuração MUST distinguir uma credencial privada de conexão ao banco de valores publicáveis destinados a um cliente Supabase; valores publicáveis MUST NOT conceder nem substituir acesso direto ao banco.
- **FR-010**: O processo de configuração MUST preservar integralmente caracteres especiais das credenciais e MUST evitar sua exposição durante validação, uso, rotação ou diagnóstico.

### Key Entities *(include if data feature)*

- **Configuração de ambiente**: conjunto de parâmetros públicos e privados que seleciona os serviços e credenciais de um ambiente específico; valores privados são acessíveis somente aos processos autorizados.
- **Credencial de persistência**: identidade e segredo usados pelo serviço servidor para acessar o banco, limitados aos privilégios necessários e sujeitos a rotação.
- **Configuração pública de cliente**: valores não secretos que permitem ao cliente web localizar o projeto Supabase e usar recursos públicos autorizados; não equivale a uma credencial de banco.
- **Estado de prontidão**: indicação observável de que o serviço e suas dependências obrigatórias, incluindo o banco, estão disponíveis, sem incluir dados confidenciais.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% dos testes de configuração válida com o banco de teste disponível, o sistema indica a persistência como pronta em até 60 segundos após a inicialização.
- **SC-002**: Em 100% dos testes com segredo ausente, inválido ou banco indisponível, a prontidão não é anunciada e o diagnóstico identifica a categoria da falha sem revelar qualquer parte do segredo.
- **SC-003**: Uma inspeção do repositório, artefatos públicos, respostas de saúde e logs de teste encontra zero senhas, strings de conexão completas ou chaves administrativas.
- **SC-004**: Um responsável consegue atualizar a credencial de um ambiente sem alterar o código ou incluir arquivos com segredos no controle de versão.
- **SC-005**: A documentação permite que pelo menos 4 de 5 pessoas responsáveis por uma configuração de teste identifiquem corretamente qual valor é privado para o servidor e qual é publicável para o cliente.

## Assumptions

- A aplicação já possui uma API responsável pelo acesso persistente; portanto, a string direta de conexão PostgreSQL é a opção aplicável à configuração do banco neste projeto.
- A opção de biblioteca cliente e a chave publicável servem à integração Supabase do web, especialmente aos recursos de identidade já existentes; não configuram a conexão PostgreSQL do backend. As bibliotecas correspondentes já estão presentes no projeto.
- O valor publicável informado não é uma senha de banco. Ainda assim, somente deve ser disponibilizado ao cliente quando necessário; uma chave administrativa nunca deve ser exposta.
- A senha de banco incluída na solicitação foi compartilhada em uma conversa e MUST ser rotacionada antes de qualquer uso; ela não deve ser copiada para a especificação, para arquivos de configuração ou para o repositório.
- O acesso de execução usa uma credencial dedicada e de privilégio mínimo. Aplicar migrações, quando necessário, é uma operação controlada e separada do tráfego normal da aplicação.
- Ambientes de desenvolvimento e teste usam credenciais e bancos isolados; produção usa o mecanismo de armazenamento de segredos oferecido pelo ambiente de implantação.
