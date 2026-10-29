# Tasks: Adoção do Design System nas telas

**Input**: Design documents from `/specs/012-design-system-ui-phases/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/ui-contract.md](./contracts/ui-contract.md), [quickstart.md](./quickstart.md)

**Regras gerais**: nenhuma alteração em `src/apps/web/lib/`, API, permissões ou validações; textos em português; elementos D1/D2 não implementados; testes existentes devem permanecer verdes após cada fase. Protótipos em `docs/05-design-system/<pasta>/` (`screen.png` + `code.html`). Ao concluir cada fase, atualizar [execution-control.md](./execution-control.md).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: pode rodar em paralelo (arquivos diferentes, sem dependência pendente)
- **[Story]**: US1 = base visual (F0), US2 = acesso e usuários (F1, F2), US3 = localizações, produtos e fornecedores (F3–F5), US4 = controle e validação (F6)

## Phase 1: Setup (baseline)

- [X] T001 Registrar o baseline: executar `npm run lint` e `npm test` na raiz e anotar o resultado (testes passando/falhando pré-existentes) em `specs/012-design-system-ui-phases/execution-control.md`
- [ ] T002 [P] Capturar telas atuais (1280 px) das rotas `/login`, `/users`, `/locations`, `/products`, `/suppliers` para comparação antes/depois, salvando em `specs/012-design-system-ui-phases/evidence/before/`

---

## Phase 2: User Story 1 — Base visual compartilhada (F0) (Priority: P1) 🎯 bloqueia as demais

**Goal**: tokens, fontes, componentes e layout comuns derivados do design system.

**Independent Test**: aplicar a base a uma página piloto e comparar cores, tipografia, navegação e estados com os prints.

- [X] T003 [US1] Mapear em `src/apps/web/tailwind.config.ts` (`theme.extend`) as cores, `fontFamily` (Plus Jakarta Sans / Manrope), `fontSize` (display, headline-*, title-md, body-*, label-*, caption), `borderRadius` e `spacing` (gutter, margin, space-*) de `docs/05-design-system/design-systen.md`, usando `primary-container #1e40af` como cor de ação (D5); incluir `./components/**` e `./app/**` em `content`
- [X] T004 [US1] Carregar as fontes com `next/font/google` (`display: "swap"`) em `src/apps/web/app/layout.tsx` e definir base (cor de fundo `background`, texto `on-surface`) em `src/apps/web/app/globals.css`
- [X] T005 [P] [US1] Criar `src/apps/web/components/ui/Button.tsx` (primário, secundário, perigo; foco visível; estado desabilitado/carregando)
- [X] T006 [P] [US1] Criar `src/apps/web/components/ui/Field.tsx` (texto, senha, seleção; rótulo associado; erro com `aria-describedby`)
- [X] T007 [P] [US1] Criar `src/apps/web/components/ui/StatusBadge.tsx` (Ativo verde `tertiary`, Inativo neutro)
- [X] T008 [P] [US1] Criar `src/apps/web/components/ui/PageHeader.tsx` (título `headline-*` + ação principal à direita)
- [X] T009 [P] [US1] Criar `src/apps/web/components/ui/DataTable.tsx` (cabeçalho, linhas, coluna de ações, rodapé de paginação Anterior/Próxima; sem estado de filtro interno — filtros seguem na URL)
- [X] T010 [P] [US1] Criar `src/apps/web/components/ui/Dialog.tsx` baseado no elemento nativo `dialog` (foco preso, Esc fecha, Cancelar/Confirmar)
- [X] T011 [P] [US1] Criar `src/apps/web/components/ui/StateMessages.tsx` (carregando, vazio, erro, sucesso)
- [X] T012 [US1] Reestilizar `src/apps/web/components/AuthenticatedNavigation.tsx` conforme navegação dos protótipos (item atual destacado, ícones SVG com `aria-hidden`), preservando a lógica de sessão, perfil e logout existente
- [X] T013 [US1] Criar layout autenticado compartilhado aplicando a navegação e a margem/gutter do design system, e usá-lo em `src/apps/web/app/users/page.tsx` como página piloto sem alterar sua lógica
- [X] T014 [P] [US1] Criar `tests/web/UiComponents.test.tsx` cobrindo Dialog (Esc fecha, Cancelar), Field (erro associado via `aria-describedby`) e DataTable (paginação chama os callbacks)
- [X] T015 [US1] Corrigir D4: ajustar o link em `.github/copilot-instructions.md` para `docs/05-design-system/design-systen.md` e, com confirmação do usuário, adicionar a orientação do design system em `.specify/memory/constitution.md` (emenda MINOR)
- [X] T016 [US1] Rodar `npm run lint` e `npm test`; atualizar F0 em `execution-control.md` (status, datas, evidência)

