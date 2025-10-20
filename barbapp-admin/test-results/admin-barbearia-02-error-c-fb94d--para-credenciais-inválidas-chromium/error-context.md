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
        - textbox "Email" [ref=e12]: wrong@test.com
      - generic [ref=e13]:
        - text: Senha
        - textbox "Senha" [ref=e14]: WrongPassword123!
      - button "Entrar" [ref=e15] [cursor=pointer]
    - paragraph [ref=e17]: Esqueceu a senha? Contate o suporte da sua barbearia
  - region "Notifications (F8)":
    - list [ref=e19]:
      - listitem [ref=e20]:
        - generic [ref=e21]:
          - generic [ref=e22]: Credenciais inválidas
          - generic [ref=e23]: Email ou senha incorretos. Verifique e tente novamente.
        - button [ref=e24] [cursor=pointer]:
          - img [ref=e25]
  - region "Notifications (F8)":
    - list [ref=e30]:
      - listitem [ref=e31]:
        - generic [ref=e32]:
          - generic [ref=e33]: Credenciais inválidas
          - generic [ref=e34]: Email ou senha incorretos. Verifique e tente novamente.
        - button [ref=e35] [cursor=pointer]:
          - img [ref=e36]
  - generic [ref=e40]:
    - img [ref=e42]
    - button "Open Tanstack query devtools" [ref=e90] [cursor=pointer]:
      - img [ref=e91]
```