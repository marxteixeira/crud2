# Modelo de dados conceitual: Fechamento da fase de identidade e perfis

Este modelo descreve os dados necessários ao registro documental de prontidão. Não representa tabelas de aplicação nem exige persistência adicional.

## Critério de saída

Representa uma condição obrigatória de conclusão ou pré-requisito da próxima fase.

| Campo | Descrição | Regra |
|---|---|---|
| `id` | Identificador estável do critério ou da tarefa de origem | Único no registro; preferir ID já existente, como T036 |
| `description` | Resultado esperado em linguagem verificável | Deve apontar para a tarefa ou critério fonte |
| `source` | Artefato de origem do roadmap, tarefa ou quickstart | Deve usar caminho relativo do repositório |
| `required` | Indica se bloqueia a transição | Critérios obrigatórios pendentes bloqueiam a fase seguinte |

## Evidência de validação

Representa o resultado observado para um critério.

| Campo | Descrição | Regra |
|---|---|---|
| `criterion_id` | Critério associado | Deve referenciar um critério existente |
| `validation` | Comando, cenário manual ou medição executada | Deve ser reproduzível sem registrar segredos |
| `result` | Estado da validação | Um de `approved`, `failed`, `blocked` ou `not-run` |
| `observed_at` | Data da execução | Deve identificar a atualidade da evidência |
| `summary` | Resultado agregado e causa conhecida | Sem tokens, credenciais, connection strings ou dados pessoais |
| `environment` | Características necessárias para reproduzir o teste | Descrever isolamento e dependências sem valores secretos |

## Pendência impeditiva

Representa um critério obrigatório sem aprovação.

| Campo | Descrição | Regra |
|---|---|---|
| `criterion_id` | Critério ainda não aprovado | Deve referenciar um critério obrigatório |
| `reason` | Falha observada, dependência ausente ou validação não executada | Distinguir falha de bloqueio ambiental |
| `next_action` | Ação necessária para resolver ou obter evidência | Deve ser verificável |
| `status` | Estado da pendência | Permanece aberta até evidência aprovada |

## Decisão de transição

Representa a conclusão da avaliação para a fase seguinte.

| Campo | Descrição | Regra |
|---|---|---|
| `target_phase` | Fase do roadmap avaliada | `Localização e produtos` |
| `decision` | Resultado da avaliação | `blocked` enquanto existir pendência impeditiva; `ready` somente quando todas as obrigatórias forem aprovadas |
| `evidence_refs` | Referências às evidências utilizadas | Deve cobrir todos os critérios obrigatórios |
| `reviewers` | Revisão independente | Dois revisores devem conseguir reproduzir a decisão |
| `recorded_at` | Data da decisão | Atualizar quando evidência atual contradizer o resultado anterior |

## Relacionamentos e transições

- Um critério pode ter várias evidências ao longo do tempo; a evidência atual que contradiz resultados anteriores prevalece até a causa ser resolvida.
- Uma evidência `failed`, `blocked` ou `not-run` para critério obrigatório mantém uma pendência impeditiva aberta.
- Uma pendência só pode ser encerrada após evidência atual `approved`.
- A decisão transita de `blocked` para `ready` somente quando todas as pendências impeditivas forem encerradas e todos os critérios obrigatórios estiverem cobertos por evidência aprovada.
