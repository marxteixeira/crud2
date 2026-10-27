# Quickstart: validar a ativação por convite

## Pré-requisitos

- API em `http://localhost:5013` e web em `http://localhost:3000` em execução.
- No painel do Supabase: URL `http://localhost:3000/**` nas URLs de redirecionamento permitidas e template "Invite user" conforme [contrato](./contracts/invitation-api.md).
- Conta de Administrador ativa.

## Cenários

1. **Ativação feliz**: como administrador, cadastre um usuário; abra o e-mail e clique no link → `/auth/confirm` → "Continuar" → `/activate` → defina a senha → acesso ao sistema. Entrar novamente com e-mail e nova senha funciona.
2. **Link já usado/expirado**: abra o mesmo link novamente → mensagem amigável de convite inválido/expirado, sem erro no fragmento da URL de login.
3. **Pré-visualização**: requisitar o link por GET (curl) não invalida o convite; o clique em "Continuar" ainda funciona.
4. **`/activate` sem sessão**: acessar diretamente → mensagem de convite inválido (sem tentar atualizar senha).
5. **Reenvio**: em `/users`, usuário pendente → "Reenviar convite" → novo e-mail; o link antigo falha e o novo funciona. Usuário ativo não oferece a ação.

## Testes automatizados

- Web: `npm test` em `tests/web` (confirm, activate, login, UserManagement).
- API: `dotnet test` (reenvio de convite, `redirect_to` no convite).
