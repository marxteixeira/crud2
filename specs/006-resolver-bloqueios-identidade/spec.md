# Feature Specification: Resolver bloqueios do fechamento de Identidade e perfis

**Feature Branch**: `006-resolver-bloqueios-identidade`
**Created**: 2026-05-05
**Status**: Draft
**Input**: "Verifique o que falta fazer, inclusive tarefas manuais, para resolver as tarefas que falharam na fase de identidade e perfis e impedem avançar para a próxima fase. Nos testes manuais em localhost a integração com Supabase funciona: administrador criado e convite enviado a um operador. Resolva ou mostre como resolver."

## Contexto e diagnóstico

A decisão de prontidão em `specs/005-fechamento-identidade-perfis/readiness-report.md` é **bloqueada**. A investigação mostrou que as 26 falhas da suíte automatizada da API têm três causas, nenhuma delas um defeito funcional comprovado de identidade:

| Grupo | Causa | Testes afetados |
|-------|-------|-----------------|
| A | Ambiente de teste sem configuração de conexão com o banco | Autenticação, gestão de usuários, erros globais, logs estruturados |
| B | Registro duplicado do esquema de autenticação "Bearer" na inicialização em ambiente de teste | Autorização por perfil |
| C | Teste de conectividade esperando banco local, enquanto o ambiente aponta para outro destino | Conectividade de persistência |

Além disso, permanecem pendentes: cenários manuais (003 T036), teste de desempenho (T039), teste de usabilidade com pessoas (T040) e validação do guia da configuração de banco (004 T018). A evidência manual já informada (administrador criado, convite enviado) cobre parte de T036.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Suíte automatizada verde e reproduzível (Priority: P1)

Como responsável técnico, quero que todos os testes automatizados passem em qualquer máquina limpa e no CI, sem credenciais reais, para provar que a fase de identidade está estável.

**Why this priority**: Sem testes verdes a constituição proíbe integrar e avançar.

**Independent Test**: Executar os gates completos (compilação, testes de domínio, API e web, lint, build) em ambiente limpo e obter 0 falhas.

**Acceptance Scenarios**:

1. **Given** uma máquina sem segredos, **When** a suíte da API é executada, **Then** todos os testes passam sem acessar o Supabase real.
2. **Given** o ambiente de CI com banco descartável, **When** os gates rodam, **Then** todos passam, incluindo o teste de conectividade.
3. **Given** um banco local ausente, **When** o desenvolvedor executa os testes, **Then** a documentação indica como subir um banco descartável (contêiner) ou os testes que o exigem informam claramente o motivo.

---

### User Story 2 - Evidências manuais e de desempenho/usabilidade registradas (Priority: P1)

Como responsável pelo produto, quero um roteiro claro das tarefas manuais restantes e o registro sem dados pessoais das já realizadas, para fechar T036, T039, T040 e T018.

**Why this priority**: São critérios de saída da fase e dependem de pessoas.

**Independent Test**: Cada critério no relatório de prontidão tem status `aprovado` com evidência datada, ou `pendente` com responsável e passo exato.

**Acceptance Scenarios**:

1. **Given** o teste manual já feito (administrador criado, convite enviado), **When** o relatório é atualizado, **Then** esses cenários constam como aprovados sem e-mails, tokens ou segredos.
2. **Given** os cenários manuais restantes (mudança de perfil com sessão aberta, inativação, revogação de sessão, troca de e-mail, convite duplicado, proteção do último administrador, identidade inativa), **When** executados, **Then** resultado e data são registrados.
3. **Given** 1.000 contas, **When** o teste de desempenho roda, **Then** o p95 da listagem/consulta fica em até 3 s.
4. **Given** pelo menos 5 participantes, **When** a tarefa de convidar e gerenciar usuários é executada, **Then** a taxa de conclusão e o feedback são registrados.

---

### User Story 3 - Decisão final e liberação da próxima fase (Priority: P2)

Como responsável pelo produto, quero a decisão final atualizada para `pronto` quando todos os critérios forem aprovados, liberando "Localização e produtos".

**Independent Test**: Relatório de prontidão com todos os critérios aprovados e decisão `pronto`; tarefas 003/004 marcadas somente com evidência.

**Acceptance Scenarios**:

1. **Given** todos os critérios aprovados, **When** o relatório é revisado, **Then** a decisão muda para `pronto` e a próxima fase é indicada.
2. **Given** algum critério pendente, **When** o relatório é revisado, **Then** a decisão permanece `bloqueada` e lista o que falta.

### Edge Cases

- Banco descartável indisponível no CI: o job deve falhar com mensagem clara, não silenciosamente.
- Teste de usabilidade com menos de 5 participantes: critério permanece pendente.
- Evidência contendo dados pessoais ou segredos: deve ser rejeitada e anonimizada.
- Correção de teste que reduza a cobertura de segurança (por exemplo, desativar validação): não permitida.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A suíte automatizada da API MUST executar sem credenciais reais e sem dependência do Supabase de produção, com configuração de teste autossuficiente.
- **FR-002**: A inicialização em ambiente de teste MUST NOT registrar o mesmo esquema de autenticação mais de uma vez, mantendo a validação real de tokens no ambiente normal.
- **FR-003**: O teste de conectividade MUST usar um banco descartável configurável e MUST indicar claramente quando ele não está disponível.
- **FR-004**: Os gates completos (compilação, testes de domínio/API/web, lint, build) MUST ser reexecutados e registrados com resultado atual.
- **FR-005**: O teste de desempenho com 1.000 contas e pelo menos 100 requisições MUST existir e ser executado, com p95 ≤ 3 s.
- **FR-006**: O relatório MUST registrar a evidência manual já realizada e listar, com passos, as tarefas manuais restantes (cenários de T036, usabilidade T040, validação T018).
- **FR-007**: Evidências MUST NOT conter segredos, tokens, strings de conexão ou dados pessoais.
- **FR-008**: As tarefas de origem (003 T036/T037/T039/T040; 004 T018) MUST só ser marcadas como concluídas com evidência aprovada.
- **FR-009**: A decisão final MUST ser `pronto` somente se todos os critérios estiverem aprovados; caso contrário, `bloqueada` com pendências.

### Key Entities

- **Critério de saída**: item exigido para encerrar a fase (status, evidência, responsável, data).
- **Evidência**: resultado verificável e anonimizado de execução automatizada ou manual.
- **Decisão de prontidão**: `pronto` ou `bloqueada`, com pendências.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos testes automatizados passam (0 falhas, hoje 26) em máquina limpa e no CI.
- **SC-002**: 100% dos critérios de saída da fase têm evidência aprovada ou pendência com responsável e passo exato.
- **SC-003**: O p95 de consulta com 1.000 contas é de no máximo 3 segundos.
- **SC-004**: Pelo menos 5 participantes completam o teste de usabilidade, com taxa de conclusão registrada.
- **SC-005**: Zero segredos ou dados pessoais nos artefatos de evidência.
- **SC-006**: Um responsável decide em menos de 10 minutos, lendo o relatório, se a próxima fase pode começar.

## Assumptions

- A integração com o Supabase foi validada manualmente em localhost pelo usuário (administrador criado, convite enviado).
- O Docker está disponível para um banco descartável local; o CI já usa um serviço de banco descartável.
- As correções limitam-se a configuração e fixtures de teste; nenhuma regra de negócio muda.
- Participantes do teste de usabilidade serão recrutados pelo responsável do produto.

## Out of Scope

- Funcionalidades da fase "Localização e produtos".
- Alterações de regras de perfis e permissões.
