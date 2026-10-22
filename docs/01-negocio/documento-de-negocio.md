# Documento de Negócios – Sistema de Controle de Produtos e Fornecedores

*Versão 1.0 · Visão de negócio para orientar o desenvolvimento de um sistema CRUD simples*

## 1. Resumo executivo

A loja controla hoje seus produtos em uma planilha. Isso funciona enquanto o catálogo é pequeno e uma única pessoa mexe nos dados, mas gera problemas recorrentes: informações duplicadas, preços desatualizados, quantidades que não batem com a prateleira, dificuldade de achar onde o item está guardado e nenhuma visão clara de quem fornece o quê.

Este documento define o que o novo sistema precisa fazer, **do ponto de vista do negócio**, e o que fica de fora de propósito. O princípio que guia tudo é a **simplicidade**: o sistema substitui a planilha com mais segurança e praticidade, sem virar um ERP.

**Em uma frase:** um sistema web simples para cadastrar, consultar, alterar e inativar produtos (com quantidade, preço e localização por estante e prateleira) e fornecedores, ligando uns aos outros.

## 2. Contexto e problema

| Dor atual (planilha) | Consequência para o negócio |
| --- | --- |
| Qualquer pessoa edita qualquer célula, sem histórico | Erros silenciosos, sem saber quem mudou o quê |
| Nomes de produto digitados de formas diferentes | Duplicidade e contagem errada |
| Local de armazenamento em texto livre | Tempo perdido procurando o produto |
| Fornecedor misturado em coluna ou em outra aba | Dificuldade para recomprar e negociar |
| Sem regras (preço negativo, quantidade em texto) | Dados inconsistentes |
| Arquivo único, risco de perda ou de versões conflitantes | Retrabalho e insegurança |

## 3. Objetivos de negócio

1. Ter **uma única fonte confiável** de produtos, quantidades, preços e localização.
2. **Reduzir o tempo** para encontrar um produto fisicamente (estante + prateleira).
3. **Saber quem fornece cada produto** e conseguir recomprar rapidamente.
4. Reduzir erros de digitação e duplicidades por meio de validações.
5. Manter o uso tão simples que qualquer funcionário aprenda em minutos.

### Indicadores de sucesso

- 100% dos produtos da planilha migrados, sem duplicidades.
- Tempo médio para localizar um produto no sistema inferior a 10 segundos.
- Zero produtos ativos sem localização ou sem preço.
- Funcionários usando o sistema no dia a dia em até 2 semanas após a entrada em produção.

## 4. Escopo

### 4.1 Dentro do escopo (MVP)

- Cadastro, consulta, edição e inativação de **produtos**.
- Cadastro, consulta, edição e inativação de **fornecedores**.
- Vínculo entre produtos e fornecedores.
- Localização do produto por **estante e prateleira**.
- Busca e filtros simples.
- Acesso com login e dois perfis simples.

### 4.2 Fora do escopo (por enquanto)

- Vendas, caixa, PDV e emissão de nota fiscal.
- Compras automatizadas e integração com sistemas dos fornecedores.
- Financeiro (contas a pagar e a receber).
- Múltiplas lojas ou filiais.
- Loja virtual ou integração com marketplaces.

## 5. Perfis de usuário

| Perfil | Quem é | O que pode fazer |
| --- | --- | --- |
| **Administrador** | Dono ou gerente | Tudo, incluindo gerenciar usuários, alterar preços e inativar cadastros |
| **Operador** | Estoquista ou atendente | Consultar, cadastrar e editar produtos e quantidades; não altera preço nem gerencia usuários |


## 6. Glossário de negócio

- **Produto:** item vendido ou guardado pela loja.
- **Fornecedor:** empresa ou pessoa que vende produtos para a loja.
- **Estante:** móvel ou módulo de armazenamento, identificado por código (ex.: A, B, 01).
- **Prateleira:** nível dentro de uma estante, identificado por número ou código (ex.: 1, 2, 3).
- **Localização:** combinação estante + prateleira (ex.: Estante B, Prateleira 3).
- **Inativar:** tirar o cadastro de uso sem apagar o histórico.

## 7. Requisitos funcionais

### 7.1 Produtos

| ID | Requisito | Prioridade |
| --- | --- | --- |
| RF-01 | Cadastrar produto com: **nome, quantidade, preço, estante e prateleira** | Essencial |
| RF-02 | Consultar lista de produtos com busca por nome | Essencial |
| RF-03 | Filtrar lista por estante, prateleira, fornecedor e situação (ativo/inativo) | Essencial |
| RF-04 | Editar qualquer dado do produto | Essencial |
| RF-05 | Inativar produto (exclusão lógica); reativar quando necessário | Essencial |
| RF-06 | Vincular um ou mais fornecedores ao produto | Essencial |
| RF-07 | Exibir detalhe do produto com localização e fornecedores | Essencial |
| RF-08 | Excluir definitivamente apenas produtos criados por engano, sem histórico (somente Administrador) | Desejável |

### 7.2 Fornecedores

