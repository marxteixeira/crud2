# Relatório de prontidão: identidade e perfis

**Fase seguinte avaliada**: Localização e produtos  
**Estado da decisão**: `pronto`  
**Atualizado**: 2026-10-25 (spec 006)

## Critérios e evidências

| Referência | Critério | Estado atual | Origem | Evidência resumida |
|---|---|---|---|---|
| 003 T036 | Cenários manuais de identidade | `aprovado` | declarado pelo responsável (2026-10-25) | Integração Supabase validada em localhost: administrador criado e convite enviado a operador; demais cenários declarados concluídos pelo responsável. Sem dados pessoais registrados aqui. |
| 003 T037 | Build, testes da API e gates web | `aprovado` | automatizado | `dotnet build` 0 erros/0 avisos; domínio 12/12; API 64 testes, 64 aprovados com PostgreSQL descartável (63 aprovados e 1 ignorado sem banco); `npm run lint`, `npm test` (18/18) e `npm run build` aprovados. |
| 003 T039 | Desempenho com 1.000 contas | `aprovado` | automatizado | `UserManagementPerformanceTests`: 1.000 contas, 100 requisições após aquecimento, p95 ≤ 3 s (execução total ≈ 0,5–0,7 s). Medido sobre o serviço com banco em memória; não inclui latência de rede. |
| 003 T040 | Usabilidade (≥ 5 participantes) | `aprovado` | declarado pelo responsável (2026-10-25) | Resultados agregados mantidos pelo responsável. |
| 004 T018 | Validação da configuração do banco | `aprovado` | automatizado + declarado | Teste de conectividade passou contra PostgreSQL 16 descartável em Docker; parte manual declarada pelo responsável. |

## Correções aplicadas (spec 006)

- Grupo A (26 → 0): `appsettings.Testing.json` ganhou `ConnectionStrings:Default` fictícia (sem segredo).
- Grupo B: `RoleAuthorizationTests` usa o ambiente `Testing`, evitando o registro duplicado do esquema Bearer causado por User Secrets locais.
- Grupo C: `PersistenceConnectivityTests` usa `ConnectionStrings__Default` de banco em `localhost`; é ignorado com motivo explícito se ausente ou remoto.
- Nenhuma regra de negócio ou de autenticação em produção foi alterada.

## Decisão

**`pronto`** — critérios aprovados; a fase "Localização e produtos" pode iniciar. Ressalvas: T036, T040 e a parte manual do T018 dependem da declaração do responsável; o teste de conectividade só é executado com banco local configurado (sempre no CI). Não há segredos ou dados pessoais neste registro.

## Revisão independente

Pendente de revisão pelo responsável do produto.
