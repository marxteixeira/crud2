# Guia de validação: Recuperar cadastro de prateleira

## Pré-requisitos

- Dependências do monorepo instaladas.
- Aplicação web e API configuradas conforme o guia de desenvolvimento do projeto.
- Pessoa autenticada com acesso à gestão de localizações.
- Pelo menos uma estante ativa disponível para seleção.

## Testes automatizados

Execute o teste focal da página:

```powershell
npm test --workspace @crud2/web-tests -- LocationsPage.test.tsx
```

Resultado esperado: testes da página passam, incluindo cadastro bem-sucedido, atualização da lista, validação/erro e preservação dos controles existentes de manutenção das prateleiras.

Execute a suíte web:

```powershell
npm test
```

Resultado esperado: suíte web concluída sem regressões.

## Validação manual ponta a ponta

1. Inicie API e aplicação web conforme o guia de desenvolvimento existente e autentique-se.
2. Abra Localizações e selecione uma estante ativa.
3. Confirme que a seção dessa estante exibe o campo "Código da prateleira" e a ação "Cadastrar prateleira".
4. Envie um código válido e confirme que ele aparece uma vez na lista da estante correta sem recarregar a página manualmente.
5. Selecione outra estante e confirme que a prateleira recém-criada não aparece vinculada a ela.
6. Repita o envio com código vazio e com código já cadastrado na mesma estante; confirme mensagens claras e ausência de duplicatas.
7. Cadastre o mesmo código em outra estante ativa; confirme que a operação é permitida e que cada prateleira permanece associada à sua própria estante.
8. Confirme que as ações de editar, alterar situação e consultar produtos permanecem disponíveis nas prateleiras anteriores e na recém-criada.
