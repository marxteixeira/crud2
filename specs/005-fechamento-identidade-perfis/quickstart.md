# Quickstart: avaliar prontidão de identidade e perfis

Este guia verifica se a fase de identidade/perfis pode ser encerrada e se localização e produtos pode começar. Use somente banco e projeto Supabase isolados para verificações que necessitem de serviços externos. Nunca inclua segredos, tokens, connection strings ou dados pessoais nos registros.

## Pré-requisitos

- .NET 8 SDK e Node.js 20 instalados; dependências do repositório restauradas.
- Serviço PostgreSQL descartável do CI ou outro banco isolado disponível para os testes que o exigirem.
- Projeto Supabase de desenvolvimento isolado somente para o roteiro manual de identidade.
- Critérios do roadmap em `specs/002-implementation-roadmap/spec.md`.

## 1. Executar gates automatizados

Na raiz do repositório, execute sequencialmente:

```powershell
dotnet build src/services/api/SistemaEstoque.sln
dotnet test src/services/api/SistemaEstoque.sln
npm run lint
npm test
npm run build
```

Registre para cada comando o código de saída, o total aprovado/falho e uma síntese sem segredos. Evite executar build e testes .NET simultaneamente no mesmo checkout, pois builds concorrentes podem disputar arquivos intermediários.

**Aprovação**: build, testes da solução e todos os comandos web terminam com sucesso. Uma falha de configuração ou serviço deve ser reproduzida com dependências de teste isoladas; não marque a suíte como aprovada até que os testes requeridos passem ou a causa esteja corrigida e validada.

## 2. Completar validações de identidade e perfis

Siga `specs/003-identidade-perfis/quickstart.md` no ambiente isolado e registre resultado para cada cenário manual. Inclua autenticação dos dois perfis, alteração de perfil com sessão já aberta, inativação, revogação, restrições do Operador e proteção do último Administrador. Não registre e-mails, IDs de identidade, senhas ou credenciais.

Para desempenho, execute a listagem paginada com 1.000 contas de teste; aqueça o ambiente e meça pelo menos 100 requisições. Registre ambiente, método e p95 agregado. O limite de aprovação é p95 de até 3 segundos.

Para usabilidade, use pelo menos cinco participantes conforme o roteiro existente. Registre tempos e taxas agregadas: fluxos administrativos em até 3 minutos e pelo menos 90% de conclusão sem ajuda. Não registre nomes nem dados pessoais.

## 3. Confirmar prontidão da persistência

Siga `specs/004-supabase-database-config/quickstart.md` em banco isolado e valide os comandos e cenários definidos em T018. Demonstre conectividade e readiness, tratamento seguro de falhas, configuração TLS exigida para produção e que as migrações são explícitas. Não conecte testes a produção nem use a credencial administrativa como identidade regular da API.

## 4. Decidir a transição

Compare evidências e tarefas abertas em:

- `specs/003-identidade-perfis/tasks.md` — especialmente T036, T037, T039 e T040.
- `specs/004-supabase-database-config/tasks.md` — especialmente T018.
- `specs/002-implementation-roadmap/spec.md` — critérios de entrada de Localização e produtos.

**Resultado atual conhecido**: o build da API, os 12 testes de domínio, o lint, os 18 testes web e o build web foram reportados como aprovados. Na execução reportada, 26 dos 63 testes API falharam. Entre as causas relatadas estão connection string de teste ausente, esquema de autenticação duplicado e divergência de configuração no teste de conectividade. T036, T039, T040 e T018 ainda não têm aprovação registrada.

**Decisão com esse baseline**: `blocked`. Não iniciar a fase de localização e produtos como implementação até que as falhas sejam resolvidas e todas as validações obrigatórias tenham evidência aprovada. Se uma falha depender de ambiente indisponível, registrar como bloqueada, não aprovada, e repetir em ambiente adequado.
