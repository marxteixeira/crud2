# Implementation Plan: Fechamento da fase de identidade e perfis

**Branch**: `005-fechamento-identidade-perfis` | **Date**: 2026-10-25 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/005-fechamento-identidade-perfis/spec.md`

## Summary

Estabelecer uma decisão reproduzível sobre a conclusão da fase de identidade e perfis e sua prontidão para a fase de localização e produtos. O trabalho confronta as tarefas ainda abertas nas fases 003 e 004 com resultados atuais de build, testes automatizados, validações operacionais, desempenho e usabilidade. A avaliação não presume aprovação a partir da existência de código: falhas e evidências ausentes mantêm a transição bloqueada. O resultado é um registro de pendências, evidências e decisão, sem alteração de código funcional ou exposição de segredos e dados pessoais.

## Technical Context

**Language/Version**: Documentação Markdown; PowerShell no Windows para execução de comandos; .NET 8 e Node.js 20 conforme os quickstarts existentes.

**Primary Dependencies**: Artefatos Spec Kit do repositório; testes xUnit existentes para API; scripts npm existentes para lint, testes e build do web.

**Storage**: Arquivos de especificação e resultados agregados no repositório; banco PostgreSQL isolado é necessário para a validação operacional, sem acesso à produção.

**Testing**: `dotnet build` e `dotnet test` da solução API; `npm run lint`, `npm test` e `npm run build`; cenários manuais no quickstart de identidade; medição de desempenho com 1.000 contas e pelo menos 100 requisições; roteiro de usabilidade com pelo menos cinco participantes.

**Target Platform**: Ambiente local Windows e CI do repositório, com serviço PostgreSQL descartável e projeto Supabase isolado somente para cenários manuais que o exijam.

**Project Type**: Avaliação e documentação de prontidão dentro do monorepo existente; não introduz aplicação, endpoint ou serviço novo.

**Performance Goals**: Nenhuma meta de desempenho para o processo de avaliação. O critério herdado da funcionalidade é p95 de até 3 segundos para listagem de 1.000 contas, após aquecimento e pelo menos 100 requisições.

**Constraints**: Não executar validações contra produção; não incluir credenciais, tokens, senhas, connection strings nem dados pessoais nos resultados. Classificar separadamente falha funcional, configuração inadequada e dependência indisponível. A fase seguinte só pode ser liberada quando todos os critérios obrigatórios forem aprovados.

**Scale/Scope**: Uma fase de identidade/perfis, a prontidão da configuração segura do banco e a decisão de entrada para localização e produtos. A avaliação registra evidências e bloqueios; correções funcionais descobertas são encaminhadas como trabalho explícito, não presumidas como parte desta avaliação.

## Constitution Check

**Gate antes da pesquisa: PASS**

- **Camadas e monorepo (I, II, IV)**: Escopo documental; não cria projetos, camadas, dependências entre camadas ou diretórios de topo.
- **Testes obrigatórios (III)**: Usa as suítes existentes como critérios de evidência; qualquer correção de código identificada depois deve preservar e passar os testes aplicáveis antes de integrar.
- **Tipagem, erros e acessibilidade (V, VI)**: Não altera aplicações, endpoints ou componentes. Registros da avaliação não incluem dados sensíveis.
- **Segurança e dados (VII)**: Usa somente ambiente de teste isolado para banco/provedor; não reproduz nem registra segredos ou dados pessoais; mantém a exigência de evidência para credenciais, permissões e migrações seguras.
- **Fluxo Spec Kit**: Mantém artefatos no diretório sequencial `specs/005-fechamento-identidade-perfis/`.

Não há violações que exijam justificativa ou esclarecimentos bloqueantes para o desenho.

## Project Structure

### Documentation (this feature)

```text
specs/005-fechamento-identidade-perfis/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── tasks.md                 # Criado posteriormente por /speckit-tasks
```

### Existing evidence sources

```text
specs/
├── 002-implementation-roadmap/spec.md
├── 003-identidade-perfis/
│   ├── tasks.md
│   └── quickstart.md
└── 004-supabase-database-config/
    ├── tasks.md
    └── quickstart.md

tests/
├── api/
└── web/
```

**Structure Decision**: Manter a avaliação e o registro de decisão nos artefatos de documentação da feature. Não criar contrato de API: esta funcionalidade é uma avaliação interna sem interface pública nem integração de sistema.

## Phase 0: Research

Registrar em [research.md](research.md) como determinar a fonte de verdade para os critérios, interpretar os resultados automatizados e tratar evidências manuais/operacionais ainda não coletadas. Nenhuma tecnologia nova ou pesquisa externa é necessária: os critérios e comandos já existem nos artefatos 002, 003 e 004.

## Phase 1: Design & Validation Guide

- Definir em [data-model.md](data-model.md) os campos e estados conceituais de critério, evidência, pendência e decisão, sem criar persistência de aplicação.
- Não gerar contratos externos: não existe endpoint nem interface pública nesta feature.
- Documentar em [quickstart.md](quickstart.md) os comandos de validação, ambiente isolado, evidências necessárias e condições para liberar ou bloquear a próxima fase.

**Constitution Check pós-design: PASS** — os artefatos seguem a estrutura existente, introduzem apenas documentação, preservam os gates automatizados obrigatórios e não incluem dados confidenciais.

## Baseline de prontidão observado

- Build da solução API: aprovado em execução sequencial.
- Testes API: 37 aprovados e 26 falhos em 63; causas reportadas incluem configuração de connection string ausente em testes, esquema de autenticação duplicado e divergência entre host esperado e configuração ativa no teste de conectividade.
- Testes de domínio: 12 aprovados.
- Web: lint, 18 testes em 6 arquivos e build aprovados.
- Identidade/perfis: tarefas T036, T037, T039 e T040 permanecem abertas em `specs/003-identidade-perfis/tasks.md`.
- Configuração do banco: T018 permanece aberta em `specs/004-supabase-database-config/tasks.md`, com validação local anterior parcial por falta de PostgreSQL local.

Este baseline não satisfaz os critérios de liberação. As causas reportadas precisam ser reproduzidas e resolvidas ou classificadas com evidência atual; os cenários manual, de desempenho e de usabilidade também precisam de resultados registrados.

## Complexity Tracking

Nenhuma violação da constituição ou complexidade arquitetural adicional.
