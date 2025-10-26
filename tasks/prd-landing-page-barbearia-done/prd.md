# PRD - Landing Page Personalizável para Barbearias

## Visão Geral

O módulo de Landing Page permite que cada barbearia tenha uma página pública e profissional para divulgar seus serviços, facilitar agendamentos e contato via WhatsApp. O Admin da Barbearia pode escolher entre 5 templates pré-definidos, personalizar informações (logo, textos, horários, redes sociais) e decidir quais serviços exibir. A landing page é criada automaticamente no cadastro da barbearia com configurações padrão, podendo ser customizada posteriormente.

## Objetivos

- **Objetivo Principal**: Fornecer uma página pública profissional para cada barbearia divulgar serviços e facilitar agendamentos
- **Métricas de Sucesso**:
  - 100% das barbearias têm landing page criada automaticamente no cadastro
  - Tempo de personalização da landing page < 5 minutos
  - Taxa de conversão landing page → agendamento > 20%
  - Taxa de cliques no botão WhatsApp > 15%
  - Carregamento da landing page < 2 segundos
- **Objetivos de Negócio**:
  - Profissionalizar presença online das barbearias
  - Aumentar taxa de agendamentos via canal digital
  - Facilitar divulgação em redes sociais (URL compartilhável)
  - Reduzir atrito entre descoberta e agendamento
  - Oferecer canal direto de contato (WhatsApp)

## Histórias de Usuário

### Persona: Admin da Barbearia
Proprietário ou gerente da barbearia que quer divulgar seus serviços online e facilitar agendamentos.

**Histórias Principais:**

- Como Admin, eu quero **ter uma landing page criada automaticamente ao cadastrar minha barbearia** para começar a divulgar imediatamente
- Como Admin, eu quero **escolher entre 5 templates diferentes** para ter uma identidade visual que combine com minha barbearia
- Como Admin, eu quero **visualizar preview dos templates antes de escolher** para tomar decisão informada
- Como Admin, eu quero **fazer upload do logo da minha barbearia** para personalizar a landing page
- Como Admin, eu quero **editar informações da barbearia** (endereço, horário, descrição, redes sociais) para manter dados atualizados
- Como Admin, eu quero **escolher quais serviços aparecem na landing page** para destacar serviços principais
- Como Admin, eu quero **ordenar serviços exibidos** para priorizar os mais populares
- Como Admin, eu quero **configurar número do WhatsApp** para receber contatos dos clientes
- Como Admin, eu quero **trocar de template quando quiser** para renovar visual da página
- Como Admin, eu quero **visualizar preview da landing page antes de publicar** para validar customizações

**Casos de Uso Secundários:**

- Como Admin, eu quero **acessar minha landing page rapidamente** para verificar como está
- Como Admin, eu quero **copiar URL da landing page facilmente** para compartilhar em redes sociais
- Como Admin, eu quero **ter atalho de login na landing page** para acessar painel admin rapidamente

### Persona: Cliente (Visitante)
Usuário final que descobre a barbearia e acessa a landing page.

**Histórias Principais:**

- Como Cliente, eu quero **ver informações completas da barbearia** (endereço, horário, sobre) para decidir se quero agendar
- Como Cliente, eu quero **visualizar todos os serviços disponíveis** com preços e duração para saber o que é oferecido
- Como Cliente, eu quero **selecionar múltiplos serviços e agendar** diretamente da landing page
- Como Cliente, eu quero **entrar em contato via WhatsApp** rapidamente para tirar dúvidas
- Como Cliente, eu quero **acessar a landing page de qualquer dispositivo** (mobile, tablet, desktop) com boa experiência

## Funcionalidades Principais

### 1. Criação Automática no Cadastro da Barbearia

**O que faz**: Sistema cria landing page automaticamente quando admin cadastra nova barbearia.

**Por que é importante**: Barbearia já começa com presença online sem precisar configurar tudo manualmente.

**Como funciona**:
- Admin completa cadastro da barbearia no painel admin
- Sistema cria landing page automaticamente com:
  - Template padrão (Template 1)
  - Dados básicos da barbearia (nome, endereço)
  - Todos os serviços cadastrados (exibidos por padrão)
  - WhatsApp do cadastro
- Landing page já está acessível via `/barbearia/CODIGO`

**Requisitos Funcionais:**

1.1. Ao criar barbearia, sistema deve criar registro de configuração da landing page automaticamente

1.2. Configuração padrão criada:
   - Template: "Template 1 - Clássico"
   - Logo: placeholder padrão do sistema
   - Nome: nome da barbearia cadastrada
   - Endereço: endereço do cadastro
   - Horário de funcionamento: "Segunda a Sábado: 09:00 - 19:00" (padrão editável)
   - Descrição: vazia (admin preenche depois)
   - WhatsApp: número do cadastro da barbearia
   - Redes sociais: vazias (admin preenche depois)
   - Serviços: todos os serviços ativos (exibir todos = true)
   - Status: Publicada (ativa desde o início)

1.3. Landing page é criada de forma assíncrona (não bloqueia cadastro)

1.4. Se criação falhar, sistema registra erro mas não impede cadastro da barbearia

1.5. Admin pode customizar landing page imediatamente após cadastro

### 2. Escolha e Troca de Templates

**O que faz**: Admin pode escolher entre 5 templates pré-definidos e trocar quando quiser.

**Por que é importante**: Cada barbearia pode ter identidade visual própria alinhada ao seu posicionamento.

