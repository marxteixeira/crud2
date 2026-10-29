# Research: Adoção do Design System

## R1 — Tokens do design system

- **Decision**: Mapear cores, tipografia, raios e espaçamentos de `design-systen.md` em `theme.extend` do Tailwind (nomes iguais aos do documento: `primary`, `surface-container-low`, `on-surface`, `headline-lg`, `gutter`…).
- **Rationale**: Os protótipos `code.html` já usam Tailwind com esses nomes; manter os nomes permite copiar a estrutura de classes e comparar facilmente.
- **Alternatives**: variáveis CSS puras (perde compatibilidade com classes dos protótipos); valores avulsos por tela (viola FR-006).
- **Nota**: o documento tem dois conjuntos de primárias (`#00288e` nos tokens e `#1E40AF` no texto). Usar `primary-container #1e40af` como cor de ação e registrar a exceção/escolha no controle.

## R2 — Shadcn UI

- **Decision**: Criar primitivos próprios em `components/ui/` com Tailwind; adotar Shadcn apenas se um componente acessível complexo (ex.: diálogo com foco preso) justificar.
- **Rationale**: Shadcn ainda não está no projeto; incluí-lo amplia dependências sem necessidade para 7 telas. Diálogo acessível pode usar o elemento nativo `dialog`.
- **Alternatives**: instalar Shadcn completo agora (custo e risco sem ganho imediato).

## R3 — Fontes

- **Decision**: Plus Jakarta Sans (títulos) e Manrope (corpo) via `next/font/google`, expostas como variáveis CSS.
- **Rationale**: Self-hosting automático, sem requisição externa em runtime, `display: swap`.
- **Alternatives**: `<link>` do Google Fonts como nos protótipos (dependência externa, pior privacidade/desempenho).

## R4 — Ícones

- **Decision**: Os protótipos usam Material Symbols; usar SVG inline mínimo (ou conjunto de ícones leve) nos itens de navegação e ações, sem carregar a fonte inteira.
- **Rationale**: Peso e controle de acessibilidade (`aria-hidden`).
- **Alternatives**: fonte Material Symbols completa (pesada).

## R5 — Estratégia de comparação visual

- **Decision**: Comparação manual por tela contra `screen.png` (captura lado a lado em desktop 1280 px) + checklist por tela; sem teste de snapshot visual automatizado.
- **Rationale**: Protótipos têm dados ilustrativos; pixel-diff geraria falsos positivos. Regressão funcional é coberta pelos testes existentes.
- **Alternatives**: Playwright visual regression (alto custo de manutenção para esta fase).

## R6 — Estratégia de entrega

- **Decision**: F0 primeiro; depois uma tela (ou par tela+diálogo) por vez, mantendo o app funcional a cada fase; F1, F2 e F3 independentes entre si após F0.
- **Rationale**: Reduz risco e permite integrar por fase.

## R7 — Divergências D1–D4

- **Decision**: D1 e D2 não implementados (sem respaldo no negócio); D3 aceita; D4 corrigida na fase F0 (ajuste de link na instrução do repositório e emenda da constituição, a confirmar pelo usuário).
- **Rationale**: Princípio de não inventar requisitos (FR-008).