| ID | Requisito | Prioridade |
| --- | --- | --- |
| RF-10 | Cadastrar fornecedor com: **nome/razão social, CNPJ ou CPF, telefone, e-mail, contato (pessoa) e observações** | Essencial |
| RF-11 | Consultar lista de fornecedores com busca por nome | Essencial |
| RF-12 | Editar dados do fornecedor | Essencial |
| RF-13 | Inativar fornecedor (não pode ser excluído se houver produtos vinculados) | Essencial |
| RF-14 | Ver, no detalhe do fornecedor, a lista de produtos que ele fornece | Essencial |
| RF-15 | Vincular e desvincular produtos a partir da tela do fornecedor | Essencial |

### 7.3 Localização (estante e prateleira)

| ID | Requisito | Prioridade |
| --- | --- | --- |
| RF-20 | Informar a estante e a prateleira do produto por **seleção de lista** (evita digitação livre) | Essencial |
| RF-21 | Manter um cadastro simples de estantes e prateleiras (ex.: estante B com 5 prateleiras) | Essencial |
| RF-22 | Consultar “o que há na Estante B, Prateleira 3” | Essencial |

### 7.4 Acesso e dados

| ID | Requisito | Prioridade |
| --- | --- | --- |
| RF-30 | Login com usuário e senha | Essencial |


## 8. Regras de negócio

- **RN-01:** o nome do produto é obrigatório e **não pode se repetir** (comparação sem diferenciar maiúsculas e acentos).
- **RN-02:** a quantidade é um número inteiro maior ou igual a zero. *(Se a loja vende por peso ou metro, ver pergunta em aberto Q-03.)*
- **RN-03:** o preço é obrigatório, maior ou igual a zero, com duas casas decimais, em reais.
- **RN-04:** todo produto ativo deve ter estante e prateleira; a prateleira pertence a uma estante existente.
- **RN-05:** um produto pode ter **zero, um ou vários fornecedores**; um fornecedor pode fornecer **vários produtos** (relação muitos-para-muitos).
- **RN-06:** CNPJ/CPF do fornecedor deve ser válido e único.
- **RN-07:** cadastros são **inativados, não apagados**, para preservar histórico e evitar perda de informação.
- **RN-08:** fornecedor com produtos ativos vinculados não pode ser inativado sem antes tratar os vínculos (mensagem clara ao usuário).
- **RN-09:** somente o Administrador altera preços.
- **RN-10:** toda alteração relevante registra **quem alterou e quando**.

## 9. Modelo de informação (conceitual)

**Produto** — nome, quantidade, preço, estante, prateleira, situação (ativo/inativo), datas de criação e alteração.

**Fornecedor** — nome/razão social, CNPJ/CPF, telefone, e-mail, contato, observações, situação.

**Estante** — código/nome. **Prateleira** — número/código, pertence a uma estante.

**Produto ↔ Fornecedor** — vínculo muitos-para-muitos.

**Usuário** — nome, login, perfil.

```
Estante 1 ──── N Prateleira 1 ──── N Produto N ──── N Fornecedor
```

## 10. Telas principais (visão de negócio)

1. **Login.**
2. **Lista de produtos:** busca no topo, filtros (estante, prateleira, fornecedor, situação), botão “Novo produto”.
3. **Formulário de produto:** nome, quantidade, preço, estante, prateleira, fornecedores.
4. **Lista de fornecedores:** busca e botão “Novo fornecedor”.
5. **Formulário de fornecedor:** dados cadastrais e produtos vinculados.
6. **Cadastro de estantes e prateleiras:** tela simples de apoio.
7. **Importação da planilha.**

Diretriz de usabilidade: no máximo **três cliques** para cadastrar ou achar um produto; mensagens de erro em linguagem comum; funcionar bem em celular, pois o estoquista pode consultar em frente à estante.

## 11. Requisitos não funcionais

- **Simplicidade:** interface enxuta, sem menus profundos nem termos técnicos.
- **Acesso:** navegador, em computador e celular.
- **Segurança:** login obrigatório; senhas protegidas; acesso por perfil.
- **Backup:** cópia automática diária dos dados, com possibilidade de restaurar.
- **Desempenho:** buscas respondem em até 2 segundos para catálogos de até alguns milhares de produtos.
- **Disponibilidade:** uso em horário comercial; indisponibilidades curtas são toleráveis.
- **Privacidade:** dados de fornecedores pessoa física tratados conforme a LGPD.



## 12. Critérios de aceite do MVP

- É possível cadastrar um produto com nome, quantidade, preço, estante e prateleira, e encontrá-lo pela busca.
- Não é possível salvar produto com nome duplicado, preço negativo ou sem localização.
- É possível vincular vários fornecedores a um produto e ver, na tela do fornecedor, todos os seus produtos.
- Fornecedor com produtos ativos vinculados não pode ser inativado sem tratar os vínculos.
- Produtos são inativados e podem ser reativados; nada se perde.
- Toda alteração mostra quem fez e quando.
- A planilha atual foi importada, e as divergências foram listadas.
- Operador não consegue alterar preços nem gerenciar usuários.
- O sistema funciona em computador e celular.
