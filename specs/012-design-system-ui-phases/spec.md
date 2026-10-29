# Feature Specification: Plano por fases de adoção do Design System nas telas

**Feature Branch**: `012-design-system-ui-phases`

**Created**: 2026-10-28

**Status**: Draft

**Input**: User description: "Adicionei na constitution uma nova orientação para o design system do sistema, onde cada pasta dentro da pasta 05-design-system contém um print e um arquivo html do protótipo. Avalie os protótipos e prepare as fases para implementação, relacionando cada tela com os protótipos respectivos. No final precisa ter um plano por fases, para a implantação com respectivos controles de execução."

## Contexto avaliado

Fonte visual: [design-systen.md](../../docs/05-design-system/design-systen.md) (tokens de cor, tipografia e espaçamento) e sete protótipos em `docs/05-design-system/<pasta>/`, cada um com `screen.png` (print) e `code.html` (protótipo).

Telas já existentes no sistema (funcionais, sem identidade visual unificada): login, ativação de convite, usuários, estantes/prateleiras, produtos, fornecedores e vínculos. Esta feature **não cria novas regras de negócio**: ela define a ordem e o controle para alinhar cada tela existente ao seu protótipo.

### Mapa tela ↔ protótipo

| ID | Tela do sistema (rota) | Pasta do protótipo | Arquivos de referência | Observações de avaliação |
|----|------------------------|--------------------|------------------------|--------------------------|
| T1 | Login / boas-vindas (`/login`, `/activate`, `/auth/confirm`) | `login_boas_vindas` | `screen.png`, `code.html` | Painel de boas-vindas com mensagem de valor e formulário "Entrar no sistema" com campo de senha de acesso. Ativação de convite reaproveita o mesmo layout. |
| T2 | Gerenciar usuários (`/users`) | `gerenciar_usuarios` | `screen.png`, `code.html` | Tabela Usuário/Perfil de Acesso/Situação/Ações, filtros por situação e perfil, diálogo "Convidar novo usuário" com escolha entre Administrador e Operador. |
| T3 | Estantes e prateleiras (`/locations`, prateleiras da estante) | `estantes_e_prateleiras` | `screen.png`, `code.html` | Seleção da estante em foco, lista por código/descrição, ação "Cadastrar nova estante" e painel de prateleiras da estante selecionada. |
| T4 | Catálogo de produtos (`/products`) | `catalogo_de_produtos` | `screen.png`, `code.html` | Filtros (estante, prateleira, situação, limpar), tabela com localização física, fornecedor, quantidade, preço unitário e ações, paginação Anterior/Próxima. |
| T5 | Cadastro de produto (diálogo em `/products`, `/products/new`) | `modal_cadastrar_produto` | `screen.png`, `code.html` | Nome, estante física, nível/prateleira, fornecedor associado, quantidade, preço. |
| T6 | Gestão de fornecedores (`/suppliers`) | `gestao_de_fornecedores` | `screen.png`, `code.html` | Tabela Fornecedor/Documento/Contato/Itens associados/Situação/Ações e confirmação de inativação ("Alterar Situação" / "Confirmar Inativação"). |
| T7 | Cadastro de fornecedor (diálogo em `/suppliers`, `/suppliers/new`) | `modal_cadastrar_fornecedor` | `screen.png`, `code.html` | Formulário de novo fornecedor sobre o diretório de fornecedores, com ação de consulta. |

### Divergências protótipo × negócio (não implementar sem decisão)

Elementos presentes nos protótipos sem respaldo nos documentos de negócio vigentes. Ficam **fora do escopo** até decisão registrada:

- D1: "Exportar CSV" (T7) — exportação não consta no MVP.
- D2: campo "Categoria" e botão "Gerar Auto" (T5) — categoria e geração automática de código não constam no modelo de produto.
- D3: qualquer dado ou texto ilustrativo do protótipo (nomes, valores, logotipos) — não é conteúdo do sistema.
- D4: referência `design-system.md` em `.github/copilot-instructions.md` aponta para um arquivo cujo nome real é `design-systen.md`; a correção do nome/link é pendência documental.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Fundação visual compartilhada (Priority: P1)