**Checkpoint F0**: critérios de saída de F0 atendidos.

---

## Phase 3: User Story 2 — Acesso e usuários (F1, F2) (Priority: P1)

**Goal**: login/ativação (T1) e gerenciamento de usuários (T2) iguais aos protótipos, mantendo regras de acesso.

**Independent Test**: comparar com `login_boas_vindas` e `gerenciar_usuarios`; executar entrar, ativar convite, convidar e alterar situação.

### F1 — T1 `login_boas_vindas`

- [X] T017 [US2] Reestilizar `src/apps/web/app/login/page.tsx` conforme `docs/05-design-system/login_boas_vindas/` (painel de boas-vindas, formulário "Entrar no sistema", campo "Senha de acesso") usando componentes `ui/`; manter schema Zod e fluxo de autenticação
- [X] T018 [P] [US2] Aplicar o mesmo layout em `src/apps/web/app/activate/page.tsx` e `src/apps/web/app/auth/confirm/page.tsx` (telas derivadas de T1), preservando mensagens de erro de `lib/auth/invitation.ts`
- [X] T019 [US2] Verificar T1 contra o print (desktop e ~375 px, teclado) e `tests/web/AuthFlow.test.tsx`, `AuthConfirm.test.tsx`, `InvitationErrors.test.ts`; atualizar F1/T1 em `execution-control.md` com evidência

### F2 — T2 `gerenciar_usuarios`

- [X] T020 [P] [US2] Reestilizar `src/apps/web/components/users/UserTable.tsx` conforme `docs/05-design-system/gerenciar_usuarios/` (colunas Usuário/Perfil de Acesso/Situação/Ações, StatusBadge) usando `DataTable`
- [X] T021 [P] [US2] Reestilizar `src/apps/web/components/users/UserForm.tsx` como diálogo "Convidar novo usuário" (opções Perfil Administrador / Perfil Operador) usando `Dialog` e `Field`
- [X] T022 [US2] Atualizar `src/apps/web/app/users/UsersPageClient.tsx` e `src/apps/web/app/users/[userId]/page.tsx` com `PageHeader`, filtros Situação/Perfil (na URL) e estados padrão, sem mudar chamadas de `lib/users.ts`
- [X] T023 [US2] Verificar T2 contra o print e `tests/web/UserManagement*.test.tsx`, `RoleAccess.test.tsx`; atualizar F2/T2 em `execution-control.md`

**Checkpoint**: F1 e F2 concluídas; acesso e usuários funcionam como antes.

---

## Phase 4: User Story 3 — Localizações, produtos e fornecedores (F3, F4, F5) (Priority: P2)

**Goal**: T3–T7 iguais aos protótipos, sem perda de função.

**Independent Test**: por tela, comparar com o print e executar cadastro, filtro, edição e inativação.

### F3 — T3 `estantes_e_prateleiras` (depende de F0)

- [X] T024 [US3] Reestilizar `src/apps/web/app/locations/LocationsPageClient.tsx` conforme `docs/05-design-system/estantes_e_prateleiras/` (estante em foco, lista código/descrição, "Cadastrar Nova Estante", painel de prateleiras)
- [X] T025 [P] [US3] Reestilizar `src/apps/web/app/locations/[rackId]/shelves/[shelfId]/page.tsx` (derivada de T3) e `src/apps/web/app/locations/page.tsx`
- [X] T026 [US3] Verificar T3 contra o print e `tests/web/LocationsPage.test.tsx`; atualizar F3/T3 em `execution-control.md`

### F4 — T4 `catalogo_de_produtos` e T5 `modal_cadastrar_produto` (depende de F0, F3)