**Como funciona**:
- Admin acessa seção "Landing Page" no painel admin
- Sistema exibe galeria com preview dos 5 templates
- Admin seleciona template desejado
- Sistema aplica template e mostra preview atualizado
- Admin pode trocar template quantas vezes quiser

**Requisitos Funcionais:**

2.1. Sistema deve oferecer exatamente 5 templates fixos

2.2. **Templates disponíveis** (sugestões de design):

**Template 1 - Clássico**
- **Tema**: Elegante e tradicional
- **Cores**: Preto, dourado, branco
- **Layout**: Header com logo centralizado, seção hero com imagem de fundo, grid de serviços 2 colunas, rodapé com redes sociais
- **Fontes**: Serif para títulos, Sans-serif para corpo
- **Ícones**: Tradicionais (tesoura, navalha, pente)

**Template 2 - Moderno**
- **Tema**: Limpo e minimalista
- **Cores**: Cinza escuro, azul elétrico, branco
- **Layout**: Header fixo, seção hero com call-to-action destacado, cards de serviços com sombras, seção "Sobre" com timeline
- **Fontes**: Sans-serif moderna (ex: Montserrat, Poppins)
- **Animações**: Scroll suave, hover nos cards

**Template 3 - Vintage**
- **Tema**: Retrô anos 50/60
- **Cores**: Marrom, creme, vermelho escuro
- **Layout**: Header com banner ilustrativo, seção hero com tipografia grande, lista de serviços com ilustrações vintage
- **Fontes**: Display vintage para títulos, Sans-serif para corpo
- **Elementos**: Texturas, bordas ornamentais

**Template 4 - Urbano**
- **Tema**: Street/Hip-hop
- **Cores**: Preto, vermelho vibrante, cinza
- **Layout**: Header com menu lateral, seção hero full-screen com vídeo/imagem, grid de serviços 3 colunas
- **Fontes**: Bold e impactantes (ex: Bebas Neue, Oswald)
- **Elementos**: Grafismos, linhas diagonais

**Template 5 - Premium**
- **Tema**: Luxuoso e sofisticado
- **Cores**: Preto, dourado metálico, cinza escuro
- **Layout**: Header transparente, seção hero com parallax, serviços em lista detalhada, seção de depoimentos
- **Fontes**: Serif clássica para títulos (ex: Playfair Display), Sans-serif elegante para corpo
- **Animações**: Transições suaves, parallax scroll

2.3. Cada template deve ter preview visual (screenshot ou componente ao vivo)

2.4. Admin pode visualizar preview do template aplicado aos seus dados antes de confirmar

2.5. Ao selecionar template, sistema deve:
   - Atualizar configuração da landing page
   - Manter todas as customizações (logo, textos, serviços)
   - Apenas trocar estrutura visual
   - Aplicar mudança imediatamente (landing page pública atualizada)

2.6. Template selecionado deve estar destacado na galeria (borda/ícone de check)

2.7. Admin pode trocar template ilimitadas vezes

2.8. Troca de template não afeta dados personalizados (logo, textos, serviços)

### 3. Personalização de Informações da Barbearia

**O que faz**: Admin pode editar todas as informações exibidas na landing page.

**Por que é importante**: Cada barbearia tem dados únicos que precisam ser comunicados aos clientes.

**Como funciona**:
- Admin acessa seção "Landing Page" > "Editar Informações"
- Sistema exibe formulário com campos editáveis
- Admin preenche/atualiza campos
- Admin salva alterações
- Sistema atualiza landing page imediatamente

**Requisitos Funcionais:**

3.1. **Campos Editáveis**:

**3.1.1. Logo da Barbearia**
- Upload de imagem (JPG, PNG, SVG)
- Tamanho recomendado: 300x300px (quadrado)
- Tamanho máximo do arquivo: 2MB
- Sistema redimensiona automaticamente para 300x300px (crop centralizado)
- Preview em tempo real após upload
- Botão "Remover logo" (volta para placeholder padrão)

**3.1.2. Nome da Barbearia**
- Campo texto (máx. 50 caracteres)
- Obrigatório
- Aparece no header e título da página

**3.1.3. Endereço Completo**
- Campo texto livre (máx. 200 caracteres)
- Exemplo: "Rua das Flores, 123 - Centro, São Paulo - SP"
- Aparece na seção "Localização" da landing page

**3.1.4. Horário de Funcionamento**
- Campo textarea (máx. 500 caracteres)
- Permite múltiplas linhas
- Exemplo:
  ```
  Segunda a Sexta: 09:00 - 19:00
  Sábado: 09:00 - 17:00
  Domingo: Fechado
  ```
- Aparece na seção "Informações" da landing page

**3.1.5. Sobre a Barbearia**
- Campo textarea (máx. 1000 caracteres)
- Permite múltiplas linhas
- Descrição livre sobre história, diferenciais, equipe
- Aparece na seção "Sobre" da landing page

**3.1.6. Número do WhatsApp**
- Campo formatado: (XX) XXXXX-XXXX
- Validação de formato brasileiro
- Valor padrão: número do cadastro da barbearia
- Obrigatório

**3.1.7. Instagram**
- Campo texto (URL ou @usuario)
- Opcional
- Exemplo: "@barbeariadojoao" ou "https://instagram.com/barbeariadojoao"
- Sistema normaliza para formato de URL

**3.1.8. Facebook**
- Campo texto (URL)
- Opcional
- Exemplo: "https://facebook.com/barbeariadojoao"

3.2. Formulário deve ter validações em tempo real (comprimento, formato)

3.3. Botão "Salvar Alterações" deve estar sempre visível

