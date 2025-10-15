# Page snapshot

```yaml
- generic [ref=e2]:
  - generic [ref=e4]:
    - generic [ref=e5]:
      - heading "BarbApp Admin" [level=2] [ref=e6]
      - paragraph [ref=e7]: Faça login para gerenciar barbearias
    - generic [ref=e8]:
      - generic [ref=e9]:
        - text: Email
        - textbox "Email" [ref=e10]: admin@barbapp.com
      - generic [ref=e11]:
        - text: Senha
        - generic [ref=e12]:
          - textbox "Senha" [ref=e13]: "123456"
          - button "Mostrar senha" [ref=e14] [cursor=pointer]: 👁️
      - button "Entrar" [ref=e15] [cursor=pointer]
  - region "Notifications (F8)":
    - list [ref=e17]:
      - listitem [ref=e18]:
        - generic [ref=e19]:
          - generic [ref=e20]: Erro ao fazer login
          - generic [ref=e21]: Verifique suas credenciais e tente novamente.
        - button [ref=e22] [cursor=pointer]:
          - img [ref=e23]
  - region "Notifications (F8)":
    - list [ref=e28]:
      - listitem [ref=e29]:
        - generic [ref=e30]:
          - generic [ref=e31]: Erro ao fazer login
          - generic [ref=e32]: Verifique suas credenciais e tente novamente.
        - button [ref=e33] [cursor=pointer]:
          - img [ref=e34]
```