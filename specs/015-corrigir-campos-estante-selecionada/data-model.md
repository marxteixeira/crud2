# Data Model: Campos da estante selecionada

Sem mudança de dados persistidos.

- **Estante (`Rack`)**: `id`, `code`, `description` (opcional), `status`, `shelves`. Fonte dos valores exibidos nos campos Código e Descrição.
- **Estante selecionada**: estado local de tela (`selectedRackId`). Ao mudar, o formulário de edição deve refletir os valores da nova estante; alterações não salvas são descartadas.
