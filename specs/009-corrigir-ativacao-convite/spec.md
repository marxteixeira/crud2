# Feature Specification: Corrigir ativação de conta por convite

**Feature Branch**: `009-corrigir-ativacao-convite`

**Created**: 2026-10-26

**Status**: Draft

**Input**: User description: "Como administrador, cadastrei um novo usuário e o registro foi criado. O e-mail de convite foi recebido, mas ao clicar no link o usuário é redirecionado para /login com erro 'access_denied / otp_expired (Email link is invalid or has expired)'. Ao clicar em 'ativar conta' e cadastrar nova senha, aparece 'Não foi possível concluir a ativação. Solicite um novo convite ao Administrador.', sem nenhuma requisição visível na aba de rede do navegador."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ativar conta a partir do link do convite (Priority: P1)

Um usuário recém-cadastrado pelo administrador recebe o e-mail de convite, clica no link logo após o recebimento e chega a uma tela onde define sua senha e ativa a conta.

**Why this priority**: Sem isso, nenhum usuário convidado consegue acessar o sistema; o cadastro de usuários fica inútil.

**Independent Test**: Cadastrar um novo usuário, abrir o e-mail recebido imediatamente, clicar no link, definir a senha e entrar no sistema.

**Acceptance Scenarios**:

1. **Given** um convite recém-enviado e ainda válido, **When** o usuário clica no link do e-mail, **Then** ele é levado à tela de ativação (não à tela de login com erro) e pode definir sua senha.
2. **Given** o usuário na tela de ativação com sessão do convite válida, **When** informa uma senha válida e confirma, **Then** a conta é ativada, a senha é gravada e ele acessa o sistema com seu perfil.
3. **Given** o link foi aberto e a senha foi definida, **When** o usuário volta ao login, **Then** consegue entrar com e-mail e a nova senha.

---

### User Story 2 - Mensagens claras para convite inválido ou expirado (Priority: P2)

Quando o link realmente expirou, já foi usado ou é inválido (inclusive por pré-visualização automática do e-mail que consome o link), o usuário vê uma explicação clara em português e um caminho para resolver.

**Why this priority**: Mesmo com o fluxo corrigido, convites expiram; hoje o erro aparece só no endereço da página e o botão de ativar falha sem feedback útil.

**Independent Test**: Abrir um link expirado/já usado e verificar a mensagem e as ações oferecidas.

**Acceptance Scenarios**:

1. **Given** um link expirado ou já utilizado, **When** o usuário o abre, **Then** vê mensagem amigável informando o motivo e que deve solicitar novo convite ao administrador.
2. **Given** a tela de ativação sem sessão de convite válida, **When** o usuário tenta ativar, **Then** o sistema informa o motivo real (convite ausente/expirado) em vez de tentar a ativação e falhar silenciosamente.

---

### User Story 3 - Administrador reenvia convite (Priority: P3)

O administrador consegue reenviar o convite a um usuário que ainda não ativou a conta, invalidando convites anteriores.

**Why this priority**: Fornece a saída para convites expirados sem recriar o usuário.

**Independent Test**: Para um usuário pendente, reenviar convite e ativar com o novo link.

**Acceptance Scenarios**:

1. **Given** um usuário com ativação pendente, **When** o administrador reenvia o convite, **Then** um novo e-mail é enviado e o novo link permite a ativação.
2. **Given** um usuário já ativo, **When** o administrador tenta reenviar, **Then** a ação não é oferecida ou é recusada com mensagem clara.

### Edge Cases

- Link aberto duas vezes (ou consumido por pré-visualização de leitor de e-mail/antivírus): segunda abertura deve resultar em mensagem clara, não em falha silenciosa.
- Link aberto em navegador diferente do esperado ou com sessão de outro usuário já ativa.
- Senha que não atende aos requisitos ou confirmação divergente.
- Falha de rede ou serviço de autenticação durante a ativação: o usuário recebe erro compreensível e pode tentar novamente.
- Endereço de retorno do convite diferente do ambiente em uso (ex.: local versus produção).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O link do convite, quando válido, MUST conduzir o usuário à tela de ativação com a identidade do convite reconhecida.
- **FR-002**: O sistema MUST permitir que o usuário convidado defina a senha e conclua a ativação, ficando com o perfil atribuído pelo administrador.
- **FR-003**: O sistema MUST detectar erros de convite informados no retorno do link (expirado, inválido, negado) e exibir mensagem amigável em português.
- **FR-004**: O sistema MUST impedir a tentativa de ativação sem sessão de convite válida e informar o motivo real ao usuário.
- **FR-005**: Falhas na ativação MUST distinguir ao menos: convite expirado/inválido, senha inválida e falha temporária de serviço.
- **FR-006**: O administrador MUST poder reenviar convite a usuários pendentes; convites anteriores deixam de valer.
- **FR-007**: O endereço de retorno dos convites MUST corresponder ao ambiente em que o sistema é executado.
- **FR-008**: O sistema MUST registrar (sem dados sensíveis) tentativas de ativação malsucedidas para diagnóstico.

### Key Entities

- **Usuário convidado**: pessoa cadastrada pelo administrador, com e-mail, perfil e estado (pendente ou ativo).
- **Convite**: link único, de validade limitada e uso único, enviado por e-mail para ativação.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% dos usuários convidados concluem a ativação na primeira tentativa, dentro do prazo de validade do convite.
- **SC-002**: Um usuário ativa a conta e entra no sistema em menos de 2 minutos após abrir o e-mail.
- **SC-003**: 100% dos casos de convite expirado/inválido exibem mensagem explicativa com próximo passo.
- **SC-004**: Chamados de suporte sobre convites que "não funcionam" caem em ao menos 80%.

## Assumptions

- O envio do e-mail de convite já funciona e o registro do usuário é criado corretamente.
- A causa raiz (link consumido/expirado antes do uso, endereço de retorno incorreto, ou ativação tentada sem sessão) será investigada na fase de planejamento.
- O prazo de validade do convite segue o padrão do provedor de autenticação existente.
- Apenas administradores reenviam convites; o escopo cobre o fluxo web existente de ativação e gestão de usuários.