Como pessoa responsável pelo desenvolvimento, quero uma base visual única (cores, tipografia, espaçamento, estrutura de página e navegação, estados padrão) derivada do design system, para que todas as telas sigam o mesmo padrão sem retrabalho.

**Why this priority**: Todas as telas dependem dela; sem ela cada fase reinventaria o visual.

**Independent Test**: Aplicar a base a uma página existente e comparar com o print de qualquer protótipo quanto a cores, tipografia, estrutura e navegação.

**Acceptance Scenarios**:

1. **Given** o design system documentado, **When** a base visual é entregue, **Then** cores, tipografia e espaçamentos usados em qualquer tela coincidem com os valores do documento.
2. **Given** um usuário autenticado, **When** navega entre telas, **Then** encontra a mesma estrutura de navegação e cabeçalho em todas, com o item atual destacado.
3. **Given** qualquer tela, **When** ocorre carregamento, lista vazia ou erro, **Then** o estado é exibido no padrão visual comum.

### User Story 2 - Telas de acesso e usuários alinhadas ao protótipo (Priority: P1)

Como visitante e como Administrador, quero login, ativação de convite e gerenciamento de usuários com a aparência dos protótipos T1 e T2, mantendo todas as regras de acesso atuais.

**Why this priority**: É a porta de entrada do sistema e valida a base visual em formulários e tabelas.

**Independent Test**: Comparar cada tela com seu print e executar os fluxos de entrada, ativação, convite e alteração de situação, confirmando que o comportamento não mudou.

**Acceptance Scenarios**:

1. **Given** a tela de login, **When** comparada a T1, **Then** estrutura, textos de valor e campos correspondem e o acesso continua funcionando.
2. **Given** a lista de usuários, **When** comparada a T2, **Then** colunas, filtros e diálogo de convite correspondem ao protótipo e respeitam as permissões do perfil.

### User Story 3 - Localizações, produtos e fornecedores alinhados aos protótipos (Priority: P2)

Como Operador ou Administrador, quero estantes/prateleiras, catálogo e cadastro de produtos, fornecedores e seus diálogos com a aparência dos protótipos T3 a T7, sem perder nenhuma função existente.

**Why this priority**: São as telas de uso diário e dependem da base visual e do padrão de tabelas e diálogos.

**Independent Test**: Para cada tela, comparar com o print e executar os fluxos de cadastro, busca/filtro, edição e inativação já existentes.

**Acceptance Scenarios**:

1. **Given** cada tela T3 a T7, **When** comparada ao seu protótipo, **Then** layout, hierarquia, rótulos e ações principais correspondem, exceto divergências D1 a D3.
2. **Given** os fluxos existentes de cadastro, filtro, edição e inativação, **When** executados após o alinhamento, **Then** produzem os mesmos resultados de antes.

### User Story 4 - Controle de execução por fase (Priority: P1)

Como responsável pelo projeto, quero um plano por fases com controle de execução, para saber o que está pronto, em andamento ou bloqueado e quando uma fase pode ser considerada concluída.

**Why this priority**: É o pedido central: transformar a avaliação em um plano executável e acompanhável.

**Independent Test**: Abrir o plano e, para cada fase, identificar telas, protótipos, dependências, critérios de saída, responsável e status atual.

**Acceptance Scenarios**:

1. **Given** o plano, **When** uma fase é consultada, **Then** mostra telas e protótipos relacionados, dependências, entregáveis, critérios de saída e status.
2. **Given** uma fase concluída, **When** o controle é atualizado, **Then** registra data, evidência (comparação com o print e testes) e liberação da fase seguinte.
3. **Given** uma divergência protótipo × negócio, **When** encontrada, **Then** é registrada no controle com decisão pendente, sem bloquear fases não relacionadas.

### Edge Cases

