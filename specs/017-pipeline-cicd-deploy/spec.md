# Especificação da Feature: Pipeline CI/CD de Deploy (Front na Vercel, API no Render)

**Feature Branch**: `017-pipeline-cicd-deploy`

**Created**: 2026-10-31

**Status**: Draft

**Input**: User description: "o objetivo é criar uma pipeline para subir essas aplicações para o servidor. Para o front, será hospedado na vercel. Para a API, será hospedado no render. Esse monorepo está utilizando github. Analisar o que é preciso ser feito, para criar esteiras de ci/cd."

## Situação Atual (análise)

- O repositório já possui uma esteira de integração contínua (build, lint e testes do front; restore, build e testes da API) executada em pushes para `main` e em pull requests. Não existe etapa de entrega (CD) nem verificação pós-deploy.
- O front (`src/apps/web`) será importado na Vercel com diretório raiz do monorepo apontando para essa pasta; a API (`src/services/api`) será criada como serviço web no Render a partir do mesmo repositório, branch `main`.
- Cada plataforma pode disparar deploy automático a cada push, mas isso ignoraria o resultado dos testes da esteira e não coordenaria front e API. É necessário definir quando e em que ordem cada aplicação é publicada.
- A API depende de configurações por ambiente (conexão com banco, autenticação do provedor de identidade, origens permitidas para o front, URL de redirecionamento) que hoje têm valores apenas de desenvolvimento local; o front precisa conhecer o endereço público da API.
- O banco de dados e a evolução de seu esquema (migrações) precisam de uma regra clara de quando são aplicados em relação ao deploy da API.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Publicar automaticamente após validação (Priority: P1)

Como mantenedor, quero que a integração de código na branch principal só publique front e API se todas as verificações (lint, testes, build) passarem, para que nenhuma versão quebrada chegue ao ambiente de produção.

**Why this priority**: É o núcleo do pedido; sem essa trava, o deploy automático das plataformas publicaria código não validado.

**Independent Test**: Integrar uma alteração na branch principal com verificações passando e confirmar que as duas aplicações ficam disponíveis com a nova versão; repetir com um teste falhando e confirmar que nada é publicado.

**Acceptance Scenarios**:

1. **Given** uma alteração integrada na branch principal com todas as verificações aprovadas, **When** a esteira conclui, **Then** a nova versão do front e da API é publicada nos respectivos ambientes de produção sem passo manual.
2. **Given** uma alteração cujas verificações falham, **When** a esteira executa, **Then** nenhuma aplicação é publicada e o mantenedor é notificado da falha.
3. **Given** uma alteração que modifica apenas o front (ou apenas a API), **When** a esteira executa, **Then** apenas a aplicação afetada é republicada.

---

### User Story 2 - Pré-visualização em pull requests (Priority: P2)

Como revisor, quero uma versão de pré-visualização do front para cada pull request, para validar mudanças de interface antes de integrá-las.

**Why this priority**: Aumenta a qualidade das revisões, mas não bloqueia o envio a produção.

**Independent Test**: Abrir um pull request que altera o front e verificar que um endereço de pré-visualização é divulgado no próprio pull request.

**Acceptance Scenarios**:

1. **Given** um pull request que altera o front, **When** as verificações passam, **Then** um link de pré-visualização é exibido no pull request.
2. **Given** um pull request de origem externa ou sem acesso a segredos, **When** a esteira executa, **Then** nenhum segredo é exposto e a pré-visualização não é publicada.

---

### User Story 3 - Configuração segura por ambiente (Priority: P2)

Como mantenedor, quero que segredos e configurações de cada ambiente (banco, autenticação, origem permitida do front, endereço da API) fiquem armazenados fora do código, para que produção funcione sem expor credenciais.

**Why this priority**: Sem essas configurações a aplicação publicada não funciona; vazar segredos é risco crítico.

**Independent Test**: Publicar em produção e confirmar que o login e as chamadas front→API funcionam, e que nenhum segredo aparece no repositório ou nos logs da esteira.

**Acceptance Scenarios**:

1. **Given** as configurações de produção cadastradas nas plataformas, **When** as aplicações são publicadas, **Then** o front se comunica com a API pública e a API aceita somente a origem do front de produção.
2. **Given** uma configuração obrigatória ausente, **When** a esteira ou a aplicação inicia, **Then** a falha é identificada de forma clara antes de a versão receber tráfego.

---

### User Story 4 - Verificação pós-deploy e reversão (Priority: P3)

Como mantenedor, quero confirmar automaticamente que a versão publicada está saudável e poder voltar à versão anterior rapidamente, para reduzir o impacto de um deploy problemático.

**Why this priority**: Reduz risco operacional, mas a feature já entrega valor sem ela.

**Independent Test**: Após um deploy, verificar que a esteira consulta a saúde da API e a página inicial do front e registra o resultado; simular falha e executar o procedimento de reversão documentado.

**Acceptance Scenarios**:

1. **Given** um deploy concluído, **When** a verificação pós-deploy executa, **Then** o resultado (saudável ou não) é registrado e falhas notificam o mantenedor.
2. **Given** uma versão defeituosa em produção, **When** o mantenedor segue o procedimento de reversão, **Then** a versão anterior volta a atender usuários em poucos minutos.

---

### Edge Cases

