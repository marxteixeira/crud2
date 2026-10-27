# Data Model: Corrigir ativação de conta por convite

Sem novas tabelas ou migrations.

## Conta de usuário (existente)

- Estados: `Pendente` → `Ativa` (via ativação) ; `Inativa` (via administração).
- Reenvio de convite: permitido somente em `Pendente`; não altera estado nem perfil.

## Convite (gerenciado pelo provedor de autenticação)

- Atributos: token de uso único, validade limitada, destino de retorno.
- Regras: novo convite invalida o anterior; token consumido ou expirado resulta em erro `otp_expired`.

## Estados da tela de ativação

| Estado | Condição | Resultado |
|--------|----------|-----------|
| Verificando | token em verificação | indicador de progresso |
| Pronto | sessão de convite válida | formulário de senha |
| Convite inválido | sem sessão / `otp_expired` | mensagem e orientação |
| Senha rejeitada | provedor recusa a senha | mensagem de senha |
| Serviço indisponível | falha de rede/5xx | mensagem e nova tentativa |
