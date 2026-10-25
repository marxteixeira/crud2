# Research: Resolver bloqueios do fechamento de Identidade e perfis

## Decisão 1: Configuração de conexão nos testes
- **Decision**: fábricas de teste injetam uma string de conexão fictícia via configuração em memória.
- **Rationale**: o ambiente Testing usa fakes e nunca abre conexão; a exigência de `ConnectionStrings:Default` é validação de inicialização.
- **Alternatives**: depender de variável de ambiente (frágil localmente); relaxar a validação (reduz segurança, rejeitado).

## Decisão 2: Esquema Bearer duplicado
- **Decision**: a fábrica JWT isola a configuração e remove o registro prévio de Bearer antes de registrar o seu.
- **Rationale**: a falha só ocorre quando o desenvolvedor tem `Supabase:Url` em User Secrets (ambiente do usuário); no CI passaria. Deve ser determinística.
- **Alternatives**: mudar `Program.cs` para tolerar duplicação (altera produção sem necessidade); trocar o ambiente para Testing (perde o caminho JWT real).

## Decisão 3: Conectividade de persistência
- **Decision**: o teste usa banco descartável indicado por variável de ambiente e é ignorado com motivo explícito quando ausente; no CI roda sempre. Docker local: `postgres:16` na porta 5432.
- **Rationale**: o teste atual fixa `localhost` e quebra quando a configuração aponta para o Supabase.
- **Alternatives**: remover o teste (perde cobertura); apontar para o Supabase (viola o spec 004).

## Decisão 4: Desempenho (T039)
- **Decision**: teste in-process com 1.000 contas em memória, ≥ 100 requisições, medindo p95 ≤ 3 s.
- **Rationale**: reprodutível sem rede ou credenciais.

## Decisão 5: Itens manuais
- **Decision**: T036, T040 e a parte manual do T018 registrados como "concluídos pelo responsável (declaração do usuário em 2026-10-25)", sem dados pessoais.
- **Rationale**: instrução explícita do usuário; a evidência detalhada permanece com ele.