3.4. Ao salvar, sistema deve:
   - Validar todos os campos
   - Exibir mensagem de sucesso: "Informações atualizadas com sucesso"
   - Atualizar landing page pública imediatamente
   - Manter preview atualizado no painel admin

3.5. Botão "Cancelar" descarta alterações não salvas

3.6. Sistema deve exibir preview lateral em tempo real das alterações (antes de salvar)

3.7. **Upload de Logo**:
   - Drag & drop ou clique para selecionar arquivo
   - Validação de tipo de arquivo (apenas imagens)
   - Validação de tamanho (máx. 2MB)
   - Preview imediato após upload
   - Crop/redimensionamento automático para 300x300px
   - Armazenamento: CDN ou storage de arquivos

### 4. Gerenciamento de Serviços Exibidos

**O que faz**: Admin pode escolher quais serviços aparecem na landing page e em qual ordem.

**Por que é importante**: Permite destacar serviços principais e organizar prioridades.

**Como funciona**:
- Admin acessa seção "Landing Page" > "Serviços"
- Sistema exibe lista de todos os serviços cadastrados
- Admin pode marcar/desmarcar serviços para exibição
- Admin pode reordenar serviços (drag & drop ou setas)
- Admin salva configuração
- Landing page exibe apenas serviços selecionados na ordem definida

**Requisitos Funcionais:**

4.1. Sistema deve listar todos os serviços ativos da barbearia

4.2. Cada serviço na lista deve exibir:
   - Nome do serviço
   - Duração
   - Preço
   - Checkbox "Exibir na landing page" (marcado por padrão)
   - Handles para reordenação (ícone de arrastar)

4.3. Admin pode desmarcar serviços para ocultar da landing page

4.4. Serviços desmarcados continuam disponíveis no sistema de agendamento

4.5. **Reordenação de Serviços**:
   - Drag & drop de serviços na lista
   - Ou botões "↑ Mover para cima" / "↓ Mover para baixo"
   - Ordem definida reflete imediatamente no preview
   - Numeração visual (1, 2, 3...) para indicar ordem

4.6. Botão "Selecionar todos" marca todos os serviços

4.7. Botão "Desmarcar todos" desmarca todos os serviços

4.8. Sistema deve validar que ao menos 1 serviço está selecionado

4.9. Se nenhum serviço selecionado, exibir erro: "Selecione ao menos um serviço para exibir"

4.10. Ao salvar, sistema deve:
   - Atualizar configuração da landing page
   - Aplicar mudanças imediatamente
   - Exibir mensagem: "Serviços atualizados com sucesso"

4.11. Landing page pública exibe serviços na ordem definida

4.12. Serviços ocultos não aparecem na landing page mas continuam disponíveis no agendamento direto

### 5. Landing Page Pública (Visualização do Cliente)

**O que faz**: Página pública acessível via URL que exibe informações da barbearia e permite agendamento.

**Por que é importante**: É o canal principal de divulgação e conversão da barbearia.

**Como funciona**:
- Cliente acessa `/barbearia/CODIGO`
- Sistema carrega configuração da landing page
- Sistema renderiza template selecionado com dados personalizados
- Cliente visualiza serviços, informações e pode agendar ou contatar via WhatsApp

**Requisitos Funcionais:**

5.1. **URL de Acesso**: `/barbearia/{codigo}` (mesmo padrão do fluxo de agendamento)

5.2. **Estrutura da Landing Page** (comum a todos os templates):

**5.2.1. Header**
- Logo da barbearia (clicável, volta para topo)
- Nome da barbearia
- Navegação: Home, Serviços, Sobre, Contato
- Botão destacado: "Agendar Agora" (CTA primário)
- Link discreto: "Área Admin" (ícone de cadeado ou texto pequeno)

**5.2.2. Seção Hero (Banner Principal)**
- Imagem de fundo (fornecida pelo template, não customizável no MVP)
- Título: Nome da barbearia
- Subtítulo: Slogan ou descrição curta
- Botão primário: "Agendar Serviço"
- Botão secundário: WhatsApp (ícone verde)

**5.2.3. Seção "Nossos Serviços"**
- Título: "Serviços"
- Grid de cards de serviços (responsivo: 3 cols desktop, 2 cols tablet, 1 col mobile)
- Cada card contém:
  - Nome do serviço
  - Descrição (se houver)
  - Duração (ícone relógio + tempo)
  - Preço (destaque visual)
  - Checkbox para seleção
- Botão flutuante fixo (ao selecionar serviços): "Agendar {X} serviço(s)" (mostra total de preços)
- Serviços aparecem na ordem definida pelo admin

**5.2.4. Seção "Sobre Nós"**
- Título: "Sobre a {Nome da Barbearia}"
- Texto da descrição (campo "Sobre a Barbearia")
- Se vazio, não exibe a seção

**5.2.5. Seção "Informações"**
- Título: "Onde Estamos"
- Ícone de localização + Endereço completo
- Ícone de relógio + Horário de funcionamento
- Ícone de WhatsApp + Número (clicável)

**5.2.6. Seção "Redes Sociais"**
- Título: "Nos Acompanhe"
- Ícones clicáveis de Instagram e Facebook (se preenchidos)
- Se nenhuma rede social preenchida, não exibe a seção

**5.2.7. Footer**
- Nome da barbearia
- Texto: "© 2025 - Todos os direitos reservados"
- Link: "Área Admin"

