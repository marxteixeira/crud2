# Research: Corrigir ativação de conta por convite

## R1 — Por que o link cai em `/login#error=otp_expired`

- **Evidência**: `InviteAsync` envia apenas `email` e `data`, sem `redirect_to`. O link padrão do Supabase (`/auth/v1/verify?token=...&type=invite`) valida o token no servidor e redireciona com tokens no fragmento. Se o token já foi consumido ou expirou, redireciona para o Site URL com `#error=access_denied&error_code=otp_expired`.
- **Causas combinadas**: (a) destino não é uma rota que trate o retorno; (b) token de uso único consumido por pré-visualização/antivírus do e-mail; (c) expiração.
- **Decision**: convite com `redirect_to` explícito e e-mail baseado em `token_hash` apontando para `/auth/confirm`.
- **Alternatives**: manter fluxo implícito e ler o fragmento no cliente (não resolve consumo por scanner); PKCE com `?code=` (exige o mesmo navegador que iniciou o fluxo, o que não vale para convite iniciado por admin).

## R2 — Por que "Definir senha e ativar" falha sem rede

- `ActivatePage` chama `supabase.auth.updateUser`. Sem sessão, o SDK retorna `AuthSessionMissingError` localmente, sem requisição. O catch único mostra a mensagem genérica.
- **Decision**: checar `getSession()` ao abrir `/activate`; sem sessão, mostrar "convite inválido ou expirado" com orientação; distinguir erro de senha (ex.: senha fraca/igual à anterior) e falha de serviço.

## R3 — Imunidade a pré-visualização

- **Decision**: `/auth/confirm` é uma página que só chama `verifyOtp({ token_hash, type: "invite" })` após clique em "Continuar". Scanners que fazem GET não consomem o token.
- **Alternatives**: verificar no GET (consumível por scanner).

## R4 — Reenvio de convite

- **Decision**: `POST /api/users/{id}/invitation` (Administrador), só para conta pendente; reemite o convite (invalida o anterior no provedor) e mantém o registro local.
- **Alternatives**: recriar usuário (perde histórico/auditoria).

## R5 — Configuração por ambiente

- **Decision**: opção `Supabase:RedirectUrl` (ou base do web) usada em `redirect_to`; deve constar nas URLs permitidas do projeto Supabase. Template do e-mail "Invite user" ajustado no painel (configuração externa, documentada no quickstart).

## R6 — Observabilidade

- **Decision**: logar apenas código de erro e TraceId no reenvio/ativação; nunca tokens ou e-mails em claro além do padrão atual.
