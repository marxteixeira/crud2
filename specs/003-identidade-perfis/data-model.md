# Modelo de Dados: Identidade e perfis

## ContaDeUsuario

Perfil e estado de autorização de propriedade da aplicação, vinculado a uma identidade Supabase Auth.

| Campo | Tipo | Obrigatório | Regras |
|---|---|---:|---|
| `id` | UUID | Sim | Identificador primário gerado pela aplicação. |
| `identity_subject` | UUID | Sim | Identificador do usuário Supabase Auth (claim `sub`); único e imutável após vinculação. |
| `email` | String | Sim | E-mail normalizado usado para entrar; único e sincronizado com o provedor. Não é senha nem segredo. |
| `pending_email` | String | Não | Novo e-mail normalizado aguardando confirmação do titular; deve ser único enquanto reservado. |
| `display_name` | String | Sim | Nome não vazio exibido aos Administradores. |
| `role` | Enum | Sim | `Administrador` ou `Operador`; valor autoritativo para autorização da aplicação. |
| `status` | Enum | Sim | `ConvitePendente`, `Ativa` ou `Inativa`; somente `Ativa` autoriza requisições protegidas. |
| `created_at` | Data/hora UTC | Sim | Definida quando a conta da aplicação é provisionada. |
| `updated_at` | Data/hora UTC | Sim | Atualizada quando dados, perfil ou situação da conta mudam. |
| `created_by` | Referência do ator | Sim | Identificador da conta Administrador que convidou ou valor reservado `bootstrap` para a primeira conta. |
| `updated_by` | Referência do ator | Sim | Identificador da conta Administrador que realizou a última alteração ou valor `bootstrap` somente na criação inicial. |

### Validações e invariantes

- `identity_subject` e e-mail normalizado são únicos individualmente; comparar e-mails sem distinção entre maiúsculas e minúsculas.
- O e-mail pendente, quando presente, também não pode colidir com o e-mail ativo ou pendente de outra conta.
- Senhas e credenciais administrativas do provedor nunca são armazenadas nesta entidade.
- O perfil vem deste registro persistido e atual, não de dados enviados pelo navegador nem de metadados de identidade alteráveis pelo usuário.
- Requisições protegidas de identidade inativa ou não vinculada são recusadas. Perfil e situação ativa são consultados do estado atual da aplicação para que mudanças tenham efeito mesmo com tokens já emitidos.
- Uma operação não pode inativar ou rebaixar o último Administrador ativo.
- Inativar é reversível e preserva o registro e os horários de auditoria.
- Alterações administrativas preservam o ator (`updated_by`) e o horário da mudança; leituras de listagem são paginadas.
- Uma troca de e-mail só substitui o endereço ativo após confirmação pelo titular; até lá, o e-mail atual continua sendo o login da conta.

### Ciclo de vida

`ConvitePendente` (convite do provedor pendente) → `Ativa` (convite aceito e conta habilitada) → `Inativa` (inativação pelo Administrador) → `Ativa` (reativação pelo Administrador).

Uma conta com convite pendente não recebe acesso a operações protegidas. O titular pode concluir o fluxo de ativação com uma sessão autenticada e e-mail confirmado pelo provedor; a aplicação só ativa a conta vinculada cujo estado ainda seja pendente. Conta inativada por Administrador não pode usar esse fluxo para se reativar. Caso o provisionamento parcial falhe entre provedor e aplicação, os estados precisam ser reconciliados.

## SessaoDeAcesso

Uma sessão de autenticação é emitida e gerida pelo Supabase Auth. A aplicação não persiste credenciais nem duplica a sessão completa do provedor. Para cumprir a saída imediata nas requisições da API, armazena somente o identificador da sessão após o logout.

| Campo | Tipo | Obrigatório | Regras |
|---|---|---:|---|
| `provider_session_id` | UUID | Sim | Identificador único de sessão do Supabase; usado para recusar tokens remanescentes da sessão encerrada. |
| `revoked_at` | Data/hora UTC | Sim | Definida pelo fluxo de logout autenticado antes de concluir a saída do provedor. |

Requisições protegidas recusam um identificador de sessão registrado como revogado. A sessão no provedor continua sendo a fonte de identidade e emissão de tokens; o registro da aplicação é somente uma marca de revogação.

## PerfilDeAcesso

Vocabulário fechado de autorização da aplicação, não é uma entidade administrável:

- `Administrador`: pode gerenciar contas e executar futuras ações reservadas ao Administrador.
- `Operador`: pode acessar fluxos operacionais permitidos em fases futuras; não pode gerenciar usuários nem alterar preços de produtos.

## Relacionamentos

- Cada `ContaDeUsuario` corresponde a uma identidade do provedor (`identity_subject`).
- Cada `ContaDeUsuario` pode ter várias sessões emitidas pelo provedor; cada sessão pode ser revogada individualmente.
- Perfil e situação ativa pertencem à conta da aplicação e valem imediatamente para todas as suas sessões.
