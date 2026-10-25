# Data Model: Resolver bloqueios do fechamento de Identidade e perfis

Feature de verificação; sem novas entidades persistidas.

## Critério de saída
- **Campos**: id (T036, T037, T039, T040, T018), status (`aprovado` | `pendente` | `falhou`), origem (`automatizado` | `declarado pelo responsável`), data, resumo anonimizado.
- **Regra**: `aprovado` exige evidência; itens declarados pelo responsável ficam identificados como tal.

## Decisão de prontidão
- **Valores**: `pronto` | `bloqueada`.
- **Transição**: `bloqueada` → `pronto` somente quando todos os critérios forem `aprovado`.

## Dados do teste de desempenho
- 1.000 contas sintéticas (identificador, e-mail fictício, perfil, status) criadas apenas em memória; nenhuma é persistida.
