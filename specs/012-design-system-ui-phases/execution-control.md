# Controle de Execução: Adoção do Design System

Atualizar a cada mudança de status. Status: Não iniciada · Em andamento · Bloqueada · Concluída. Uma fase só vira Concluída com todos os critérios de saída atendidos e evidência registrada.

## Fases

| Fase | Escopo | Depende de | Status | Responsável | Início | Conclusão | Evidência | Pendências |
|------|--------|------------|--------|-------------|--------|-----------|-----------|------------|
| F0 | Tokens, fontes, componentes `ui/`, layout/navegação, correção de D4 | — | Em validação | Copilot | 2026-10-28 | | Build, lint, tsc e 60 testes passando; comparação visual com print pendente | Revisão visual manual |
| F1 | T1 `login_boas_vindas` | F0 | Em validação | Copilot | 2026-10-28 | | Build, lint, tsc e 60 testes passando; comparação visual com print pendente | Revisão visual manual |
| F2 | T2 `gerenciar_usuarios` | F0 | Em validação | Copilot | 2026-10-28 | | Build, lint, tsc e 60 testes passando; comparação visual com print pendente | Revisão visual manual |
| F3 | T3 `estantes_e_prateleiras` | F0 | Em validação | Copilot | 2026-10-28 | | Build, lint, tsc e 60 testes passando; comparação visual com print pendente | Revisão visual manual |
| F4 | T4 `catalogo_de_produtos`, T5 `modal_cadastrar_produto` | F0, F3 | Em validação | Copilot | 2026-10-28 | | Build, lint, tsc e 60 testes passando; comparação visual com print pendente | Revisão visual manual |
| F5 | T6 `gestao_de_fornecedores`, T7 `modal_cadastrar_fornecedor` | F0, F4 | Em validação | Copilot | 2026-10-28 | | Build, lint, tsc e 60 testes passando; comparação visual com print pendente | Revisão visual manual |
| F6 | Validação transversal | F1–F5 | Não iniciada | | | | | |

## Telas

| Tela | Protótipo | Fase | Print | Funções | Estados | Celular | Teclado/contraste | Status |
|------|-----------|------|-------|---------|---------|---------|-------------------|--------|
| T1 | `login_boas_vindas` | F1 | ☐ | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T2 | `gerenciar_usuarios` | F2 | ☐ | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T3 | `estantes_e_prateleiras` | F3 | ☐ | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T4 | `catalogo_de_produtos` | F4 | ☐ | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T5 | `modal_cadastrar_produto` | F4 | ☐ | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T6 | `gestao_de_fornecedores` | F5 | ☐ | ☐ | ☐ | ☐ | ☐ | Não iniciada |
| T7 | `modal_cadastrar_fornecedor` | F5 | ☐ | ☐ | ☐ | ☐ | ☐ | Não iniciada |

## Divergências

| ID | Descrição | Decisão | Status |
|----|-----------|---------|--------|
| D1 | "Exportar CSV" em T7 | Fora do MVP | Pendente de decisão |
| D2 | "Categoria" e "Gerar Auto" em T5 | Fora do modelo atual | Pendente de decisão |
| D3 | Dados ilustrativos dos protótipos | Não são conteúdo | Aceita |
| D4 | Link `design-system.md` (nome real `design-systen.md`) e ausência da orientação em `constitution.md` | Corrigir na F0 | Resolvida (constitution v1.1.0, princípio VIII) |
| D5 | Duas cores primárias no design system (`#00288e` e `#1E40AF`) | Usar `primary-container #1e40af` como ação | Aceita |
