# 📝 Regras de Commit para o Projeto

## 🎯 Objetivo
Padronizar mensagens de commit para facilitar a leitura, rastreabilidade e automações (como changelogs e deploys).

## 📌 Formato da Mensagem
Use o seguinte padrão:

<tipo>(escopo opcional): <descrição breve>


### Exemplos:
- `feat(login): adicionar autenticação via Google`
- `fix(api): corrigir erro de timeout na requisição`
- `docs(readme): atualizar instruções de instalação`

## 🔖 Tipos de Commit

| Tipo     | Descrição                                                                 |
|----------|---------------------------------------------------------------------------|
| feat     | Nova funcionalidade                                                       |
| fix      | Correção de bug                                                           |
| docs     | Alterações na documentação                                                |
| style    | Formatação, identação, espaços, etc. (sem alteração de código funcional) |
| refactor | Refatoração de código (sem mudança de funcionalidade)                    |
| test     | Adição ou modificação de testes                                           |
| chore    | Tarefas de manutenção (build, configs, dependências, etc.)               |

## 📚 Boas Práticas
- Escreva mensagens claras e objetivas.
- Use o imperativo: “adicionar”, “corrigir”, “remover”.
- Commits devem ser pequenos e focados em uma única tarefa.
- Evite mensagens genéricas como “update” ou “fix bug”.

## ✅ Checklist antes de commitar
- [ ] Código testado e funcionando
- [ ] Sem erros de lint
- [ ] Documentação atualizada (se necessário)
- [ ] Commit segue o padrão definido