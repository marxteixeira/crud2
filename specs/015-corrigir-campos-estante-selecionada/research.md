# Research: Campos da estante selecionada

## Decisão 1: Remontar o formulário por estante

- **Decision**: adicionar `key={selectedRack.id}` ao `<form aria-label="Editar estante ...">` em `LocationsPageClient.tsx`, mantendo os inputs com `defaultValue`.
- **Rationale**: `defaultValue` só vale na montagem do input. Ao trocar de estante, o React reaproveita o mesmo `<form>` e os inputs mantêm o valor antigo. Uma `key` por estante remonta os inputs com os valores da estante escolhida, descarta alterações não salvas (FR-005) e cobre seleção automática (abertura e pós-cadastro, FR-004).
- **Alternatives considered**:
  - Inputs controlados com estado e `useEffect` de sincronização: mais código e risco de sobrescrever digitação durante refetch.
  - `key` com id + código/descrição: remontaria o formulário a cada refetch e apagaria a digitação em andamento; descartado.

## Decisão 2: Estratégia de teste

- **Decision**: teste em `tests/web/LocationsPage.test.tsx` com duas estantes de código/descrição distintos; trocar o seletor e verificar os valores dos campos (inclusive descrição vazia e retorno à estante anterior).
- **Rationale**: reproduz a regressão relatada (FR-008).

Sem NEEDS CLARIFICATION pendentes.
