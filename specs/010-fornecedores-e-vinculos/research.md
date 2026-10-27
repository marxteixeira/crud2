# Pesquisa: Fornecedores e vínculos

**Feature**: [spec.md](spec.md)

## Decisões

### Persistir fornecedores e vínculos no PostgreSQL existente

- **Decisão**: Usar `SistemaEstoqueDbContext`, configurações EF Core e migração explícita já adotados para `Produto`, `Estante` e `Prateleira`.
- **Racional**: Mantém a fonte de dados do catálogo única, respeita a arquitetura em camadas e permite impor unicidade e integridade referencial na persistência.
- **Alternativas consideradas**: Criar serviço ou banco independente; rejeitado por duplicar infraestrutura e contrariar o monorepo e as decisões de arquitetura existentes.

### Normalizar e validar CPF/CNPJ sem biblioteca adicional

- **Decisão**: Remover caracteres de apresentação, validar o comprimento e os dígitos verificadores conforme o tipo (CPF ou CNPJ) e persistir a sequência canônica de dígitos em um campo único. Aplicar validação na entrada e invariante no domínio; garantir unicidade com índice único no banco.
- **Racional**: CPF e CNPJ têm algoritmos públicos determinísticos; normalização mais restrição persistente evita duplicidade por pontuação e condições de corrida sem introduzir dependência ou serviço externo.
- **Alternativas consideradas**: Comparar texto formatado; rejeitado porque permite cadastrar repetidamente o mesmo identificador com pontuações diferentes. Consumir serviço remoto ou biblioteca; não necessário para validação estrutural e indisponível offline.

### Modelar vínculo com auditoria e inativação lógica

- **Decisão**: Representar produto-fornecedor por entidade própria com estado ativo/inativo e autoria/horário de criação e última alteração. Desassociar inativa o vínculo; novas associações exigem produto e fornecedor ativos.
- **Racional**: A especificação exige consultar nos dois lados e registrar quem/quando altera vínculos. Inativação mantém a trilha sem impedir que o vínculo deixe de aparecer nas consultas operacionais.
- **Alternativas consideradas**: Tabela de junção removida fisicamente; rejeitada porque elimina o histórico de desassociação. Histórico separado de eventos; não necessário para o escopo.

### Inativar fornecedor somente sem produtos ativos associados

- **Decisão**: Bloquear a inativação quando existir vínculo ativo com produto ativo. Permitir desassociação ou inativação do produto antes; preservar vínculos históricos.
- **Racional**: Segue RN-08 e o critério de saída da Fase 3 no roadmap; um fornecedor pode deixar de ser opção para novos vínculos sem quebrar referências históricas.
- **Alternativas consideradas**: Remover vínculos automaticamente ao inativar; rejeitado por alteração implícita de dados e perda de controle/auditoria.

### Reutilizar autorização, paginação, validação e apresentação existentes

- **Decisão**: Aplicar política de conta ativa na API, perfis existentes para mutações e consulta autorizada para os dois perfis; manter filtros e paginação na URL, TanStack Query para estado remoto e acesso à API através de `lib/`.
- **Racional**: Alinha-se aos padrões de `ProductsController`, contratos do catálogo e princípios constitucionais VI e VII.
- **Alternativas consideradas**: Fazer validação somente na interface ou criar um segundo padrão de API; rejeitado porque não protege acesso direto e fragmenta a aplicação.

## Referências internas consultadas

- `docs/01-negocio/documento-de-negocio.md`: RF-06 e RF-10 a RF-15; RN-05, RN-06, RN-08 e RN-10.
- `specs/002-implementation-roadmap/spec.md`: Fase 3 — Fornecedores e vínculos, dependências e critérios de saída.
- `specs/007-localizacao-produtos/spec.md` e `specs/007-localizacao-produtos/plan.md`: limites de catálogo e regras de localização.
- `specs/007-localizacao-produtos/data-model.md` e `contracts/catalog-api.yaml`: padrões de estado, auditoria, paginação, respostas e autenticação.
- `src/services/api/SistemaEstoque.Domain/Entities/Produto.cs` e `ProdutoConfiguration.cs`: invariantes, auditoria e configuração EF existentes.
- `src/services/api/SistemaEstoque.API/Controllers/ProductsController.cs` e `src/services/api/SistemaEstoque.Application/Products/ProductContracts.cs`: padrões de endpoint e DTO paginado.
- `.specify/memory/constitution.md`: arquitetura, testes obrigatórios, validação, autorização e segurança de dados.

## Premissas para implementação

- CPF/CNPJ é obrigatório para fornecedor e sua forma canônica é única globalmente.
- Telefone, e-mail, contato e observações são opcionais, conforme a especificação.
- A atribuição e remoção de vínculos segue as permissões de edição do produto já existentes; alterações administrativas do cadastro de fornecedor são exclusivas de Administrador.
- Fornecedores inativos permanecem consultáveis; seus vínculos históricos são exibidos nos detalhes, mas não permitem novas associações.
- Filtragem por fornecedor respeita os demais filtros e a situação selecionada, com a mesma paginação do catálogo.
