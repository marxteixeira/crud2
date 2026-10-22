# ADR-0001: Monorepo modular e estrutura inicial

- **Status:** Aceito

## Contexto
Frontend (Next.js) e backend (.NET 8) evoluem juntos e são desenvolvidos com apoio de IA, que se beneficia de contexto unificado.

## Decisão
- Monorepo: `src/apps/web` (Next.js), `src/services/api` (solução .NET em camadas `SistemaEstoque.*`), testes em `tests/`.
- Camadas: Domain (sem dependências) <- Application <- Infrastructure <- API.
- Walking skeleton: `GET /health` consumido pela página inicial do web.
- CI único em `.github/workflows/ci.yml` com jobs `api` e `web`.

## Consequências
A configuração de testes do web aponta para `tests/web` (fora da pasta do app).