5.3. **Comportamento de Seleção de Serviços**:
   - Cliente pode marcar/desmarcar checkbox de múltiplos serviços
   - Sistema calcula total de duração e preço em tempo real
   - Botão flutuante aparece quando ao menos 1 serviço selecionado
   - Botão exibe: "Agendar {X} serviço(s) - R$ {total}"

5.4. **Botão "Agendar Agora"**:
   - Ao clicar SEM selecionar serviços: redireciona para `/barbearia/{codigo}/agendar` (fluxo completo do PRD)
   - Ao clicar COM serviços selecionados: redireciona para `/barbearia/{codigo}/agendar?servicos={ids}` (pré-seleciona serviços)

5.5. **Botão WhatsApp**:
   - Ao clicar, abre WhatsApp Web (desktop) ou app (mobile)
   - URL: `https://wa.me/5511999999999?text=Olá!%20Gostaria%20de%20fazer%20um%20agendamento`
   - Número formatado: 55 (país) + DDD + número (sem formatação)
   - Mensagem padrão URL-encoded: "Olá! Gostaria de fazer um agendamento"

5.6. **Link "Área Admin"**:
   - Redireciona para `/admin/login`
   - Discreto (não compete com CTAs principais)
   - Tooltip: "Acesso restrito para administradores"

5.7. **Responsividade Obrigatória**:
   - Mobile-first
   - Breakpoints: mobile (<768px), tablet (768-1024px), desktop (>1024px)
   - Menu hamburguer em mobile
   - Cards empilhados em mobile
   - Botão WhatsApp flutuante em mobile (canto inferior direito)

5.8. **Performance**:
   - Carregamento < 2 segundos
   - Lazy loading de imagens
   - Otimização de assets (minificação CSS/JS)

5.9. **SEO Básico** (não obrigatório MVP mas recomendado):
   - Title tag: "{Nome da Barbearia} - Agendamento Online"
   - Meta description: primeiros 150 chars da descrição

### 6. Preview da Landing Page no Painel Admin

**O que faz**: Admin pode visualizar preview em tempo real da landing page durante customização.

**Por que é importante**: Admin valida mudanças antes de publicar/aplicar.

**Como funciona**:
- Admin está editando informações ou escolhendo template
- Painel lateral ou modal exibe preview da landing page
- Preview atualiza em tempo real conforme admin edita
- Admin pode alternar entre visualização mobile/desktop

**Requisitos Funcionais:**

6.1. Preview deve estar disponível em 2 contextos:
   - **Durante edição**: Painel lateral/split view mostrando preview ao lado do formulário
   - **Galeria de templates**: Modal com preview do template aplicado aos dados da barbearia

6.2. Preview deve renderizar exatamente como landing page pública (mesmos componentes)

6.3. **Controles do Preview**:
   - Botões: 📱 Mobile | 💻 Desktop
   - Alternar entre visualizações responsivas
   - Zoom (opcional)

6.4. Preview atualiza em tempo real (debounce de 500ms após parar de digitar)

6.5. Preview não é clicável (apenas visual)

6.6. Botão "Abrir em nova aba" abre landing page pública real (para testar links)

6.7. Se dados obrigatórios estão faltando (ex: logo), preview exibe placeholder

### 7. Acesso Rápido e Compartilhamento de URL

**O que faz**: Admin pode copiar URL da landing page facilmente e acessar rapidamente.

**Por que é importante**: Facilita divulgação em redes sociais e compartilhamento com clientes.

**Como funciona**:
- Admin vê URL da landing page destacada no painel
- Admin clica em botão "Copiar URL"
- Sistema copia URL para clipboard
- Admin pode abrir landing page em nova aba

**Requisitos Funcionais:**

7.1. Seção "Landing Page" deve exibir box destacado com:
   - Label: "URL da sua Landing Page"
   - URL completa: `https://app.barbapp.com/barbearia/{codigo}`
   - Botão: "📋 Copiar URL"
   - Botão: "🔗 Abrir Landing Page"

7.2. Ao clicar "Copiar URL":
   - Sistema copia URL para clipboard
   - Exibe tooltip: "✓ URL copiada!"
   - Tooltip desaparece após 2 segundos

7.3. Ao clicar "Abrir Landing Page":
   - Abre URL em nova aba
   - Landing page carrega com dados atualizados

7.4. URL deve ser fácil de ler e compartilhar (sem caracteres especiais)

## Fluxos Principais

### Fluxo 1: Criação Automática no Cadastro (Admin)
1. Admin acessa painel admin pela primeira vez
2. Admin completa cadastro da barbearia (nome, endereço, WhatsApp, serviços)
3. Admin clica "Concluir Cadastro"
4. Sistema:
   - Cria barbearia no banco
   - **Cria configuração de landing page automaticamente**:
     - Template: Template 1
     - Logo: placeholder
     - Dados básicos preenchidos do cadastro
     - Todos os serviços marcados para exibição
     - Status: Publicada
5. Sistema exibe mensagem: "✓ Barbearia cadastrada com sucesso! Sua landing page já está no ar."
6. Sistema oferece botão: "Personalizar Landing Page" ou "Ir para Dashboard"

### Fluxo 2: Escolha e Troca de Template (Admin)
1. Admin acessa "Landing Page" > "Templates"
2. Sistema exibe galeria com 5 templates:
   ```
   [Preview Template 1 - Clássico] ✓ (selecionado)
   [Preview Template 2 - Moderno]
   [Preview Template 3 - Vintage]
   [Preview Template 4 - Urbano]
   [Preview Template 5 - Premium]
   ```
