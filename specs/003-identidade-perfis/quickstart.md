# Quickstart — Validar identidade e perfis

Este guia descreve a validação ponta a ponta da fase de identidade e perfis. Cenários locais usam um projeto Supabase isolado e credenciais de teste; os testes automatizados de CI usam doubles de integração e não dependem de segredos de produção.

## Pré-requisitos

- .NET 8 SDK e Node.js 20.
- Dependências restauradas: `dotnet restore src/services/api/SistemaEstoque.sln` e `npm ci`.
- Banco PostgreSQL isolado disponível e configurado em `ConnectionStrings:Default`.
- Projeto Supabase Auth de desenvolvimento com URLs de redirecionamento de ativação configuradas para a origem local do web.
- Configuração local do provedor fornecida por User Secrets ou variáveis de ambiente (`Supabase__Url`, `Supabase__AnonKey`, `Supabase__ServiceRoleKey`, `NEXT_PUBLIC_SUPABASE_URL`, `NEXT_PUBLIC_SUPABASE_ANON_KEY`); nunca gravada em arquivos versionados. O segredo administrativo do provedor fica somente no processo da API.

## Preparar o ambiente local

1. Configure as opções locais obrigatórias do provedor e do banco usando User Secrets ou variáveis de ambiente, seguindo o README. Não reutilize tokens, senhas ou chaves de produção.
2. Aplique a migração de banco da funcionalidade por meio do processo de migração aprovado; não use o startup normal da aplicação para aplicar migrações automaticamente.
3. Para o primeiro Administrador, um operador de implantação autorizado cria/confirma a identidade no painel administrativo do Supabase e obtém o `sub` pela interface confiável do provedor. Em uma operação PostgreSQL restrita e transacional, cria o registro da conta com esse `identity_subject`, perfil `Administrador`, estado `Ativa` e autoria `bootstrap`; não define senha nem grava credenciais em SQL versionado. Verifica, antes de liberar o sistema, que o registro aponta para a identidade correta e que existe pelo menos um Administrador ativo. Registra apenas a conclusão operacional, sem e-mail, identificador pessoal ou segredo. A API não executa esse bootstrap automaticamente no startup.
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
9. Tente entrar com uma identidade Supabase vinculada a uma conta inativa. Mesmo que o provedor emita uma sessão, a API deve negar a identidade e o web deve limpar a sessão local, negar as áreas protegidas e mostrar mensagem genérica.

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
- Após aquecimento e pelo menos 100 requisições, a listagem paginada responde em até 3 segundos no percentil 95 para 1.000 contas no ambiente de teste documentado.
- Nenhum teste requer credenciais reais nem registra tokens, senhas ou segredos administrativos.

## Validação de usabilidade

Validar com pelo menos cinco participantes representando os perfis de Administrador e Operador. Solicitar que identifiquem como entrar e sair e, a um Administrador, que provisionem, localizem, editem e inativem uma conta. Registrar tempos e conclusão sem ajuda; os objetivos são até 3 minutos para o fluxo administrativo e pelo menos 90% de conclusão sem assistência. Guardar apenas resultados agregados, sem nomes, e-mails ou outros dados pessoais.

## Verificação de configuração e privacidade

- Confirme que valores obrigatórios ausentes ou inválidos impedem o início da API com mensagem segura.
- Confirme que o segredo de administração do provedor existe apenas no ambiente servidor e não aparece na resposta de configuração pública do web, nos bundles, logs, testes ou arquivos versionados.
- Confirme que respostas de consulta de usuário nunca incluem senha, tokens, segredo administrativo ou detalhes internos do provedor.
