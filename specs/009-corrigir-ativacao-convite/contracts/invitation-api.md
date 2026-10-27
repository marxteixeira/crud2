# Contrato: Convite e ativação

## Web

| Rota | Comportamento |
|------|---------------|
| `GET /auth/confirm?token_hash=...&type=invite` | Exibe botão "Continuar"; ao clicar, verifica o token e redireciona para `/activate`. Em falha, exibe mensagem de convite inválido/expirado. |
| `GET /activate` | Sem sessão de convite: mensagem de convite inválido. Com sessão: formulário de senha. |
| `GET /login` | Interpreta `error`/`error_code` em query ou fragmento e mostra mensagem em português. |

Parâmetro de entrada `type` aceito: apenas `invite`. Destinos de redirecionamento são fixos (sem open redirect).

## API

### `POST /api/users/{id}/invitation`

- **Autorização**: Administrador.
- **204**: convite reenviado.
- **404**: usuário inexistente.
- **409**: usuário não está pendente.
- **401/403**: não autenticado / sem perfil.
- **502**: provedor de autenticação indisponível (Problem Details, sem detalhes internos).

## Configuração

- `Supabase:RedirectUrl`: URL base do web usada no `redirect_to` do convite; deve estar nas URLs permitidas do Supabase.
- Template "Invite user" do Supabase: link `{{ .SiteURL }}/auth/confirm?token_hash={{ .TokenHash }}&type=invite`.