3. Admin clica em "Template 4 - Urbano"
4. Sistema abre modal com preview do Template 4 aplicado aos dados da barbearia
5. Modal exibe:
   - Preview em tempo real (mobile/desktop)
   - Botão "Aplicar Template"
   - Botão "Cancelar"
6. Admin clica "Aplicar Template"
7. Sistema:
   - Atualiza configuração (template_id = 4)
   - Exibe mensagem: "✓ Template atualizado com sucesso!"
   - Fecha modal
   - Galeria marca Template 4 como selecionado
8. Landing page pública já exibe novo template

### Fluxo 3: Personalização de Informações (Admin)
1. Admin acessa "Landing Page" > "Editar Informações"
2. Sistema exibe formulário em split view (50% form, 50% preview)
3. Admin faz upload de logo:
   - Clica em área de upload
   - Seleciona arquivo "logo.png" (400KB)
   - Sistema valida, redimensiona e exibe preview
4. Admin edita "Sobre a Barbearia":
   ```
   Desde 2015 oferecendo os melhores cortes da região.
   Equipe especializada e ambiente moderno.
   ```
5. Admin edita horário:
   ```
   Segunda a Sexta: 09:00 - 19:00
   Sábado: 09:00 - 17:00
   ```
6. Admin preenche Instagram: "@barbeariaurban"
7. Preview lateral atualiza em tempo real a cada campo editado
8. Admin clica "Salvar Alterações"
9. Sistema:
   - Valida campos
   - Atualiza banco de dados
   - Exibe mensagem: "✓ Informações atualizadas!"
10. Landing page pública atualizada imediatamente

### Fluxo 4: Gerenciamento de Serviços (Admin)
1. Admin acessa "Landing Page" > "Serviços"
2. Sistema exibe lista de serviços:
   ```
   1. [✓] Corte Social (30min) - R$ 35,00 [↑ ↓]
   2. [✓] Barba (20min) - R$ 25,00 [↑ ↓]
   3. [✓] Corte + Barba (50min) - R$ 55,00 [↑ ↓]
   4. [✓] Corte Infantil (20min) - R$ 25,00 [↑ ↓]
   ```
3. Admin desmarca "Corte Infantil" (não quer exibir na landing page)
4. Admin clica "↑" em "Corte + Barba" para mover para posição 1
5. Lista atualiza:
   ```
   1. [✓] Corte + Barba (50min) - R$ 55,00 [↑ ↓]
   2. [✓] Corte Social (30min) - R$ 35,00 [↑ ↓]
   3. [✓] Barba (20min) - R$ 25,00 [↑ ↓]
   4. [ ] Corte Infantil (20min) - R$ 25,00 [↑ ↓]
   ```
6. Preview lateral atualiza exibindo apenas 3 serviços na nova ordem
7. Admin clica "Salvar"
8. Sistema atualiza configuração
9. Landing page pública exibe 3 serviços na ordem definida

### Fluxo 5: Cliente Acessa Landing Page e Agenda
1. Cliente recebe link: `https://app.barbapp.com/barbearia/XYZ123AB`
2. Cliente acessa link no smartphone
3. Sistema carrega landing page:
   - Template 4 (Urbano)
   - Logo da barbearia
   - Header com botão "Agendar Agora" destacado
4. Cliente rola para seção "Serviços"
5. Cliente vê 3 serviços:
   - [✓] Corte + Barba - 50min - R$ 55,00
   - [ ] Corte Social - 30min - R$ 35,00
   - [ ] Barba - 20min - R$ 25,00
6. Cliente marca checkbox em "Corte + Barba"
7. Botão flutuante aparece: "Agendar 1 serviço - R$ 55,00"
8. Cliente clica no botão flutuante
9. Sistema redireciona para: `/barbearia/XYZ123AB/agendar?servicos=3`
10. Fluxo de agendamento do PRD inicia com "Corte + Barba" pré-selecionado

### Fluxo 6: Cliente Entra em Contato via WhatsApp
1. Cliente está na landing page (mobile)
2. Cliente vê botão flutuante do WhatsApp (verde, canto inferior direito)
3. Cliente clica no botão
4. Sistema abre WhatsApp app com:
   - Número: (11) 98765-4321
   - Mensagem pré-preenchida: "Olá! Gostaria de fazer um agendamento"
5. Cliente pode editar mensagem e enviar

## Estrutura de Dados (Backend)

### Tabela: `landing_page_configs`

```sql
CREATE TABLE landing_page_configs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    barbershop_id UUID NOT NULL REFERENCES barbershops(id) ON DELETE CASCADE,
    template_id INT NOT NULL DEFAULT 1, -- 1 a 5
    logo_url VARCHAR(500), -- URL do logo no storage
    about_text TEXT, -- Sobre a barbearia (max 1000 chars)
    opening_hours TEXT, -- Horário de funcionamento
    instagram_url VARCHAR(255),
    facebook_url VARCHAR(255),
    whatsapp_number VARCHAR(20) NOT NULL, -- Formato: +5511999999999
    is_published BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(barbershop_id)
);
```

### Tabela: `landing_page_services` (relação N:N)

```sql
CREATE TABLE landing_page_services (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    landing_page_config_id UUID NOT NULL REFERENCES landing_page_configs(id) ON DELETE CASCADE,
    service_id UUID NOT NULL REFERENCES services(id) ON DELETE CASCADE,
    display_order INT NOT NULL, -- Ordem de exibição (1, 2, 3...)
    is_visible BOOLEAN DEFAULT TRUE, -- Exibir na landing page?
    created_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(landing_page_config_id, service_id)
);

CREATE INDEX idx_landing_page_services_config ON landing_page_services(landing_page_config_id);
CREATE INDEX idx_landing_page_services_order ON landing_page_services(landing_page_config_id, display_order);
```

