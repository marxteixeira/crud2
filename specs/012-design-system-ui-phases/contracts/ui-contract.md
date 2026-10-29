# Contrato de UI: Telas × Protótipos

Contrato visual que cada tela deve cumprir. Referência: `docs/05-design-system/<pasta>/screen.png` e `code.html`. Comportamento, rotas e permissões permanecem os atuais.

## Componentes compartilhados (F0)

| Componente | Uso | Requisitos |
|------------|-----|------------|
| Layout autenticado | Todas as telas internas | Navegação lateral/superior conforme protótipos, item atual destacado, acesso a "Usuários" só para Administrador (regra atual) |
| PageHeader | Título + ação principal | Tipografia `headline-*`, botão primário à direita |
| Button | Primário, secundário, perigo | Cores de ação do design system, foco visível |
| Field | Texto, seleção, senha | Rótulo associado, mensagem de erro com `aria-describedby` |
| DataTable | Listas | Cabeçalho, linhas, ações, paginação Anterior/Próxima, filtros na URL |
| Dialog | Cadastros e confirmações | Foco preso, Esc fecha, título e botões Cancelar/Confirmar |
| StatusBadge | Situação (Ativo/Inativo) | Verde para ativo, neutro para inativo |
| Estados | Carregando, vazio, erro, sucesso | Padrão único |

## Mapeamento por tela

| Tela | Rota(s) | Protótipo | Elementos obrigatórios | Fora do escopo |
|------|---------|-----------|------------------------|----------------|
| T1 | `/login`, `/activate`, `/auth/confirm` | `login_boas_vindas` | Painel de boas-vindas, formulário "Entrar no sistema", campo senha | — |
| T2 | `/users`, `/users/[userId]` | `gerenciar_usuarios` | Tabela, filtros Situação/Perfil, diálogo "Convidar novo usuário" (Administrador/Operador) | — |
| T3 | `/locations`, `/locations/[rackId]/shelves/[shelfId]` | `estantes_e_prateleiras` | Estante em foco, lista código/descrição, "Cadastrar Nova Estante", prateleiras da estante | — |
| T4 | `/products`, `/products/[productId]` | `catalogo_de_produtos` | Filtros estante/prateleira/situação/limpar, tabela, paginação, "Cadastrar novo produto" | — |
| T5 | `/products/new` e diálogo | `modal_cadastrar_produto` | Nome, estante, prateleira, fornecedor, quantidade inicial, preço | D2: Categoria, Gerar Auto |
| T6 | `/suppliers`, `/suppliers/[supplierId]` | `gestao_de_fornecedores` | Tabela Fornecedor/Documento/Contato/Itens/Situação/Ações, confirmação de inativação | — |
| T7 | `/suppliers/new` e diálogo | `modal_cadastrar_fornecedor` | Formulário de cadastro, Cancelar/Cadastrar | D1: Exportar CSV |

## Regras

- Valores de cor, fonte e espaçamento só via tokens (exceções registradas).
- Nenhuma alteração em `lib/`, chamadas à API ou regras de validação.
- Rótulos em português; textos ilustrativos dos protótipos não são copiados.
