# Quickstart — Validar identidade e perfis

Este guia descreve a validação ponta a ponta da fase de identidade e perfis. Cenários locais usam um projeto Supabase isolado e credenciais de teste; os testes automatizados de CI usam doubles de integração e não dependem de segredos de produção.

## Pré-requisitos

- .NET 8 SDK e Node.js 20.
- Dependências restauradas: `dotnet restore src/services/api/SistemaEstoque.sln` e `npm ci`.
- Banco PostgreSQL isolado disponível e configurado em `ConnectionStrings:Default`.
- Projeto Supabase Auth de desenvolvimento com URLs de redirecionamento de ativação configuradas para a origem local do web.
- Configuração local do provedor fornecida por User Secrets ou variáveis de ambiente (`Supabase__Url`, `Supabase__ServiceRoleKey`, `NEXT_PUBLIC_SUPABASE_URL`, `NEXT_PUBLIC_SUPABASE_ANON_KEY`); nunca gravada em arquivos versionados. O segredo administrativo do provedor fica somente no processo da API.

## Preparar o ambiente local

1. Configure as opções locais obrigatórias do provedor e do banco usando User Secrets ou variáveis de ambiente, seguindo o README. Não reutilize tokens, senhas ou chaves de produção.
2. Aplique a migração de banco da funcionalidade por meio do processo de migração aprovado; não use o startup normal da aplicação para aplicar migrações automaticamente.
3. Para o primeiro Administrador, um operador autorizado cria/confirma a identidade no painel administrativo do Supabase, atribui o perfil de Administrador nos metadados administrativos e cria o registro vinculado ao `sub` por acesso operacional restrito à base. Não inclua e-mails reais, identificadores de produção ou credenciais em migrations e arquivos versionados. Confirme que a conta está ativa antes de convidar outros usuários.
4. Inicie a API:

   ```powershell
   dotnet run --project src/services/api/SistemaEstoque.API --urls http://localhost:5013
   ```

5. Em outro terminal, inicie a aplicação web:

   ```powershell
   npm run dev
   ```

## Cenários ponta a ponta

1. Abra a página de login no web e entre com o e-mail da conta Administrador de teste. Confirme que o usuário autenticado pode abrir o gerenciamento de usuários.
2. Convide um usuário Operador com e-mail próprio. Conclua a ativação no e-mail de teste e estabeleça uma senha sem que ela seja exibida ao Administrador.
3. Em janela separada, entre como Operador. Confirme que as operações de gerenciamento de usuários e de Administrador são recusadas, e que o web mostra somente áreas permitidas.
4. Com o Administrador, altere o perfil da conta Operador para Administrador e faça novamente uma requisição com o token já emitido para a sessão aberta do usuário. A requisição seguinte deve usar as permissões atualizadas.
5. Como Administrador, inative essa conta. Confirme que uma requisição protegida posterior com token já emitido é recusada e que o registro continua consultável por um Administrador.
6. Encerre uma sessão autenticada. Confirme que qualquer uso posterior do token daquela sessão nas operações protegidas retorna `401`.
7. Solicite a troca do e-mail de uma conta e confirme que o endereço ativo permanece válido até o titular confirmar o novo endereço.
8. Tente convidar novamente um e-mail existente e tente inativar/rebaixar o último Administrador ativo. As operações devem ser recusadas sem alteração parcial dos dados.

Os campos, estados e invariantes de conta estão descritos em [data-model.md](data-model.md); as rotas e respostas HTTP estão em [contracts/user-management-api.yaml](contracts/user-management-api.yaml).

## Validação automatizada

Na raiz do repositório:

```powershell
dotnet build src/services/api/SistemaEstoque.sln
dotnet test src/services/api/SistemaEstoque.sln
npm run lint
npm test
npm run build
```

Resultados esperados:

- API aceita JWTs válidos do emissor e público configurados e rejeita tokens ausentes, malformados, expirados, revogados ou de contas inativas.
- Testes de autorização provam que somente Administradores gerenciam contas; mudanças de perfil e situação valem para a próxima requisição autenticada sem exigir novo login.
- Testes de regras impedem e-mail duplicado e a remoção do último Administrador ativo.
- Testes web cobrem login, logout, ativação e gestão de usuários com mensagens compreensíveis e controles acessíveis.
- Nenhum teste requer credenciais reais nem registra tokens, senhas ou segredos administrativos.

## Verificação de configuração e privacidade

- Confirme que valores obrigatórios ausentes ou inválidos impedem o início da API com mensagem segura.
- Confirme que o segredo de administração do provedor existe apenas no ambiente servidor e não aparece na resposta de configuração pública do web, nos bundles, logs, testes ou arquivos versionados.
- Confirme que respostas de consulta de usuário nunca incluem senha, tokens, segredo administrativo ou detalhes internos do provedor.