- Protótipo exibe dado, campo ou ação sem função no sistema (ver D1 a D3): não implementar; registrar.
- Tela em uso real é alterada: nenhuma regra de negócio, permissão ou contrato muda por causa do visual.
- Telas de detalhe/edição sem protótipo próprio (ex.: edição de produto, detalhe de fornecedor, vínculos): seguem os padrões da base visual e do protótipo mais próximo, registradas como "derivadas".
- Telas pequenas (celular): protótipos são desktop; adaptação segue o design system e é validada na fase final.
- Ação restrita ao Administrador permanece oculta ou desabilitada para o Operador.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O plano MUST relacionar cada tela do sistema a exatamente um protótipo de origem (pasta, print e HTML) ou declará-la "derivada" com o protótipo-base.
- **FR-002**: O plano MUST organizar a adoção em fases ordenadas: F0 base visual; F1 acesso (T1); F2 usuários (T2); F3 estantes e prateleiras (T3); F4 produtos (T4 e T5); F5 fornecedores (T6 e T7); F6 validação transversal.
- **FR-003**: Cada fase MUST declarar objetivo, telas e protótipos, dependências, entregáveis, critérios de entrada e de saída verificáveis.
- **FR-004**: Cada tela entregue MUST ser comparada ao print do protótipo respectivo, com evidência registrada (captura da tela implementada lado a lado e lista de diferenças aceitas).
- **FR-005**: O alinhamento visual MUST NOT alterar regras de negócio, permissões, validações ou fluxos existentes; testes existentes MUST continuar passando.
- **FR-006**: Cores, tipografia e espaçamento MUST vir do design system documentado; valores avulsos fora dele exigem registro como exceção.
- **FR-007**: Toda tela MUST tratar estados de carregamento, vazio, erro e sucesso no padrão visual comum.
- **FR-008**: Elementos dos protótipos sem respaldo nos documentos de negócio (D1 a D3) MUST ficar fora do escopo e constar no controle como decisão pendente.
- **FR-009**: O plano MUST incluir um controle de execução por fase e por tela com: status (não iniciada, em andamento, bloqueada, concluída), responsável, datas de início e conclusão, evidências e pendências.
- **FR-010**: Uma fase só MUST ser marcada concluída quando todos os seus critérios de saída estiverem atendidos e a evidência registrada; a fase seguinte depende da conclusão das suas dependências declaradas.
- **FR-011**: A fase final MUST validar consistência entre todas as telas, uso em celular, contraste e navegação por teclado, e regressão funcional completa.
- **FR-012**: O controle MUST registrar as pendências documentais D4 (referência do design system) e qualquer nova divergência encontrada.

### Key Entities

- **Tela**: página ou diálogo do sistema; atributos: identificador (T1…T7 ou derivada), rota, perfil com acesso, protótipo de origem, status.
- **Protótipo**: pasta em `05-design-system` com print e HTML de referência.
- **Fase**: conjunto delimitado de telas com dependências, entregáveis, critérios de saída e status.
- **Registro de execução**: status, responsável, datas, evidência (comparação com print, testes) e pendências por fase e por tela.
- **Divergência**: diferença entre protótipo e negócio, com decisão pendente ou tomada.

## Plano de implantação por fases e controle de execução

### Visão geral

| Fase | Objetivo | Telas / Protótipos | Depende de | Prioridade |
|------|----------|--------------------|------------|------------|
| F0 | Base visual: tokens do design system, tipografia, estrutura de página e navegação, componentes padrão (botões, campos, tabela, filtros, diálogo, estados) | Todos (referência: `design-systen.md` + prints) | — | P1 |
| F1 | Acesso | T1 `login_boas_vindas` (login, ativação, confirmação) | F0 | P1 |
| F2 | Usuários | T2 `gerenciar_usuarios` (lista, filtros, convite) | F0 | P1 |
| F3 | Estantes e prateleiras | T3 `estantes_e_prateleiras` | F0 | P2 |
| F4 | Produtos | T4 `catalogo_de_produtos`; T5 `modal_cadastrar_produto`; edição/detalhe derivados de T4/T5 | F0, F3 | P2 |
| F5 | Fornecedores | T6 `gestao_de_fornecedores`; T7 `modal_cadastrar_fornecedor`; vínculos derivados de T6 | F0, F4 | P2 |
| F6 | Validação transversal | Todas as telas | F1–F5 | P1 |

### Critérios de saída por fase

