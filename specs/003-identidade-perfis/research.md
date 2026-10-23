# Pesquisa: Identidade e perfis

**Escopo**: decidir como implementar a autenticação e administração já determinadas pela especificação e pela arquitetura do projeto. As decisões abaixo mantêm Supabase Auth, JWT, API .NET e PostgreSQL; não representam uma comparação de provedores.

## Decisão 1: Login por e-mail e senha e ativação por convite

**Decisão**: o web autentica a pessoa com `signInWithPassword` do Supabase Auth. Um Administrador provisiona a conta por convite de e-mail com o endpoint administrativo do provedor. O titular segue o convite e escolhe sua própria senha; o Administrador nunca cria, recebe ou consulta essa senha. A aplicação mantém a conta inicialmente como `ConvitePendente` e ativa somente a identidade vinculada que concluir o fluxo de ativação. O endpoint de ativação é a única operação da aplicação permitida a uma identidade pendente.

**Justificativa**: login por senha e convite são recursos nativos do provedor aprovado e satisfazem a decisão de usar e-mail como login, sem cadastro público ou compartilhamento de credenciais. A credencial de administração do Supabase só será usada pela API.

**Alternativas consideradas**:

- Administrador atribuir uma senha inicial: rejeitada porque expõe ou exige transferência da senha do titular.
- Cadastro público: rejeitado porque a especificação permite provisionamento somente por Administrador.
- Gerar link para envio por serviço próprio: não necessário para o fluxo atual; pode ser reconsiderado se a loja aprovar um serviço de e-mail dedicado.
- Atribuir perfil em `user_metadata`: rejeitada porque metadados editáveis pelo usuário não podem ser autoridade de autorização.

## Decisão 2: Verificação de JWT na API

**Decisão**: usar autenticação bearer do ASP.NET Core com validação explícita de assinatura, emissor, público e expiração. Preferir chaves assimétricas do Supabase e sua publicação JWKS, configurando o emissor e o público esperado por ambiente. A API não deve aceitar um token apenas porque está decodificado, nem registrar seu conteúdo.

**Justificativa**: chaves assimétricas permitem ao serviço validar tokens com chave pública sem distribuir o segredo de assinatura do provedor à API. A validação completa preserva o limite de confiança entre navegador, provedor e serviço.

**Alternativas consideradas**:

- Segredo compartilhado HS256: suportado pelo provedor, mas exige manter um segredo de assinatura na configuração da API.
- Confiar em claims sem validar assinatura, emissor ou público: rejeitado por não autenticar o emissor do token.

## Decisão 3: Perfis e autorização atualizados imediatamente

**Decisão**: manter `Administrador`/`Operador` e o estado da conta na tabela de perfil da aplicação. Espelhar o perfil em `app_metadata` (metadado administrativo não editável pelo usuário) para que tokens e fluxos do provedor recebam a role; nunca usar `user_metadata` como fonte de autorização. Para cada requisição protegida, a API consulta a conta vinculada ao `sub`, confirma que está ativa e deriva o papel autorizado do estado persistido atual. A autorização ASP.NET Core deve usar essa role atual, substituindo qualquer claim de role desatualizada. Atualizar o `app_metadata` quando o Administrador altera o perfil.

**Justificativa**: JWTs assinados são validados até expirarem; mudar uma role ou desativar uma conta não altera tokens já emitidos. Uma consulta à persistência da aplicação no limite da API faz com que alterações tenham efeito na requisição seguinte, conforme a especificação, e evita uma chamada ao serviço de autenticação em cada operação.

**Alternativas consideradas**:

- Usar somente a role presente no JWT ou nos metadados do provedor: rejeitada porque tokens já emitidos continuariam carregando permissões antigas.
- Revogar sessões do provedor ou reduzir a duração do JWT sem revalidar no serviço: rejeitada como mecanismo isolado; limita sessões futuras ou a janela de validade, mas não invalida imediatamente um JWT de acesso existente.
- Usar `user_metadata` como role: rejeitada porque o titular pode alterar metadados próprios pelo cliente.
- Consultar o endpoint do provedor em cada operação: rejeitada como fonte de estado corrente porque acrescenta latência e dependência de disponibilidade externa; a persistência PostgreSQL existente já é necessária ao sistema.

## Decisão 4: Logout imediato e revogação da sessão corrente

**Decisão**: antes de limpar a sessão do cliente e concluir o logout no Supabase, a aplicação envia o JWT corrente à API para registrar como revogado o identificador de sessão contido no token. A API recusa esse identificador em todas as requisições protegidas subsequentes. Se a revogação local não puder ser persistida, não apresentar logout como concluído; retornar erro seguro para que o cliente informe a falha. Encerrar a sessão no provedor impede a renovação futura do token.

**Justificativa**: sign-out do provedor revoga a sessão de autenticação/renovação, mas um JWT de acesso já emitido pode continuar criptograficamente válido até sua expiração. O marcador local satisfaz o requisito de rejeitar novas requisições da sessão encerrada sem depender de introspecção externa por operação.

**Alternativas consideradas**:

- Apenas apagar o token no navegador: rejeitada porque um token ainda válido continuaria aceito se reutilizado.
- Apenas revogar a sessão no Supabase: rejeitada como única barreira para JWTs de acesso já emitidos.
- Consultar o Supabase em todas as requisições: rejeitada por impor dependência remota no caminho de cada chamada.

