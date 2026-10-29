# Data Model: Adoção do Design System

Não há mudança no modelo de dados do sistema. As entidades abaixo descrevem os registros de controle mantidos em [execution-control.md](./execution-control.md).

## Tela

| Campo | Descrição |
|-------|-----------|
| id | `T1`…`T7` ou `Dn` para derivada |
| rota | Rota do sistema |
| prototipo | Pasta em `docs/05-design-system/` (ou protótipo-base se derivada) |
| fase | `F1`…`F5` |
| status | Não iniciada · Em andamento · Bloqueada · Concluída |
| checklist | Alinhada ao print · Funções preservadas · Estados · Celular · Teclado/contraste |

## Fase

| Campo | Descrição |
|-------|-----------|
| id | `F0`…`F6` |
| dependências | Fases que devem estar Concluídas |
| status / responsável / início / conclusão | Controle de execução |
| evidência | Capturas lado a lado, saída dos testes, lista de diferenças aceitas |

Transições: Não iniciada → Em andamento (dependências concluídas) → Concluída (todos os critérios de saída e evidência) ; Em andamento → Bloqueada → Em andamento.

## Divergência

| Campo | Descrição |
|-------|-----------|
| id | `D1`… |
| descrição | Diferença protótipo × negócio ou documental |
| decisão | Pendente · Fora do escopo · Aceita · Resolvida |