| Fase | Critérios de saída |
|------|--------------------|
| F0 | Valores de cor/tipografia/espaçamento conferidos com o documento; navegação comum aplicada a uma tela piloto; estados padrão demonstrados; testes existentes passando. |
| F1 | Telas de acesso conferidas com o print T1; fluxos de entrada, ativação e confirmação inalterados; testes passando. |
| F2 | Lista, filtros e convite conferidos com T2; permissões por perfil inalteradas; testes passando. |
| F3 | Estantes e prateleiras conferidas com T3; cadastro e consulta inalterados; testes passando. |
| F4 | Catálogo e cadastro conferidos com T4/T5 (exceto D2); filtros, paginação, preço e quantidade inalterados; testes passando. |
| F5 | Lista, inativação e cadastro conferidos com T6/T7 (exceto D1); vínculos produto–fornecedor inalterados; testes passando. |
| F6 | Verificação de consistência entre telas, celular, contraste e teclado concluída; regressão completa aprovada; divergências com decisão registrada. |

### Controle de execução (preencher durante a implementação)

Status permitido: Não iniciada · Em andamento · Bloqueada · Concluída.

| Fase | Status | Responsável | Início | Conclusão | Evidência (comparação com print, testes) | Pendências |
|------|--------|-------------|--------|-----------|------------------------------------------|------------|
| F0 | Não iniciada | | | | | |
| F1 | Não iniciada | | | | | |
| F2 | Não iniciada | | | | | |
| F3 | Não iniciada | | | | | |
| F4 | Não iniciada | | | | | |
| F5 | Não iniciada | | | | | |
| F6 | Não iniciada | | | | | |

### Controle por tela

| Tela | Protótipo | Fase | Alinhada ao print | Funções preservadas | Estados (vazio/erro/carregando) | Celular | Status |
|------|-----------|------|-------------------|---------------------|----------------------------------|---------|--------|
| T1 | `login_boas_vindas` | F1 | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T2 | `gerenciar_usuarios` | F2 | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T3 | `estantes_e_prateleiras` | F3 | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T4 | `catalogo_de_produtos` | F4 | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T5 | `modal_cadastrar_produto` | F4 | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T6 | `gestao_de_fornecedores` | F5 | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T7 | `modal_cadastrar_fornecedor` | F5 | ☐ | ☐ | ☐ | ☐ | Não iniciada |

### Controle de divergências

| ID | Descrição | Decisão | Status |
|----|-----------|---------|--------|
| D1 | "Exportar CSV" em T7 | Fora do MVP | Pendente de decisão |
| D2 | "Categoria" e "Gerar Auto" em T5 | Fora do modelo atual | Pendente de decisão |
| D3 | Dados ilustrativos dos protótipos | Não são conteúdo do sistema | Aceita |
| D4 | Nome do arquivo do design system na instrução do repositório | Corrigir link ou nome | Pendente |

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das telas do sistema (7 principais) possuem protótipo de origem ou protótipo-base registrado no plano.
- **SC-002**: 100% das telas entregues têm evidência de comparação com o print respectivo, com todas as diferenças aceitas justificadas.
- **SC-003**: 0 regressões funcionais: todos os testes existentes passam ao final de cada fase.
- **SC-004**: Qualquer pessoa consegue identificar, em menos de 1 minuto, o status, o responsável e a próxima fase pronta para iniciar consultando o controle.
- **SC-005**: Uma pessoa nova consegue concluir as tarefas principais (entrar, convidar usuário, cadastrar estante, produto e fornecedor) em qualquer tela sem instrução, com navegação e estados consistentes em todas.
- **SC-006**: Todas as telas são utilizáveis em tela de celular e por teclado ao final da fase F6.

## Assumptions

- Os protótipos representam o visual desejado para desktop; o comportamento de celular segue o design system e é validado em F6.
- Telas sem protótipo próprio (edição de produto, detalhe de fornecedor, vínculos, prateleira individual) herdam o padrão do protótipo mais próximo.
- A numeração do controle (T1–T7, D1–D4, F0–F6) é a referência de acompanhamento.
- A orientação sobre o design system na constitution é lida como obrigatória para novas telas; este plano a aplica às telas já existentes.
- A fase 011 (importação e reconciliação) não possui telas prototipadas e fica fora desta feature.
- Responsáveis e datas são preenchidos durante a execução, não neste documento.
