# Modelo de Dados: Localização e produtos

**Feature**: [spec.md](spec.md)

## Produto

Representa um item do catálogo da loja.

| Campo | Tipo lógico | Obrigatório | Regras |
|---|---|---:|---|
| `id` | UUID | Sim | Identificador gerado pelo sistema. |
| `name` | Texto | Sim | Forma original para apresentação; não vazio após remoção de espaços externos. |
| `normalized_name` | Texto | Sim | Chave de comparação normalizada; única inclusive entre produtos inativos. |
| `quantity` | Inteiro | Sim | Maior ou igual a zero; unidades inteiras no MVP. |
| `price` | Decimal monetário | Não | Em reais, maior ou igual a zero, precisão de centavos; obrigatório para produto ativo. |
| `shelf_id` | UUID | Não | Referência à prateleira; obrigatório para produto ativo. |
| `status` | Enum | Sim | `Active` ou `Inactive`; produto novo criado por Operador sem preço inicia inativo. |
| `created_at` | Data/hora UTC | Sim | Definido na criação. |
| `updated_at` | Data/hora UTC | Sim | Atualizado em toda alteração relevante. |
| `created_by` | UUID | Sim | Conta autenticada que criou o registro. |
| `updated_by` | UUID | Sim | Conta autenticada responsável pela última alteração. |

### Invariantes e transições

- O nome de exibição deve ser não vazio; a chave normalizada é única entre produtos ativos e inativos.
- A chave normalizada aplica normalização Unicode, retirada de marcas diacríticas e caixa invariável; a mesma função é usada por criação e edição.
- Quantidade é inteira e não negativa; preço, quando presente, é não negativo e limitado a duas casas decimais.
- Um produto ativo deve ter preço e prateleira ativa pertencente a uma estante ativa. Um produto inativo pode não ter preço ou prateleira.
- Apenas Administrador pode definir ou alterar preço. Um Operador pode criar produto inativo sem preço, mas o pedido do Operador não define preço.
- A ativação exige preço e localização válidos. Produto inativado mantém seu registro, preço e histórico.
- Alterações de nome, quantidade, preço, localização ou situação atualizam `updated_at` e `updated_by`.
- A exclusão física não faz parte dos fluxos normais desta fase.

## Estante

Representa uma estrutura física de armazenamento.

| Campo | Tipo lógico | Obrigatório | Regras |
|---|---|---:|---|
| `id` | UUID | Sim | Identificador gerado pelo sistema. |
| `code` | Texto | Sim | Identificador/código visível, não vazio; único sem distinção de caixa/acentos. |
| `normalized_code` | Texto | Sim | Chave normalizada para unicidade. |
| `description` | Texto | Não | Descrição amigável opcional. |
| `status` | Enum | Sim | `Active` ou `Inactive`. |
| `created_at`, `updated_at` | Data/hora UTC | Sim | Auditoria temporal. |
| `created_by`, `updated_by` | UUID | Sim | Conta responsável pela criação/última alteração. |

## Prateleira

Representa um nível de uma única estante.

| Campo | Tipo lógico | Obrigatório | Regras |
|---|---|---:|---|
| `id` | UUID | Sim | Identificador gerado pelo sistema. |
| `rack_id` | UUID | Sim | Referência obrigatória à estante proprietária. |
| `code` | Texto | Sim | Identificador da prateleira, não vazio. |
| `normalized_code` | Texto | Sim | Único sem distinção de caixa/acentos dentro da mesma estante. |
| `status` | Enum | Sim | `Active` ou `Inactive`. |
| `created_at`, `updated_at` | Data/hora UTC | Sim | Auditoria temporal. |
| `created_by`, `updated_by` | UUID | Sim | Conta responsável pela criação/última alteração. |

### Invariantes e relacionamentos

- `normalized_code` é único em conjunto com `rack_id`.
- A estante deve estar ativa para permitir seleção de suas prateleiras.
- Uma prateleira não pode ser inativada enquanto houver produtos ativos nela.
- Uma estante não pode ser inativada enquanto houver produtos ativos localizados em qualquer uma de suas prateleiras.
- Inativar uma localização não apaga a estante, a prateleira nem seus vínculos históricos. Reativação mantém os códigos e referências.
- Uma estante contém zero ou mais prateleiras. Uma prateleira pertence a uma única estante. Uma prateleira pode localizar zero ou mais produtos. Um produto tem zero ou uma prateleira.
- As referências de produto para prateleira usam comportamento de exclusão restritivo; a aplicação não remove os registros fisicamente.

## Auditoria e autorização

- O `created_by`/`updated_by` referencia o identificador da conta da aplicação já autenticada.
- O horário é gravado em UTC e só representa alterações persistidas com sucesso.
- Ambos os perfis autenticados podem manter estantes, prateleiras e os campos de produto que lhes são permitidos.
- Só Administrador pode definir/alterar preço. A política é validada no servidor em cada operação, não baseada em campos enviados pelo cliente.
- Listagens são paginadas; a busca/filtro não deve expor dados de produtos inativos quando a situação ativa foi explicitamente selecionada.

## Índices e restrições de persistência

- Índice único em `Produto.normalized_name`.
- Índice em `Produto.status`, `Produto.shelf_id` e campos usados para paginação/busca conforme o plano de consulta.
- Índice único em `Estante.normalized_code`.
- Índice único composto em `Prateleira.rack_id` e `Prateleira.normalized_code`; índice em `Prateleira.status`.
- Chaves estrangeiras com exclusão restritiva para impedir perda acidental de referências.
- Restrições/validações persistentes para quantidade não negativa, preço não negativo quando presente e precisão monetária compatível com centavos.
