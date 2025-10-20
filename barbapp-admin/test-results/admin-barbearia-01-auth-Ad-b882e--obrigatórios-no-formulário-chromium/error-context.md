# Page snapshot

```yaml
- generic [ref=e2]:
  - generic [ref=e4]:
    - generic [ref=e5]:
      - heading "🪒 BarbApp" [level=2] [ref=e6]
      - generic [ref=e7]:
        - heading "Barbearia da Neide" [level=3] [ref=e8]
        - paragraph [ref=e9]: Faça login para gerenciar sua barbearia
    - generic [ref=e10]:
      - generic [ref=e11]:
        - text: Email
        - textbox "Email" [active] [ref=e12]
        - paragraph [ref=e13]: Email inválido
      - generic [ref=e14]:
        - text: Senha
        - textbox "Senha" [ref=e15]
        - paragraph [ref=e16]: Senha deve ter no mínimo 6 caracteres
      - button "Entrar" [ref=e17] [cursor=pointer]
    - paragraph [ref=e19]: Esqueceu a senha? Contate o suporte da sua barbearia
  - region "Notifications (F8)":
    - list
  - region "Notifications (F8)":
    - list
  - generic [ref=e20]:
    - img [ref=e22]
    - button "Open Tanstack query devtools" [ref=e70] [cursor=pointer]:
      - img [ref=e71]
```