## Decisão 5: Convites, administração e primeiro Administrador

**Decisão**: endpoints de gerenciamento da aplicação exigem sessão atual e perfil Administrador. A API chama a interface administrativa do Supabase usando uma credencial privilegiada obtida somente do ambiente de execução; nenhum segredo é entregue ao navegador. O perfil da aplicação só é criado ou atualizado com a identidade `sub` retornada pelo provedor. O primeiro Administrador é provisionado por procedimento único e controlado de implantação, com criação da identidade no Supabase e criação/vinculação de seu perfil `Administrador` na base da aplicação; não existe endpoint público ou regra de autoelevação.

**Justificativa**: o Supabase Admin API exige credencial privilegiada e a documentação do projeto proíbe expor segredos. Separar a identidade do perfil da aplicação mantém os dados de autorização e o estado de ativação verificáveis pelo serviço.

**Alternativas consideradas**:

- Incluir a credencial administrativa no bundle web ou enviá-la pelo navegador: rejeitada por conceder privilégios administrativos a clientes não confiáveis.
- Permitir que o primeiro usuário vire Administrador ao se cadastrar publicamente: rejeitada por permitir autoelevação e contrariar o escopo sem cadastro público.
- Aplicar migrações ou criar um Administrador implicitamente no startup: rejeitada porque transforma inicialização da aplicação em operação privilegiada e repetível.

## Decisão 6: Alteração do e-mail de uma conta

**Decisão**: quando o Administrador solicitar uma troca de e-mail, registrar o novo endereço como pendente e iniciar a confirmação exigida pelo provedor. Manter o endereço atual como identificador ativo de login até que o titular confirme a troca; depois, atualizar o endereço canônico da conta da aplicação. Reservar endereços pendentes para impedir que outra conta tente usá-los durante a confirmação.

**Justificativa**: o e-mail é a identidade de login e controlar a confirmação pelo titular reduz o risco de redirecionar a conta a uma caixa postal que o Administrador não controla.

**Alternativa rejeitada**: substituir o e-mail de login imediatamente, sem confirmação do novo endereço, porque isso pode bloquear o titular legítimo ou entregar a conta a terceiros.

## Decisões de implementação derivadas

- Usar `@supabase/supabase-js` com os auxiliares oficiais `@supabase/ssr` para sessão e fluxos de autenticação no App Router; a API continua recebendo o bearer JWT para validar chamadas protegidas.
- Padronizar e validar o e-mail antes de gravá-lo e aplicar unicidade sem distinção entre maiúsculas e minúsculas, para que uma mesma caixa postal não gere contas duplicadas.
- A API pode integrar com a interface Admin do Supabase por HTTP usando `HttpClient`; adicionar SDK .NET só se a pesquisa de implementação mostrar benefício claro sem alterar a arquitetura.
- Operações que alteram conta envolvem PostgreSQL e provedor externo, sem transação distribuída. Criar o convite antes do perfil local; se persistir o perfil falhar, compensar removendo a identidade recém-criada quando seguro e registrar erro explícito. Para atualizações, o estado PostgreSQL continua sendo a fonte de autorização; falhas ao sincronizar metadados do provedor devem ser notificadas e reconciliáveis, nunca apresentadas como sucesso completo.
- Provisionar o primeiro Administrador com um procedimento único e auditável: um operador autorizado cria/confirma a identidade no painel administrativo do Supabase, atribui o `app_metadata.role` de Administrador e cria o perfil vinculado ao `sub` na base de produção por acesso operacional restrito. Não incluir identificadores pessoais nem credenciais reais em migrations versionadas.
- Manter testes de integração automatizados independentes de projeto Supabase e credenciais reais; reservar teste ponta a ponta com projeto isolado para validação manual/de ambiente.
- Nunca baixar o prazo de expiração de access token abaixo do mínimo recomendado pelo provedor. O controle de acesso imediato vem da checagem local corrente e revogação de sessão, não de um TTL artificialmente curto.

## Referências oficiais

- Supabase, autenticação por senha: https://supabase.com/docs/guides/auth/passwords
- Supabase, autenticação server-side no Next.js: https://supabase.com/docs/guides/auth/server-side/nextjs
- Supabase, convite de usuário por e-mail: https://supabase.com/docs/reference/javascript/auth-admin-inviteuserbyemail
- Supabase, geração de link de convite/recuperação: https://supabase.com/docs/reference/javascript/auth-admin-generatelink
- Supabase, atualização administrativa de usuário e `app_metadata`: https://supabase.com/docs/reference/javascript/auth-admin-updateuserbyid
- Supabase, logout administrativo e revogação da sessão: https://supabase.com/docs/reference/javascript/auth-admin-signout
- Supabase, JWTs e claims: https://supabase.com/docs/guides/auth/jwts
- Supabase, chaves de assinatura e JWKS: https://supabase.com/docs/guides/auth/signing-keys
- Supabase, sessões e expiração de access token: https://supabase.com/docs/guides/auth/sessions
- Supabase, gerenciamento de dados do usuário: https://supabase.com/docs/guides/auth/managing-user-data
- Supabase, custom access token hook: https://supabase.com/docs/guides/auth/auth-hooks/custom-access-token-hook
- ASP.NET Core, configuração de autenticação JWT bearer: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-8.0
- ASP.NET Core, autorização por roles: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-8.0