- [X] T027 [P] [US3] Reestilizar `src/apps/web/components/products/ProductList.tsx` conforme `docs/05-design-system/catalogo_de_produtos/` (filtros estante/prateleira/situação/limpar na URL, colunas Localização Física/Fornecedor/Quantidade/Preço Unitário/Ações, paginação)
- [X] T028 [P] [US3] Reestilizar `src/apps/web/components/products/ProductForm.tsx` conforme `docs/05-design-system/modal_cadastrar_produto/` (nome, estante, prateleira, fornecedor, quantidade inicial, preço); NÃO incluir "Categoria" nem "Gerar Auto" (D2); preservar regra de preço apenas para Administrador
- [X] T029 [US3] Atualizar `src/apps/web/app/products/ProductsPageClient.tsx`, `app/products/new/page.tsx` e `app/products/[productId]/page.tsx` com `PageHeader`, diálogo de cadastro e estados padrão
- [X] T030 [P] [US3] Reestilizar `src/apps/web/components/products/ProductDetails.tsx` e `ProductSuppliers.tsx` (derivadas de T4/T5)
- [X] T031 [US3] Verificar T4/T5 contra os prints e `tests/web/ProductForm.test.tsx`, `ProductList.test.tsx`, `ProductSupplierLinks.test.tsx`; atualizar F4/T4/T5 em `execution-control.md`

### F5 — T6 `gestao_de_fornecedores` e T7 `modal_cadastrar_fornecedor` (depende de F0, F4)

- [X] T032 [P] [US3] Criar/ajustar a lista em `src/apps/web/app/suppliers/SuppliersPageClient.tsx` conforme `docs/05-design-system/gestao_de_fornecedores/` (Fornecedor/Documento/Contato/Itens Associados/Situação/Ações) e diálogo de confirmação de inativação ("Alterar Situação" / "Confirmar Inativação")
- [X] T033 [P] [US3] Reestilizar `src/apps/web/components/suppliers/SupplierForm.tsx` conforme `docs/05-design-system/modal_cadastrar_fornecedor/`; NÃO incluir "Exportar CSV" (D1)
- [X] T034 [US3] Atualizar `src/apps/web/app/suppliers/new/page.tsx`, `app/suppliers/[supplierId]/page.tsx` e `components/suppliers/SupplierProducts.tsx` (derivadas de T6/T7)
- [X] T035 [US3] Verificar T6/T7 contra os prints e `tests/web/SuppliersPage.test.tsx`; atualizar F5/T6/T7 em `execution-control.md`

**Checkpoint**: F3, F4 e F5 concluídas.

---

## Phase 5: User Story 4 — Controle e validação transversal (F6) (Priority: P1)

**Goal**: consistência final e fechamento do controle de execução.

**Independent Test**: abrir `execution-control.md` e identificar status, responsável, evidência e pendências de todas as fases.

- [ ] T036 [US4] Percorrer todas as telas em ~375 px e por teclado; corrigir problemas de layout e foco nos arquivos afetados
- [ ] T037 [P] [US4] Verificar contraste de texto/fundo e `aria-label` de ícones e botões em todas as telas; corrigir exceções
- [ ] T038 [P] [US4] Buscar valores avulsos de cor/fonte/espaçamento fora dos tokens em `src/apps/web/app` e `src/apps/web/components`; remover ou registrar como exceção (FR-006)
- [X] T039 [US4] Rodar `npm run lint`, `npm test` e `npm run build`; confirmar zero regressões frente ao baseline de T001
- [ ] T040 [US4] Capturar telas finais em `specs/012-design-system-ui-phases/evidence/after/`, resolver ou registrar decisão de D1–D5 e marcar todas as fases e telas em `execution-control.md`

---

## Dependencies & Execution Order

- Phase 1 → Phase 2 (F0, bloqueia tudo) → Phase 3 (F1, F2 independentes entre si) e Phase 4 (F3 → F4 → F5) → Phase 5.
- Phase 3 e a F3 podem ocorrer em paralelo após F0; F4 exige F3; F5 exige F4 (mesmos componentes de tabela/diálogo e vínculos).
- Dentro de F0: T003–T004 antes de T005–T013; T005–T011 em paralelo; T012–T013 após os componentes; T016 por último.

## Parallel Examples

- F0: T005, T006, T007, T008, T009, T010, T011 e T014 em paralelo.
- F2: T020 e T021 em paralelo.
- F4: T027, T028 e T030 em paralelo. F5: T032 e T033 em paralelo.

## Implementation Strategy

- **MVP**: Phases 1–2 (F0) + F1 (T017–T019): base visual validada em uma página piloto e na porta de entrada.
- **Incremental**: uma fase por vez; ao fim de cada uma, rodar testes, registrar evidência em `execution-control.md` e só então liberar a seguinte.
