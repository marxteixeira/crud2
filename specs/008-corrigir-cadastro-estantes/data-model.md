# Data Model: Corrigir cadastro de estantes

Sem alterações de entidades ou de esquema. O modelo permanece o da feature [007](../007-localizacao-produtos/data-model.md).

## Estado exigido do banco

| Migration | Tabelas |
|-----------|---------|
| `IdentityAndRevokedSessions` | `contas_de_usuario`, `sessoes_revogadas` |
| `AddStorageLocations` | `estantes`, `prateleiras` |
| `AddProducts` | `produtos` |

## Conceito novo (não persistido)

- **Estado de migrations**: resultado da comparação entre migrations do código e as aplicadas no banco (lista de pendentes). Usado em aviso de startup e no health check `ready`.