## Endpoints da API (Backend)

### 1. Criar Landing Page Automaticamente
**POST** `/api/admin/landing-pages`

**Autenticação**: Admin da Barbearia

**Request Body**:
```json
{
  "barbershopId": "uuid",
  "templateId": 1,
  "logoUrl": null,
  "aboutText": "",
  "openingHours": "Segunda a Sábado: 09:00 - 19:00",
  "instagramUrl": null,
  "facebookUrl": null,
  "whatsappNumber": "+5511999999999",
  "services": [
    {"serviceId": "uuid1", "displayOrder": 1, "isVisible": true},
    {"serviceId": "uuid2", "displayOrder": 2, "isVisible": true}
  ]
}
```

**Response** (201):
```json
{
  "id": "uuid",
  "barbershopId": "uuid",
  "templateId": 1,
  "isPublished": true,
  "publicUrl": "https://app.barbapp.com/barbearia/XYZ123AB",
  "createdAt": "2025-10-20T10:00:00Z"
}
```

### 2. Obter Configuração da Landing Page (Admin)
**GET** `/api/admin/landing-pages/{barbershopId}`

**Autenticação**: Admin da Barbearia

**Response** (200):
```json
{
  "id": "uuid",
  "barbershopId": "uuid",
  "templateId": 2,
  "logoUrl": "https://cdn.barbapp.com/logos/xyz.png",
  "aboutText": "Desde 2015...",
  "openingHours": "Segunda a Sexta: 09:00 - 19:00",
  "instagramUrl": "https://instagram.com/barbearia",
  "facebookUrl": null,
  "whatsappNumber": "+5511999999999",
  "isPublished": true,
  "services": [
    {
      "serviceId": "uuid1",
      "serviceName": "Corte + Barba",
      "duration": 50,
      "price": 55.00,
      "displayOrder": 1,
      "isVisible": true
    },
    {
      "serviceId": "uuid2",
      "serviceName": "Corte Social",
      "duration": 30,
      "price": 35.00,
      "displayOrder": 2,
      "isVisible": true
    }
  ],
  "updatedAt": "2025-10-20T10:00:00Z"
}
```

### 3. Atualizar Landing Page (Admin)
**PUT** `/api/admin/landing-pages/{barbershopId}`

**Autenticação**: Admin da Barbearia

**Request Body**:
```json
{
  "templateId": 4,
  "aboutText": "Texto atualizado...",
  "openingHours": "Segunda a Sexta: 10:00 - 20:00",
  "instagramUrl": "https://instagram.com/barbeariaurban",
  "facebookUrl": "https://facebook.com/barbeariaurban",
  "whatsappNumber": "+5511987654321",
  "services": [
    {"serviceId": "uuid1", "displayOrder": 1, "isVisible": true},
    {"serviceId": "uuid2", "displayOrder": 2, "isVisible": false}
  ]
}
```

**Response** (200):
```json
{
  "id": "uuid",
  "barbershopId": "uuid",
  "message": "Landing page atualizada com sucesso",
  "updatedAt": "2025-10-20T11:00:00Z"
}
```

### 4. Upload de Logo
**POST** `/api/admin/landing-pages/{barbershopId}/logo`

**Autenticação**: Admin da Barbearia

**Request**: Multipart/form-data
- Field: `logo` (arquivo de imagem)

**Response** (200):
```json
{
  "logoUrl": "https://cdn.barbapp.com/logos/xyz123.png",
  "message": "Logo atualizado com sucesso"
}
```

**Validações**:
- Tipo: JPG, PNG, SVG
- Tamanho máx: 2MB
- Redimensionamento automático: 300x300px (crop centralizado)

### 5. Obter Landing Page Pública (Sem Autenticação)
**GET** `/api/public/barbershops/{code}/landing-page`

**Autenticação**: Nenhuma (público)

**Response** (200):
```json
{
  "barbershop": {
    "id": "uuid",
    "name": "Barbearia Urban",
    "code": "XYZ123AB",
    "address": "Rua das Flores, 123 - Centro, São Paulo - SP"
  },
  "landingPage": {
    "templateId": 4,
    "logoUrl": "https://cdn.barbapp.com/logos/xyz.png",
    "aboutText": "Desde 2015 oferecendo os melhores cortes...",
    "openingHours": "Segunda a Sexta: 09:00 - 19:00\nSábado: 09:00 - 17:00",
    "instagramUrl": "https://instagram.com/barbeariaurban",
    "facebookUrl": "https://facebook.com/barbeariaurban",
    "whatsappNumber": "+5511987654321",
    "services": [
      {
        "id": "uuid1",
        "name": "Corte + Barba",
        "description": "Corte social + Barba completa",
        "duration": 50,
        "price": 55.00
      },
      {
        "id": "uuid2",
        "name": "Corte Social",
        "description": null,
        "duration": 30,
        "price": 35.00
      }
    ]
  }
}
```

**Erro** (404):
```json
{
  "error": "Landing page não encontrada"
}
```

## Requisitos de UI/UX

### Painel Admin

- **Navegação Clara**: Seção "Landing Page" no menu principal com ícone 🎨
- **Split View**: Formulário (50%) + Preview (50%) durante edição
- **Galeria Visual**: Templates exibidos como cards grandes com screenshots
- **Drag & Drop**: Reordenação de serviços intuitiva
- **Feedback Imediato**: Preview atualiza em tempo real (debounce 500ms)
- **CTAs Claros**:
  - "Salvar Alterações" (primário, verde)
  - "Cancelar" (secundário, cinza)
  - "Aplicar Template" (primário, azul)
