# Pesquisa: Melhorar a experiência de Localizações

## Decisões

### Usar o componente de diálogo existente

- **Decisão**: Apresentar o formulário de criação da estante no componente compartilhado `Dialog`, aberto pelo botão "+ Cadastrar Nova Estante".
- **Racional**: A aplicação já usa esse componente em outras páginas, incluindo fluxo de convite de usuário. Ele oferece semântica de diálogo modal, associação acessível ao título e fechamento por Escape, consistente com os padrões existentes.
- **Alternativas consideradas**: Criar um novo componente modal ou usar um link de âncora para mostrar o formulário na página. A primeira alternativa duplicaria infraestrutura existente; a segunda não atenderia ao requisito de modal.

### Reutilizar mutação e atualização existentes

- **Decisão**: Manter a criação via mutação `createRack`, selecionar o identificador retornado e invalidar as consultas de Localizações; fechar o modal e limpar o formulário somente depois da conclusão bem-sucedida.
- **Racional**: A página já atualiza consultas após mutações e seleciona o identificador da estante criada. A invalidação da consulta ativa recarrega os dados sem uma atualização completa do navegador, e o ID selecionado permite que a estante recém-criada permaneça em foco.
- **Alternativas consideradas**: Fazer navegação ou recarregamento completo do documento; isso é desnecessário e perde a experiência de aplicação de página única. Usar os dados de retorno diretamente sem invalidar a consulta; isso diverge do padrão usado pelas demais mutações da página.

### Remover somente a criação redundante de prateleiras

- **Decisão**: Retirar o formulário e a ação de cadastro de prateleira, preservando listagem, edição, situação e consulta de produtos vinculados.
- **Racional**: A solicitação nomeia especificamente o campo "Código da prateleira" e o botão "Cadastrar prateleira". As ações existentes sobre prateleiras têm propósitos distintos e permanecem necessárias à consulta e manutenção de registros existentes.
- **Alternativas consideradas**: Remover toda a seção de prateleiras ou seus demais controles; essas opções excederiam o escopo e contradiriam a necessidade de preservar a consulta de prateleiras cadastradas.

### Interpretar os campos Código e Descrição como os do cadastro de estante

- **Decisão**: Os campos que devem iniciar vazios e sem texto de exemplo são os do novo cadastro de estante, agora dentro do modal. Os campos do editor de uma estante já selecionada continuam exibindo seus valores salvos.
- **Racional**: O cadastro de nova estante já possui exatamente os campos Código e Descrição; o editor de uma estante existente usa esses mesmos rótulos para mostrar dados persistidos, que não são texto de exemplo e precisam continuar presentes para edição.
- **Alternativas consideradas**: Limpar os campos do editor de estante existente; isso ocultaria dados atuais e tornaria a edição ambígua. Acrescentar um campo Descrição ao formulário de prateleira; não existe campo de descrição no modelo atual de prateleira e isso expandiria o escopo de dados.

## Padrões e restrições relevantes

- A implementação atual está em `src/apps/web/app/locations/LocationsPageClient.tsx`; acesso a estantes e prateleiras passa por `src/apps/web/lib/locations` e estado de servidor por TanStack Query.
- O padrão de diálogo é `src/apps/web/components/ui/Dialog.tsx`, já usado por `src/apps/web/app/users/UsersPageClient.tsx`.
- O teste focal existente é `tests/web/LocationsPage.test.tsx`; a suíte usa Vitest e Testing Library.
- A tela de referência e seu protótipo existentes são `docs/05-design-system/estantes_e_prateleiras/screen.png` e `docs/05-design-system/estantes_e_prateleiras/code.html`. A especificação do usuário determina as diferenças intencionais para este ajuste.
- Nenhuma dependência, API pública, entidade persistida ou migração é necessária.
