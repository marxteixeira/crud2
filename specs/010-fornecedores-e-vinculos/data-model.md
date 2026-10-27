# Modelo de Dados: Fornecedores e vínculos

**Feature**: [spec.md](spec.md)

## Fornecedor

Representa uma pessoa ou empresa que fornece produtos à loja.

| Campo | Tipo lógico | Obrigatório | Regras |
|---|---|---:|---|
| `id` | UUID | Sim | Identificador gerado pelo sistema. |
| `name` | Texto | Sim | Nome ou razão social; não vazio após remoção de espaços externos. |
| `tax_identifier` | Texto de dígitos | Sim | CPF com 11 ou CNPJ com 14 dígitos; armazenado sem pontuação, com dígitos verificadores válidos e único. |
| `phone` | Texto | Não | Telefone para contato, quando informado. |
| `email` | Texto | Não | E-mail para contato, quando informado e válido. |
| `contact_name` | Texto | Não | Pessoa de contato, quando informada. |
| `notes` | Texto | Não | Observações fornecidas pela loja. |
| `status` | Enum | Sim | `Active` ou `Inactive`. |
| `created_at` | Data/hora UTC | Sim | Definido na criação. |
| `updated_at` | Data/hora UTC | Sim | Atualizado em toda alteração persistida. |
| `created_by` | UUID | Sim | Conta autenticada responsável pela criação. |
| `updated_by` | UUID | Sim | Conta autenticada responsável pela última alteração. |

### Invariantes e transições

- Nome/razão social e identificador são obrigatórios.
- A validação determina CPF ou CNPJ pelo número de dígitos e verifica os dígitos verificadores; pontuação e espaços externos são removidos antes da persistência.
- A representação canônica contém somente dígitos. CPF e CNPJ não podem ser repetidos, inclusive quando a apresentação original diferir.
- Dados opcionais são validados quando informados; telefone e observações não são usados como identificadores.
- Fornecedor ativo pode receber novos vínculos. Fornecedor inativo continua consultável, mas não pode receber vínculos.
- A inativação é lógica e é recusada se houver ao menos um vínculo ativo com produto ativo.
- Alterações de dados ou situação atualizam `updated_at` e `updated_by`; cadastro não é removido fisicamente.

## Vínculo de fornecimento

Representa a associação entre um produto e um fornecedor.

| Campo | Tipo lógico | Obrigatório | Regras |
|---|---|---:|---|
| `id` | UUID | Sim | Identificador gerado pelo sistema. |
| `product_id` | UUID | Sim | Referência ao produto existente. |
| `supplier_id` | UUID | Sim | Referência ao fornecedor existente. |
| `status` | Enum | Sim | `Active` ou `Inactive`; vínculo inativo representa desassociação. |
| `created_at` | Data/hora UTC | Sim | Definido na associação inicial. |
| `updated_at` | Data/hora UTC | Sim | Atualizado em associação ou desassociação. |
| `created_by` | UUID | Sim | Conta autenticada que criou o vínculo. |
| `updated_by` | UUID | Sim | Conta autenticada responsável pela última alteração. |

### Invariantes e transições

- Um produto pode ter zero ou vários fornecedores; um fornecedor pode estar associado a zero ou vários produtos.
- Pode existir no máximo um vínculo ativo para cada par produto/fornecedor. Ao associar um vínculo previamente inativo, reativar o registro existente e manter a auditoria de criação.
- Uma nova associação exige produto ativo e fornecedor ativo. Um vínculo já existente pode ser inativado mesmo se um dos cadastros estiver inativo, para permitir a resolução de referências.
- Desassociar altera o vínculo para inativo; não remove fisicamente o registro. Consultas operacionais retornam vínculos ativos por padrão; uma consulta histórica explícita pode filtrar vínculos inativos, identificados como históricos.
- A associação/desassociação registra a conta e horário em `updated_by` e `updated_at`.
- A inativação do fornecedor é bloqueada somente por vínculos ativos ligados a produtos ativos. Vínculos históricos ou produtos inativos não bloqueiam a operação.

## Produto e regras relacionadas

O produto é a entidade já definida em [specs/007-localizacao-produtos/data-model.md](../../007-localizacao-produtos/data-model.md). Esta fase acrescenta a consulta de fornecedores associados e o filtro opcional por fornecedor às listagens existentes; não altera campos, regras de preço, localização, situação ou histórico do produto.

## Relacionamentos e persistência

- `Fornecedor` 1 — 0..N `Vínculo de fornecimento` N..1 — 1 `Produto`.
- Chaves estrangeiras do vínculo referenciam fornecedor e produto com exclusão restritiva.
- Índice único em `Fornecedor.tax_identifier`.
- Índice único filtrado/condicional para vínculo ativo por `(product_id, supplier_id)`; adicionar índice em `supplier_id`, `product_id` e situação conforme os padrões de consulta do projeto.
- Inativar um vínculo preserva a linha e sua auditoria.
- As consultas são paginadas e usam leitura sem rastreamento; a filtragem por fornecedor compõe-se aos filtros atuais do catálogo.
- A verificação de vínculos ativos a produtos ativos e a transição de situação do fornecedor devem ser feitas em uma operação transacional, considerando concorrência com criação de vínculo.

## Autorização e auditoria

- Todas as operações exigem conta ativa; políticas são verificadas na API.
- Administradores podem cadastrar, editar e inativar fornecedores.
- Consultas de catálogo/fornecimento são disponíveis aos perfis previstos no catálogo. Criar e remover vínculos segue as permissões de edição de produto existentes.
- A interface não é fronteira de segurança: tentativas não autorizadas são recusadas no servidor sem alteração de dados.
- Todo registro criado/alterado tem autoria e horário UTC; mutações também produzem logs estruturados sem dados pessoais desnecessários.