- **Estados de Loading**: Indicadores durante upload e salvamento
- **Validações em Tempo Real**: Erros exibidos abaixo dos campos
- **Botão Flutuante "Copiar URL"**: Sempre visível no topo da seção
- **Tooltips**: Ajudam admin a entender cada campo

### Landing Page Pública

- **Mobile-First Obrigatório**: Interface otimizada para smartphones
- **Performance**: Carregamento rápido (< 2s), lazy loading de imagens
- **Botões Grandes**: CTAs com área mínima 44x44px (touch-friendly)
- **Hierarquia Visual Clara**:
  - CTA primário: "Agendar Agora" (destaque máximo)
  - CTA secundário: WhatsApp (verde, reconhecível)
  - Links discretos: "Área Admin" (não compete)
- **Feedback Visual**: Checkbox de seleção de serviços com transição suave
- **Botão Flutuante**: Aparece ao selecionar serviços, sempre visível ao rolar
- **Responsividade Perfeita**: Layout adapta em mobile, tablet, desktop
- **Acessibilidade Básica**:
  - Contraste adequado (WCAG AA)
  - Textos legíveis (min 16px mobile)
  - Labels descritivos
  - Estados de foco visíveis

### Templates

Cada template deve ter:
- **Identidade Visual Única**: Cores, fontes, layout distintivos
- **Estrutura Consistente**: Mesmas seções (header, hero, serviços, sobre, contato, footer)
- **Componentes Reutilizáveis**: Logo, serviços, botões sempre nas mesmas áreas lógicas
- **Preview de Alta Qualidade**: Screenshots ou renders realistas para galeria
- **Performance Otimizada**: CSS/JS minificados, imagens otimizadas

## Restrições Técnicas

### Stack Tecnológica
- **Frontend Admin**: React + Vite + TypeScript (painel admin existente)
- **Frontend Público**: React + Vite + TypeScript (landing page pública)
- **Backend**: .NET 8 (API REST)
- **Banco de Dados**: PostgreSQL
- **Storage**: AWS S3, Azure Blob Storage ou Cloudinary (upload de logos)
- **CDN**: Cloudflare ou similar (servir assets estáticos)

### Arquitetura Multi-tenant
- **Isolamento**: Cada barbearia tem configuração de landing page independente
- **Queries Filtradas**: Sempre filtrar por `barbershop_id`
- **URL Pública**: Código da barbearia é identificador único

### Upload de Arquivos
- **Storage Externo**: Logos armazenados em bucket S3/Azure/Cloudinary
- **Processamento**: Redimensionamento/crop server-side (biblioteca Sharp ou ImageSharp)
- **Segurança**: Validação de tipo MIME, limite de tamanho, sanitização de nomes

### Performance
- **Carregamento Landing Page**: < 2 segundos
- **Preview em Tempo Real**: Debounce 500ms
- **Lazy Loading**: Imagens carregam sob demanda
- **Cache**: Landing page pública cacheada (TTL 5 minutos)

### Segurança
- **Autenticação**: Apenas admin da barbearia pode editar sua landing page
- **Validação de Propriedade**: Admin só acessa landing page de sua barbearia
- **Upload Seguro**: Validação de tipo de arquivo, antivírus (opcional), limite de tamanho
- **Rate Limiting**: Limitar requisições de upload (ex: 10 uploads/hora)
- **CORS**: Landing page pública permite CORS para compartilhamento

### SEO e Compartilhamento
- **Meta Tags Básicas**: title, description (pré-renderizadas no server se possível)
- **Open Graph** (Futuro): og:title, og:description, og:image para preview em redes sociais
- **URLs Amigáveis**: `/barbearia/CODIGO` (clean, sem query strings)

## Não-Objetivos (Fora de Escopo MVP)

### Explicitamente Excluído

- **Customização Total de Cores**: Admin não pode escolher cores livremente (apenas templates fixos)
- **Editor Visual de Layout**: Admin não pode arrastar/soltar componentes
- **Múltiplas Fotos**: Apenas logo (não galeria de fotos)
- **Vídeos**: Não suporta upload de vídeos no MVP
- **Depoimentos**: Seção de reviews/depoimentos fica para versão futura
- **Mapa Integrado**: Não embeds Google Maps no MVP
- **Chat Online**: Widget de chat fica para versão futura
- **A/B Testing**: Testar variações de landing page fica para versão futura
- **Analytics Integrado**: Dashboard de métricas (visitas, conversões) fica para versão futura
- **Formulário de Contato**: Apenas WhatsApp no MVP (formulário fica para futuro)
- **Multi-idioma**: Apenas português no MVP
- **Dark Mode**: Apenas tema claro no MVP
- **Agendamento Inline**: Cliente agenda sem sair da landing page (modal) - versão futura
- **Promoções na Landing Page**: Banner de promoções/descontos fica para futuro
- **Blog Integrado**: Seção de blog/notícias fica para versão futura

### Considerações Futuras (Pós-MVP)

- Galeria de fotos (múltiplas imagens)
- Depoimentos de clientes
- Google Maps integrado
- Analytics dashboard (visitas, conversões, origem de tráfego)
- Customização avançada de cores
- Mais templates (total de 10+)
- Open Graph completo para redes sociais
- Formulário de contato customizável
- Chat online integrado
- Sistema de promoções/banners
- Agendamento em modal (sem sair da landing page)

