# crud2 – Sistema de Controle de Produtos e Fornecedores

Projeto de aprendizado do fluxo de desenvolvimento assistido por IA.

## Estrutura

| Pasta/arquivo | Para que serve | Conteúdo atual |
|---|---|---|
| `.github/copilot-instructions.md` | Instruções permanentes que a IA lê sempre; aponta para os docs | Criado |
| `.github/instructions/` | Regras por tipo de arquivo (`*.instructions.md` com `applyTo`), ex.: regras só para `.cs` ou `.tsx` | Vazio |
| `.github/prompts/` | Prompts reutilizáveis (`*.prompt.md`), ex.: "criar spec", "revisar" | Vazio |
| `docs/01-negocio/` | **O quê e por quê**: problema, escopo, regras de negócio, critérios de aceite | `documento-de-negocio.md` |
| `docs/02-arquitetura/` | **Como**: stack, modelagem, auth, estrutura do repo, diretrizes para a IA | `especificacao-de-arquitetura.md` |
| `docs/03-requisitos/` | Requisitos refinados/derivados (ex.: backlog, épicos, user stories) | Vazio |
| `docs/04-decisoes-adr/` | Registro de decisões (ADR), uma por arquivo | Vazio |
| `specs/` | Uma spec por funcionalidade (entrada direta para a IA implementar) | Vazio |
| `src/` | Código (backend/frontend, conforme a arquitetura) | Vazio |
| `tests/` | Testes | Vazio |

## Fluxo sugerido
1. Negócio + arquitetura (feito) →
2. Quebrar o MVP em funcionalidades (`docs/03-requisitos/`) →
3. Escrever uma spec por funcionalidade (`specs/`) →
4. Pedir à IA que implemente a spec em `src/` com testes em `tests/` →
5. Revisar, e registrar decisões relevantes em ADR.

Arquivos `.gitkeep` apenas mantêm pastas vazias no Git; podem ser removidos quando a pasta tiver conteúdo.
