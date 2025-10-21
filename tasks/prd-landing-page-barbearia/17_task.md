---
status: pending
parallelizable: false
blocked_by: ["10.0", "11.0", "14.0", "15.0"]
---

# Tarefa 17.0: Componente LandingPageForm

## Visão Geral
Criar o componente `LandingPageForm.tsx`, que será o formulário principal no painel de administração para personalizar a landing page. Este formulário integrará outros componentes menores, como o `LogoUploader` e o `ServiceManager`, e gerenciará a submissão dos dados para a API.

## Requisitos Funcionais (prd.md Seção 3)
- **Campos Editáveis**: O formulário deve permitir a edição dos seguintes campos:
  - Logo da Barbearia (usando `LogoUploader`).
  - Sobre a Barbearia (`textarea`, máx. 1000 caracteres).
  - Horário de Funcionamento (`textarea`, máx. 500 caracteres).
  - Número do WhatsApp (com validação de formato).
  - URL do Instagram (opcional, validação de URL).
  - URL do Facebook (opcional, validação de URL).
- **Gerenciamento de Serviços**: Integrar o componente `ServiceManager` para gerenciar os serviços exibidos.
- **Validação**: Implementar validação de formulário em tempo real para todos os campos.
- **Submissão**: Um botão "Salvar Alterações" para enviar os dados atualizados para o backend.
- **Feedback**: Exibir feedback de sucesso ou erro após a submissão.

## Detalhes de Implementação (techspec-frontend.md Seção 1.4)
- **Framework**: React com TypeScript.
- **Gerenciamento de Formulário**: Utilizar `react-hook-form` para gerenciamento de estado, validação e submissão.
- **Validação de Schema**: Usar `zod` com `zodResolver` para definir o schema de validação do formulário.
- **Componentes UI**: Utilizar componentes do `shadcn/ui` como `Form`, `FormField`, `Input`, `Textarea`, `Button`.
- **Hooks**: Utilizar o hook `useLandingPage` para buscar os dados iniciais e obter a função `updateConfig` para a submissão.
- **Integração de Componentes**: O `LandingPageForm` atuará como um contêiner, orquestrando os componentes `LogoUploader` e `ServiceManager`.

## Estrutura do Componente (`LandingPageForm.tsx`)
- **Schema de Validação (`zod`)**:
  ```typescript
  const formSchema = z.object({
    aboutText: z.string().max(1000).optional(),
    openingHours: z.string().max(500).optional(),
    instagramUrl: z.string().url().optional().or(z.literal('')),
    facebookUrl: z.string().url().optional().or(z.literal('')),
    whatsappNumber: z.string().regex(/^\+55\d{11}$/, 'Formato inválido'),
  });
  ```
- **Setup do Formulário**:
  - `const form = useForm({ resolver: zodResolver(formSchema), defaultValues: config });`
- **Estado de Serviços**: Manter o estado dos serviços separadamente, pois ele vem de um componente filho (`ServiceManager`).
  - `const [services, setServices] = useState(config?.services || []);`
- **Função de Submissão (`onSubmit`)**:
  - Coleta os dados do formulário e o estado atual dos serviços.
  - Chama a função `updateConfig` do hook `useLandingPage` com o payload combinado.
- **Renderização**:
  - Envolver o formulário com o provider do `react-hook-form`.
  - Renderizar o componente `LogoUploader`.
  - Renderizar cada `FormField` para os campos de texto (Sobre, Horário, etc.).
  - Renderizar o componente `ServiceManager`, passando `services` e `setServices`.
  - Botões de "Salvar" e "Cancelar".

## Critérios de Aceitação
- [ ] O formulário é pré-preenchido com os dados existentes da landing page ao carregar.
- [ ] A validação de campos (comprimento máximo, formato de URL, formato de telefone) funciona em tempo real.
- [ ] O componente `LogoUploader` está integrado e funcionando dentro do formulário.
- [ ] O componente `ServiceManager` está integrado, e as alterações feitas nele (ordem, visibilidade) são mantidas no estado do `LandingPageForm`.
- [ ] Ao clicar em "Salvar Alterações", os dados do formulário e do `ServiceManager` são enviados para a API através do hook `useLandingPage`.
- [ ] O botão "Salvar" fica desabilitado durante a submissão.
- [ ] Mensagens de sucesso ou erro são exibidas após a tentativa de salvar.