## Questões em Aberto

### Questões de Negócio

1. **Limite de Templates**: 5 templates são suficientes para MVP? Ou melhor começar com 3 e adicionar gradualmente?
   - **Sugestão**: Começar com 3 (Clássico, Moderno, Premium) e adicionar 2 depois

2. **Rascunho vs Publicado**: Landing page deve ter modo "Rascunho" (não visível publicamente) ou sempre publicada desde criação?
   - **Decisão MVP**: Sempre publicada (campo `is_published` existe mas sempre `true`)

3. **Placeholder do Logo**: Se admin não fizer upload de logo, qual placeholder usar?
   - **Sugestão**: Logo genérico "BarbApp" ou ícone de tesoura

4. **Obrigatoriedade de Campos**:
   - "Sobre a Barbearia" é obrigatório ou opcional?
   - Redes sociais obrigatórias ou opcionais?
   - **Sugestão MVP**: Apenas Nome, WhatsApp obrigatórios. Resto opcional.

5. **Preços**: Serviços sempre exibem preço ou admin pode ocultar?
   - **Decisão Necessária**: Sempre exibir ou ter toggle "Mostrar preços"?

6. **Edição de Serviços**: Admin edita serviços na seção "Landing Page" ou apenas marca/desmarca (edição de nome/preço fica na seção "Serviços" do admin)?
   - **Decisão MVP**: Apenas marca/desmarca e reordena. Edição de dados fica em "Gestão de Serviços"

### Questões Técnicas

7. **Storage de Logos**:
   - Usar AWS S3, Azure Blob ou Cloudinary?
   - **Decisão Necessária**: Qual provider usar?

8. **Processamento de Imagem**:
   - Server-side (ImageSharp .NET) ou client-side (Canvas API)?
   - **Sugestão**: Server-side para garantir qualidade e segurança

9. **Cache da Landing Page**:
   - Cachear resposta da API? Por quanto tempo?
   - **Sugestão**: Cache de 5 minutos no CDN/API Gateway

10. **Pre-rendering**:
    - Landing page deve ser SSR (Server-Side Rendering) para SEO?
    - Ou CSR (Client-Side Rendering) é suficiente no MVP?
    - **Decisão Necessária**: Avaliar impacto no SEO

11. **Validação de URLs de Redes Sociais**:
    - Validar formato de URL do Instagram/Facebook?
    - Normalizar @usuario para URL completa?
    - **Sugestão**: Validação básica (regex) + normalização automática

12. **Múltiplos Idiomas de Template**:
    - Textos fixos dos templates (ex: "Nossos Serviços") em português?
    - Preparar i18n desde o início ou adicionar depois?
    - **Decisão MVP**: Apenas português (i18n fica para futuro)

### Questões de UX

13. **Preview em Tempo Real**:
    - Split view (lado a lado) ou modal full-screen?
    - **Sugestão**: Split view em desktop, modal em mobile

14. **Feedback de Salvamento**:
    - Toast notification ou banner no topo?
    - **Sugestão**: Toast (não intrusivo)

15. **Galeria de Templates**:
    - Exibir 5 templates em grid 3x2 ou carrossel?
    - **Sugestão**: Grid 3x2 em desktop, carrossel em mobile

16. **Ordem de Campos no Formulário**:
    - Qual sequência lógica? Logo → Textos → Redes Sociais → Serviços?
    - **Sugestão**: Logo e Nome → Sobre → Horário/Endereço → Redes Sociais

17. **Botão WhatsApp na Landing Page**:
    - Flutuante fixo (sempre visível) ou apenas na seção "Contato"?
    - **Sugestão**: Flutuante em mobile, ambos em desktop

18. **Seleção de Múltiplos Serviços**:
    - Limite máximo de serviços selecionáveis de uma vez?
    - **Decisão MVP**: Sem limite (cliente pode selecionar quantos quiser)

19. **Empty State**:
    - Se barbearia não tem serviços cadastrados, o que exibir na landing page?
    - **Sugestão**: Mensagem "Em breve novos serviços!" + botão de contato

20. **URL Amigável (Slug)**:
    - Além de `/barbearia/CODIGO`, oferecer `/barbearia/nome-da-barbearia`?
    - **Decisão MVP**: Apenas código (slug fica para futuro)

---

## Prioridades de Implementação

### Fase 1 (MVP Essencial)
1. ✅ Criação automática de landing page no cadastro
2. ✅ Escolha entre 3 templates (Clássico, Moderno, Premium)
3. ✅ Upload e edição de logo
4. ✅ Edição de informações (sobre, horário, redes sociais)
5. ✅ Gerenciamento de serviços (marcar/desmarcar, reordenar)
6. ✅ Landing page pública responsiva
7. ✅ Botão WhatsApp funcional
8. ✅ Botão "Agendar Agora" integrado ao fluxo do PRD

### Fase 2 (Melhorias)
1. Adicionar 2 templates extras (Vintage, Urbano)
2. Preview em tempo real no painel admin
3. Analytics básico (visitas, cliques em "Agendar", cliques em WhatsApp)
4. Galeria de fotos (múltiplas imagens)
5. Open Graph completo para redes sociais

### Fase 3 (Avançado)
1. Customização avançada de cores
2. Editor visual de layout
3. Sistema de depoimentos
4. Google Maps integrado
5. Formulário de contato
6. Chat online

---

**Data de Criação**: 2025-10-20  
**Versão**: 1.0  
**Status**: Aguardando Validação e Decisões Técnicas