- Deploys simultâneos de commits sucessivos: apenas o mais recente deve prevalecer, sem publicar versões fora de ordem.
- Mudança que exige alteração de esquema do banco junto com mudança na API: a ordem (migração antes/depois do deploy) deve ser definida e não deixar a API incompatível com o banco.
- API hospedada em plano que "hiberna" por inatividade: a verificação pós-deploy deve tolerar a primeira resposta lenta.
- Alteração que muda front e API ao mesmo tempo: a API deve ser publicada antes do front quando houver mudança de contrato.
- Falha de uma das plataformas (indisponibilidade): a esteira deve falhar com mensagem clara e permitir nova tentativa manual.
- Alterações apenas em documentação ou especificações não devem disparar publicação.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A esteira MUST executar as verificações existentes (lint, testes e build do front; restore, build e testes da API) antes de qualquer publicação.
- **FR-002**: A publicação em produção MUST ocorrer somente a partir da branch principal e somente se todas as verificações obrigatórias passarem.
- **FR-003**: O front MUST ser publicado na Vercel e a API no Render, ambos a partir deste monorepo e dos respectivos diretórios.
- **FR-004**: O sistema MUST publicar somente a aplicação cujos arquivos (ou dependências compartilhadas) foram alterados.
- **FR-005**: Alterações exclusivamente em documentação, especificações ou testes não relacionados MUST NOT disparar publicação.
- **FR-006**: O deploy automático nativo das plataformas MUST ser coordenado com a esteira, de modo que nenhuma publicação ocorra sem aprovação das verificações.
- **FR-007**: Quando houver mudança nas duas aplicações, a API MUST ser publicada e validada antes do front.
- **FR-008**: Pull requests que alteram o front MUST gerar uma pré-visualização identificável no próprio pull request, sem expor segredos.
- **FR-009**: Todos os segredos e configurações de ambiente (conexão com banco, autenticação, origens permitidas, endereço da API, URL de redirecionamento) MUST ser mantidos nos cofres das plataformas/repositório, nunca no código.
- **FR-010**: A configuração de produção da API MUST permitir apenas a origem do front de produção e usar a URL de redirecionamento de produção.
- **FR-011**: A esteira MUST executar uma verificação pós-deploy de saúde da API e de disponibilidade do front, registrando o resultado.
- **FR-012**: Falhas de verificação ou de deploy MUST notificar o mantenedor com indicação da etapa que falhou.
- **FR-013**: Execuções concorrentes para a mesma branch MUST ser serializadas ou canceladas, garantindo que a versão mais recente prevaleça.
- **FR-014**: O projeto MUST documentar o procedimento de reversão para a versão anterior de cada aplicação.
- **FR-015**: O projeto MUST documentar a configuração inicial necessária nas plataformas (diretório raiz, comandos de build/start, variáveis, ligação com o repositório) e a regra de aplicação das migrações de banco em relação ao deploy da API.
- **FR-016**: A API MUST expor um ponto de verificação de saúde utilizável pela esteira e pela plataforma.

### Key Entities

- **Esteira (pipeline)**: Conjunto de etapas automatizadas acionadas por eventos do repositório (pull request, push na principal), com resultado aprovado/reprovado.
- **Ambiente**: Destino de publicação (produção; pré-visualização para pull requests) com suas configurações e segredos próprios.
- **Versão publicada (release)**: Resultado de um deploy associado a um commit, com estado (em andamento, saudável, falho) e possibilidade de reversão.
- **Configuração/Segredo**: Valor dependente de ambiente, armazenado fora do código e disponibilizado a cada aplicação.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das publicações em produção ocorrem após aprovação de todas as verificações obrigatórias.
- **SC-002**: Da integração na branch principal até a nova versão disponível em produção, decorrem no máximo 15 minutos em condições normais.
- **SC-003**: Publicar uma nova versão exige zero passos manuais após a integração do código.
- **SC-004**: 100% dos pull requests que alteram o front exibem link de pré-visualização em até 10 minutos.
- **SC-005**: Nenhum segredo ou credencial é encontrado no repositório ou nos logs públicos da esteira.
- **SC-006**: Após cada deploy, o resultado da verificação de saúde fica disponível em até 5 minutos e falhas notificam o mantenedor.
- **SC-007**: Um mantenedor consegue restaurar a versão anterior de qualquer aplicação em até 10 minutos seguindo a documentação.
- **SC-008**: Um novo mantenedor consegue reproduzir a configuração das plataformas apenas com a documentação, sem ajuda externa.

## Assumptions

- Serão usados os planos gratuitos/de entrada das plataformas (Vercel Hobby; Render com serviço web de entrada), aceitando limitações como hibernação da API por inatividade; planos pagos estão fora do escopo.
- Há um único ambiente de produção; ambiente de homologação separado está fora do escopo desta versão.
- O front será importado na Vercel com diretório raiz `src/apps/web` e a API no Render a partir da branch `main`, conforme as telas de configuração já iniciadas.
- O banco de dados e o provedor de identidade já existem como serviços gerenciados e são reutilizados; criar ou provisionar o banco está fora do escopo.
- As migrações de banco serão aplicadas de forma controlada pela esteira ou procedimento documentado antes da publicação da API; o mecanismo exato será definido no planejamento.
- O repositório GitHub é o único gatilho de eventos; a esteira de CI existente (`.github/workflows/ci.yml`) será estendida e reutilizada.
- O mantenedor receberá notificações pelos recursos nativos do GitHub (e-mail/status de checks); canais adicionais estão fora do escopo.
- Domínio personalizado, CDN avançada, monitoramento contínuo e observabilidade completa estão fora do escopo.
