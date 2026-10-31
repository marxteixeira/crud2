# Data Model

Sem alterações de dados persistidos.

- **Estante** (existente): `id`, `code`, `description`.
- **Estante selecionada** (estado de UI): id de uma estante presente na lista de estantes em cache.
  - Após cadastro bem-sucedido: passa a ser o id da nova estante, que é inserida no cache da lista antes da seleção.
  - Falha no cadastro: não muda.
  - Escolha manual: sobrescreve qualquer seleção anterior.
