# Tech Spec - Landing Page Barbearia (Frontend)

## Visão Geral

Este documento especifica a implementação do frontend para o sistema de Landing Pages personalizáveis. São dois aplicativos distintos:
1. **Painel Admin** (barbapp-admin): Interface para admin gerenciar landing page
2. **Landing Page Pública** (barbapp-public): Página pública acessível por clientes

## Arquitetura Frontend

### Estrutura de Projetos

```
barbApp/
├── barbapp-admin/          # Painel Admin (React + TypeScript + Vite)
│   └── src/
│       └── features/
│           └── landing-page/
│               ├── components/
│               │   ├── TemplateGallery.tsx
│               │   ├── TemplatePreview.tsx
│               │   ├── LandingPageForm.tsx
│               │   ├── LogoUploader.tsx
│               │   ├── ServiceManager.tsx
│               │   └── PreviewPanel.tsx
│               ├── hooks/
│               │   ├── useLandingPage.ts
│               │   ├── useTemplates.ts
│               │   └── useLogoUpload.ts
│               ├── types/
│               │   └── landing-page.types.ts
│               └── pages/
│                   ├── LandingPageEditor.tsx
│                   └── TemplateSelector.tsx
│
└── barbapp-public/         # Landing Page Pública (NOVA)
    ├── src/
    │   ├── templates/
    │   │   ├── Template1Classic.tsx
    │   │   ├── Template2Modern.tsx
    │   │   ├── Template3Vintage.tsx
    │   │   ├── Template4Urban.tsx
    │   │   └── Template5Premium.tsx
    │   ├── components/
    │   │   ├── Header.tsx
    │   │   ├── Hero.tsx
    │   │   ├── ServiceCard.tsx
    │   │   ├── WhatsAppButton.tsx
    │   │   └── Footer.tsx
    │   ├── hooks/
    │   │   ├── useLandingPageData.ts
    │   │   └── useServiceSelection.ts
    │   ├── types/
    │   │   └── landing-page.types.ts
    │   └── pages/
    │       └── LandingPage.tsx
    ├── vite.config.ts
    ├── tailwind.config.js
    └── package.json
```

## 1. Painel Admin - Gestão de Landing Page

### 1.1. Types e Interfaces

**`types/landing-page.types.ts`**

```typescript
export interface LandingPageConfig {
  id: string;
  barbershopId: string;
  templateId: number; // 1-5
  logoUrl?: string;
  aboutText?: string;
  openingHours?: string;
  instagramUrl?: string;
  facebookUrl?: string;
  whatsappNumber: string;
  isPublished: boolean;
  services: LandingPageService[];
  updatedAt: string;
}

export interface LandingPageService {
  serviceId: string;
  serviceName: string;
  description?: string;
  duration: number; // minutos
  price: number;
  displayOrder: number;
  isVisible: boolean;
}

export interface Template {
  id: number;
  name: string;
  theme: string;
  description: string;
  previewImage: string;
  colors: {
    primary: string;
    secondary: string;
    accent: string;
  };
}

export interface UpdateLandingPageRequest {
  templateId?: number;
  aboutText?: string;
  openingHours?: string;
  instagramUrl?: string;
  facebookUrl?: string;
  whatsappNumber?: string;
  services?: Array<{
    serviceId: string;
    displayOrder: number;
    isVisible: boolean;
  }>;
}
```

### 1.2. Templates Mockados

**`constants/templates.ts`**

```typescript
import { Template } from '@/types/landing-page.types';

export const TEMPLATES: Template[] = [
  {
    id: 1,
    name: 'Clássico',
    theme: 'classic',
    description: 'Elegante e tradicional, ideal para barbearias com conceito premium.',
    previewImage: '/templates/classic-preview.png',
    colors: {
      primary: '#1A1A1A', // Preto
      secondary: '#D4AF37', // Dourado
      accent: '#FFFFFF', // Branco
    },
  },
  {
    id: 2,
    name: 'Moderno',
    theme: 'modern',
    description: 'Limpo e minimalista, perfeito para um visual contemporâneo.',
    previewImage: '/templates/modern-preview.png',
    colors: {
      primary: '#2C3E50', // Cinza escuro
      secondary: '#3498DB', // Azul elétrico
      accent: '#ECF0F1', // Branco/Cinza claro
    },
  },
  {
    id: 3,
    name: 'Vintage',
    theme: 'vintage',
    description: 'Estilo retrô anos 50/60, para barbearias com conceito clássico.',
    previewImage: '/templates/vintage-preview.png',
    colors: {
      primary: '#5D4037', // Marrom
      secondary: '#B71C1C', // Vermelho escuro
      accent: '#F5E6D3', // Creme
    },
  },
  {
    id: 4,
    name: 'Urbano',
    theme: 'urban',
    description: 'Visual street/hip-hop, ideal para barbearias jovens e descoladas.',
    previewImage: '/templates/urban-preview.png',
    colors: {
      primary: '#000000', // Preto
      secondary: '#E74C3C', // Vermelho vibrante
      accent: '#95A5A6', // Cinza
    },
  },
  {
    id: 5,
    name: 'Premium',
    theme: 'premium',
    description: 'Luxuoso e sofisticado, para barbearias de alto padrão.',
    previewImage: '/templates/premium-preview.png',
    colors: {
      primary: '#1C1C1C', // Preto
      secondary: '#C9A961', // Dourado metálico
      accent: '#2E2E2E', // Cinza escuro
    },
  },
];
```

### 1.3. Hooks Customizados

**`hooks/useLandingPage.ts`**

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { landingPageApi } from '@/services/api/landing-page.api';
import { toast } from '@/components/ui/use-toast';

export const useLandingPage = (barbershopId: string) => {
  const queryClient = useQueryClient();

  // Buscar configuração da landing page
  const { data: config, isLoading, error } = useQuery({
    queryKey: ['landingPage', barbershopId],
    queryFn: () => landingPageApi.getConfig(barbershopId),
    staleTime: 5 * 60 * 1000, // 5 minutos
  });

  // Atualizar configuração
  const updateMutation = useMutation({
    mutationFn: (data: UpdateLandingPageRequest) =>
      landingPageApi.updateConfig(barbershopId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['landingPage', barbershopId] });
      toast({
        title: 'Sucesso!',
        description: 'Landing page atualizada com sucesso.',
        variant: 'success',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Erro',
        description: error.message || 'Erro ao atualizar landing page.',
        variant: 'destructive',
      });
    },
  });

  return {
    config,
    isLoading,
    error,
    updateConfig: updateMutation.mutate,
    isUpdating: updateMutation.isPending,
  };
};
```

**`hooks/useLogoUpload.ts`**

```typescript
import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { landingPageApi } from '@/services/api/landing-page.api';
import { toast } from '@/components/ui/use-toast';

const MAX_FILE_SIZE = 2 * 1024 * 1024; // 2MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/svg+xml'];

export const useLogoUpload = (barbershopId: string) => {
  const [preview, setPreview] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const validateFile = (file: File): string | null => {
    if (!ALLOWED_TYPES.includes(file.type)) {
      return 'Formato inválido. Use JPG, PNG ou SVG.';
    }
    if (file.size > MAX_FILE_SIZE) {
      return 'Arquivo muito grande. Tamanho máximo: 2MB.';
    }
    return null;
  };

  const uploadMutation = useMutation({
    mutationFn: (file: File) => landingPageApi.uploadLogo(barbershopId, file),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['landingPage', barbershopId] });
      toast({
        title: 'Logo atualizado!',
        description: 'Seu logo foi enviado com sucesso.',
        variant: 'success',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Erro no upload',
        description: error.message || 'Erro ao enviar logo.',
        variant: 'destructive',
      });
    },
  });

  const handleFileSelect = (file: File) => {
    const error = validateFile(file);
    if (error) {
      toast({
        title: 'Arquivo inválido',
        description: error,
        variant: 'destructive',
      });
      return;
    }

    // Preview local
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreview(reader.result as string);
    };
    reader.readAsDataURL(file);

    // Upload
    uploadMutation.mutate(file);
  };

  return {
    preview,
    isUploading: uploadMutation.isPending,
    uploadLogo: handleFileSelect,
  };
};
```

### 1.4. Componentes Principais

**`components/TemplateGallery.tsx`**

```typescript
import React from 'react';
import { TEMPLATES } from '@/constants/templates';
import { Card, CardContent } from '@/components/ui/card';
import { Check } from 'lucide-react';

interface TemplateGalleryProps {
  selectedTemplateId: number;
  onSelectTemplate: (templateId: number) => void;
}

export const TemplateGallery: React.FC<TemplateGalleryProps> = ({
  selectedTemplateId,
  onSelectTemplate,
}) => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {TEMPLATES.map((template) => (
        <Card
          key={template.id}
          className={`cursor-pointer transition-all hover:shadow-lg ${
            selectedTemplateId === template.id
              ? 'ring-2 ring-primary shadow-lg'
              : ''
          }`}
          onClick={() => onSelectTemplate(template.id)}
        >
          <CardContent className="p-4">
            <div className="relative">
              <img
                src={template.previewImage}
                alt={template.name}
                className="w-full h-48 object-cover rounded-md mb-4"
              />
              {selectedTemplateId === template.id && (
                <div className="absolute top-2 right-2 bg-primary text-white rounded-full p-1">
                  <Check size={16} />
                </div>
              )}
            </div>
            <h3 className="text-lg font-semibold mb-2">{template.name}</h3>
            <p className="text-sm text-muted-foreground">{template.description}</p>
            <div className="flex gap-2 mt-4">
              {Object.values(template.colors).map((color, idx) => (
                <div
                  key={idx}
                  className="w-8 h-8 rounded-full border-2 border-gray-200"
                  style={{ backgroundColor: color }}
                />
              ))}
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
};
```

**`components/LogoUploader.tsx`**

```typescript
import React, { useRef } from 'react';
import { Upload, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useLogoUpload } from '@/hooks/useLogoUpload';

interface LogoUploaderProps {
  barbershopId: string;
  currentLogoUrl?: string;
}

export const LogoUploader: React.FC<LogoUploaderProps> = ({
  barbershopId,
  currentLogoUrl,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const { preview, isUploading, uploadLogo } = useLogoUpload(barbershopId);

  const displayUrl = preview || currentLogoUrl;

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      uploadLogo(file);
    }
  };

  const handleRemove = () => {
    // TODO: Implementar remoção de logo (volta para placeholder)
  };

  return (
    <div className="space-y-4">
      <label className="block text-sm font-medium">Logo da Barbearia</label>
      <div className="flex items-center gap-4">
        <div className="relative w-32 h-32 border-2 border-dashed border-gray-300 rounded-lg overflow-hidden">
          {displayUrl ? (
            <>
              <img
                src={displayUrl}
                alt="Logo"
                className="w-full h-full object-cover"
              />
              <button
                onClick={handleRemove}
                className="absolute top-1 right-1 bg-red-500 text-white rounded-full p-1 hover:bg-red-600"
              >
                <X size={14} />
              </button>
            </>
          ) : (
            <div className="flex items-center justify-center h-full text-gray-400">
              <Upload size={32} />
            </div>
          )}
        </div>
        <div className="flex-1">
          <input
            ref={fileInputRef}
            type="file"
            accept="image/jpeg,image/png,image/svg+xml"
            onChange={handleFileChange}
            className="hidden"
          />
          <Button
            onClick={() => fileInputRef.current?.click()}
            disabled={isUploading}
            variant="outline"
          >
            {isUploading ? 'Enviando...' : 'Fazer Upload'}
          </Button>
          <p className="text-xs text-muted-foreground mt-2">
            JPG, PNG ou SVG. Máx. 2MB. Tamanho recomendado: 300x300px
          </p>
        </div>
      </div>
    </div>
  );
};
```

**`components/ServiceManager.tsx`**

```typescript
import React, { useState } from 'react';
import { DragDropContext, Draggable, Droppable } from '@hello-pangea/dnd';
import { GripVertical, Eye, EyeOff } from 'lucide-react';
import { Checkbox } from '@/components/ui/checkbox';
import { Button } from '@/components/ui/button';
import { LandingPageService } from '@/types/landing-page.types';

interface ServiceManagerProps {
  services: LandingPageService[];
  onChange: (services: LandingPageService[]) => void;
}

export const ServiceManager: React.FC<ServiceManagerProps> = ({
  services,
  onChange,
}) => {
  const [localServices, setLocalServices] = useState(services);

  const handleDragEnd = (result: any) => {
    if (!result.destination) return;

    const items = Array.from(localServices);
    const [reorderedItem] = items.splice(result.source.index, 1);
    items.splice(result.destination.index, 0, reorderedItem);

    // Atualizar displayOrder
    const updated = items.map((item, index) => ({
      ...item,
      displayOrder: index + 1,
    }));

    setLocalServices(updated);
    onChange(updated);
  };

  const toggleVisibility = (serviceId: string) => {
    const updated = localServices.map((s) =>
      s.serviceId === serviceId ? { ...s, isVisible: !s.isVisible } : s
    );
    setLocalServices(updated);
    onChange(updated);
  };

  const selectAll = () => {
    const updated = localServices.map((s) => ({ ...s, isVisible: true }));
    setLocalServices(updated);
    onChange(updated);
  };

  const deselectAll = () => {
    const updated = localServices.map((s) => ({ ...s, isVisible: false }));
    setLocalServices(updated);
    onChange(updated);
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <label className="text-sm font-medium">Serviços Exibidos</label>
        <div className="flex gap-2">
          <Button size="sm" variant="outline" onClick={selectAll}>
            Selecionar Todos
          </Button>
          <Button size="sm" variant="outline" onClick={deselectAll}>
            Desmarcar Todos
          </Button>
        </div>
      </div>

      <DragDropContext onDragEnd={handleDragEnd}>
        <Droppable droppableId="services">
          {(provided) => (
            <div
              {...provided.droppableProps}
              ref={provided.innerRef}
              className="space-y-2"
            >
              {localServices.map((service, index) => (
                <Draggable
                  key={service.serviceId}
                  draggableId={service.serviceId}
                  index={index}
                >
                  {(provided) => (
                    <div
                      ref={provided.innerRef}
                      {...provided.draggableProps}
                      className="flex items-center gap-3 p-3 border rounded-md bg-white"
                    >
                      <div
                        {...provided.dragHandleProps}
                        className="cursor-grab text-gray-400"
                      >
                        <GripVertical size={20} />
                      </div>
                      <Checkbox
                        checked={service.isVisible}
                        onCheckedChange={() => toggleVisibility(service.serviceId)}
                      />
                      <div className="flex-1">
                        <p className="font-medium">{service.serviceName}</p>
                        <p className="text-sm text-muted-foreground">
                          {service.duration}min • R$ {service.price.toFixed(2)}
                        </p>
                      </div>
                      {service.isVisible ? (
                        <Eye size={18} className="text-green-500" />
                      ) : (
                        <EyeOff size={18} className="text-gray-400" />
                      )}
                    </div>
                  )}
                </Draggable>
              ))}
              {provided.placeholder}
            </div>
          )}
        </Droppable>
      </DragDropContext>
    </div>
  );
};
```

**`components/LandingPageForm.tsx`**

```typescript
import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { LogoUploader } from './LogoUploader';
import { ServiceManager } from './ServiceManager';
import { useLandingPage } from '@/hooks/useLandingPage';

const formSchema = z.object({
  aboutText: z.string().max(1000).optional(),
  openingHours: z.string().max(500).optional(),
  instagramUrl: z.string().url().optional().or(z.literal('')),
  facebookUrl: z.string().url().optional().or(z.literal('')),
  whatsappNumber: z.string().regex(/^\+55\d{11}$/, 'Formato inválido'),
});

interface LandingPageFormProps {
  barbershopId: string;
}

export const LandingPageForm: React.FC<LandingPageFormProps> = ({
  barbershopId,
}) => {
  const { config, updateConfig, isUpdating } = useLandingPage(barbershopId);

  const form = useForm({
    resolver: zodResolver(formSchema),
    defaultValues: {
      aboutText: config?.aboutText || '',
      openingHours: config?.openingHours || '',
      instagramUrl: config?.instagramUrl || '',
      facebookUrl: config?.facebookUrl || '',
      whatsappNumber: config?.whatsappNumber || '',
    },
  });

  const [services, setServices] = useState(config?.services || []);

  const onSubmit = (data: z.infer<typeof formSchema>) => {
    updateConfig({
      ...data,
      services: services.map((s) => ({
        serviceId: s.serviceId,
        displayOrder: s.displayOrder,
        isVisible: s.isVisible,
      })),
    });
  };

  if (!config) return <div>Carregando...</div>;

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <LogoUploader
          barbershopId={barbershopId}
          currentLogoUrl={config.logoUrl}
        />

        <FormField
          control={form.control}
          name="aboutText"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Sobre a Barbearia</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Conte a história da sua barbearia..."
                  rows={4}
                  maxLength={1000}
                  {...field}
                />
              </FormControl>
              <FormMessage />
              <p className="text-xs text-muted-foreground">
                {field.value?.length || 0}/1000 caracteres
              </p>
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="openingHours"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Horário de Funcionamento</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Segunda a Sexta: 09:00 - 19:00"
                  rows={3}
                  maxLength={500}
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="whatsappNumber"
          render={({ field }) => (
            <FormItem>
              <FormLabel>WhatsApp</FormLabel>
              <FormControl>
                <Input placeholder="+5511999999999" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="instagramUrl"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Instagram</FormLabel>
                <FormControl>
                  <Input
                    placeholder="https://instagram.com/suabarbearia"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="facebookUrl"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Facebook</FormLabel>
                <FormControl>
                  <Input
                    placeholder="https://facebook.com/suabarbearia"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <ServiceManager services={services} onChange={setServices} />

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline">
            Cancelar
          </Button>
          <Button type="submit" disabled={isUpdating}>
            {isUpdating ? 'Salvando...' : 'Salvar Alterações'}
          </Button>
        </div>
      </form>
    </Form>
  );
};
```

### 1.5. Página Principal

**`pages/LandingPageEditor.tsx`**

```typescript
import React, { useState } from 'react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Button } from '@/components/ui/button';
import { Copy, ExternalLink } from 'lucide-react';
import { TemplateGallery } from '@/components/landing-page/TemplateGallery';
import { LandingPageForm } from '@/components/landing-page/LandingPageForm';
import { PreviewPanel } from '@/components/landing-page/PreviewPanel';
import { useLandingPage } from '@/hooks/useLandingPage';
import { toast } from '@/components/ui/use-toast';

export const LandingPageEditor: React.FC = () => {
  const barbershopId = 'xxx'; // TODO: Pegar do contexto/auth
  const { config, updateConfig } = useLandingPage(barbershopId);
  const [selectedTemplate, setSelectedTemplate] = useState(config?.templateId || 1);

  const landingPageUrl = `https://app.barbapp.com/barbearia/${config?.barbershop?.code}`;

  const copyUrl = () => {
    navigator.clipboard.writeText(landingPageUrl);
    toast({
      title: 'URL copiada!',
      description: 'A URL foi copiada para a área de transferência.',
    });
  };

  const handleTemplateChange = (templateId: number) => {
    setSelectedTemplate(templateId);
    updateConfig({ templateId });
  };

  return (
    <div className="container mx-auto py-8 space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Landing Page</h1>
          <p className="text-muted-foreground">
            Personalize a página pública da sua barbearia
          </p>
        </div>
        <div className="flex gap-3">
          <Button variant="outline" onClick={copyUrl}>
            <Copy size={16} className="mr-2" />
            Copiar URL
          </Button>
          <Button onClick={() => window.open(landingPageUrl, '_blank')}>
            <ExternalLink size={16} className="mr-2" />
            Abrir Landing Page
          </Button>
        </div>
      </div>

      <div className="bg-card border rounded-lg p-4">
        <p className="text-sm text-muted-foreground mb-2">
          URL da sua Landing Page:
        </p>
        <code className="text-sm bg-muted px-3 py-2 rounded block">
          {landingPageUrl}
        </code>
      </div>

      <Tabs defaultValue="edit" className="w-full">
        <TabsList>
          <TabsTrigger value="edit">Editar Informações</TabsTrigger>
          <TabsTrigger value="template">Escolher Template</TabsTrigger>
          <TabsTrigger value="preview">Preview</TabsTrigger>
        </TabsList>

        <TabsContent value="edit" className="mt-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <div className="space-y-6">
              <LandingPageForm barbershopId={barbershopId} />
            </div>
            <div className="hidden lg:block">
              <PreviewPanel config={config} />
            </div>
          </div>
        </TabsContent>

        <TabsContent value="template" className="mt-6">
          <TemplateGallery
            selectedTemplateId={selectedTemplate}
            onSelectTemplate={handleTemplateChange}
          />
        </TabsContent>

        <TabsContent value="preview" className="mt-6">
          <PreviewPanel config={config} fullScreen />
        </TabsContent>
      </Tabs>
    </div>
  );
};
```

## 2. Landing Page Pública

### 2.1. Setup do Projeto

```bash
cd barbApp
npm create vite@latest barbapp-public -- --template react-ts
cd barbapp-public
npm install
npm install react-router-dom @tanstack/react-query axios lucide-react
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

### 2.2. Types

**`types/landing-page.types.ts`** (mesmo do admin)

```typescript
export interface PublicLandingPage {
  barbershop: {
    id: string;
    name: string;
    code: string;
    address: string;
  };
  landingPage: {
    templateId: number;
    logoUrl?: string;
    aboutText?: string;
    openingHours?: string;
    instagramUrl?: string;
    facebookUrl?: string;
    whatsappNumber: string;
    services: PublicService[];
  };
}

export interface PublicService {
  id: string;
  name: string;
  description?: string;
  duration: number;
  price: number;
}
```

### 2.3. Hooks

**`hooks/useLandingPageData.ts`**

```typescript
import { useQuery } from '@tanstack/react-query';
import axios from 'axios';
import { PublicLandingPage } from '@/types/landing-page.types';

const API_URL = import.meta.env.VITE_API_URL;

export const useLandingPageData = (code: string) => {
  return useQuery<PublicLandingPage>({
    queryKey: ['publicLandingPage', code],
    queryFn: async () => {
      const { data } = await axios.get(
        `${API_URL}/public/barbershops/${code}/landing-page`
      );
      return data;
    },
    staleTime: 5 * 60 * 1000, // Cache 5 minutos
    retry: 1,
  });
};
```

**`hooks/useServiceSelection.ts`**

```typescript
import { useState, useMemo } from 'react';
import { PublicService } from '@/types/landing-page.types';

export const useServiceSelection = (services: PublicService[]) => {
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  const toggleService = (serviceId: string) => {
    setSelectedIds((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(serviceId)) {
        newSet.delete(serviceId);
      } else {
        newSet.add(serviceId);
      }
      return newSet;
    });
  };

  const selectedServices = useMemo(
    () => services.filter((s) => selectedIds.has(s.id)),
    [services, selectedIds]
  );

  const totalPrice = useMemo(
    () => selectedServices.reduce((sum, s) => sum + s.price, 0),
    [selectedServices]
  );

  const totalDuration = useMemo(
    () => selectedServices.reduce((sum, s) => sum + s.duration, 0),
    [selectedServices]
  );

  return {
    selectedIds,
    selectedServices,
    totalPrice,
    totalDuration,
    toggleService,
    hasSelection: selectedIds.size > 0,
  };
};
```

### 2.4. Componentes Compartilhados

**`components/ServiceCard.tsx`**

```typescript
import React from 'react';
import { Clock, DollarSign } from 'lucide-react';
import { PublicService } from '@/types/landing-page.types';

interface ServiceCardProps {
  service: PublicService;
  isSelected: boolean;
  onToggle: () => void;
}

export const ServiceCard: React.FC<ServiceCardProps> = ({
  service,
  isSelected,
  onToggle,
}) => {
  return (
    <div
      className={`border-2 rounded-lg p-4 cursor-pointer transition-all hover:shadow-md ${
        isSelected ? 'border-primary bg-primary/5' : 'border-gray-200'
      }`}
      onClick={onToggle}
    >
      <div className="flex items-start justify-between mb-3">
        <h3 className="text-lg font-semibold">{service.name}</h3>
        <input
          type="checkbox"
          checked={isSelected}
          onChange={onToggle}
          className="w-5 h-5 cursor-pointer"
        />
      </div>
      {service.description && (
        <p className="text-sm text-gray-600 mb-3">{service.description}</p>
      )}
      <div className="flex items-center gap-4 text-sm text-gray-500">
        <div className="flex items-center gap-1">
          <Clock size={16} />
          <span>{service.duration}min</span>
        </div>
        <div className="flex items-center gap-1 font-semibold text-primary">
          <DollarSign size={16} />
          <span>R$ {service.price.toFixed(2)}</span>
        </div>
      </div>
    </div>
  );
};
```

**`components/WhatsAppButton.tsx`**

```typescript
import React from 'react';
import { MessageCircle } from 'lucide-react';

interface WhatsAppButtonProps {
  phoneNumber: string;
  message?: string;
  className?: string;
  floating?: boolean;
}

export const WhatsAppButton: React.FC<WhatsAppButtonProps> = ({
  phoneNumber,
  message = 'Olá! Gostaria de fazer um agendamento',
  className = '',
  floating = false,
}) => {
  const formattedNumber = phoneNumber.replace(/\D/g, '');
  const encodedMessage = encodeURIComponent(message);
  const whatsappUrl = `https://wa.me/${formattedNumber}?text=${encodedMessage}`;

  const baseClasses = floating
    ? 'fixed bottom-6 right-6 z-50 bg-green-500 text-white rounded-full p-4 shadow-lg hover:bg-green-600 transition-all hover:scale-110'
    : 'inline-flex items-center gap-2 bg-green-500 text-white px-6 py-3 rounded-lg hover:bg-green-600 transition-colors';

  return (
    <a
      href={whatsappUrl}
      target="_blank"
      rel="noopener noreferrer"
      className={`${baseClasses} ${className}`}
      aria-label="Contato via WhatsApp"
    >
      <MessageCircle size={floating ? 28 : 20} />
      {!floating && <span>Falar no WhatsApp</span>}
    </a>
  );
};
```

### 2.5. Templates (Exemplo: Template 1 - Clássico)

**`templates/Template1Classic.tsx`**

```typescript
import React from 'react';
import { MapPin, Clock, Instagram, Facebook } from 'lucide-react';
import { PublicLandingPage } from '@/types/landing-page.types';
import { ServiceCard } from '@/components/ServiceCard';
import { WhatsAppButton } from '@/components/WhatsAppButton';
import { useServiceSelection } from '@/hooks/useServiceSelection';
import { useNavigate } from 'react-router-dom';

interface Template1ClassicProps {
  data: PublicLandingPage;
}

export const Template1Classic: React.FC<Template1ClassicProps> = ({ data }) => {
  const navigate = useNavigate();
  const { barbershop, landingPage } = data;
  const {
    selectedIds,
    totalPrice,
    hasSelection,
    toggleService,
  } = useServiceSelection(landingPage.services);

  const handleSchedule = () => {
    const serviceIds = Array.from(selectedIds).join(',');
    const url = hasSelection
      ? `/barbearia/${barbershop.code}/agendar?servicos=${serviceIds}`
      : `/barbearia/${barbershop.code}/agendar`;
    navigate(url);
  };

  return (
    <div className="min-h-screen bg-white font-serif">
      {/* Header */}
      <header className="bg-black text-white py-4 px-6 sticky top-0 z-40">
        <div className="container mx-auto flex justify-between items-center">
          <div className="flex items-center gap-4">
            {landingPage.logoUrl && (
              <img
                src={landingPage.logoUrl}
                alt={barbershop.name}
                className="w-12 h-12 rounded-full"
              />
            )}
            <h1 className="text-2xl font-bold">{barbershop.name}</h1>
          </div>
          <nav className="hidden md:flex gap-6">
            <a href="#servicos" className="hover:text-gold transition">
              Serviços
            </a>
            <a href="#sobre" className="hover:text-gold transition">
              Sobre
            </a>
            <a href="#contato" className="hover:text-gold transition">
              Contato
            </a>
          </nav>
          <button
            onClick={handleSchedule}
            className="bg-gold text-black px-6 py-2 rounded font-semibold hover:bg-gold/90 transition"
          >
            Agendar Agora
          </button>
        </div>
      </header>

      {/* Hero Section */}
      <section className="relative h-[60vh] bg-gradient-to-r from-black to-gray-800 flex items-center justify-center text-white">
        <div className="text-center z-10">
          <h2 className="text-5xl font-bold mb-4">{barbershop.name}</h2>
          <p className="text-xl mb-8">Tradição e Elegância desde sempre</p>
          <button
            onClick={handleSchedule}
            className="bg-gold text-black px-8 py-3 rounded-lg text-lg font-semibold hover:bg-gold/90 transition"
          >
            Agendar Serviço
          </button>
        </div>
      </section>

      {/* Services Section */}
      <section id="servicos" className="py-16 px-6">
        <div className="container mx-auto">
          <h2 className="text-4xl font-bold text-center mb-12">
            Nossos Serviços
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {landingPage.services.map((service) => (
              <ServiceCard
                key={service.id}
                service={service}
                isSelected={selectedIds.has(service.id)}
                onToggle={() => toggleService(service.id)}
              />
            ))}
          </div>
        </div>
      </section>

      {/* About Section */}
      {landingPage.aboutText && (
        <section id="sobre" className="py-16 px-6 bg-gray-50">
          <div className="container mx-auto max-w-3xl text-center">
            <h2 className="text-4xl font-bold mb-8">Sobre Nós</h2>
            <p className="text-lg text-gray-700 leading-relaxed whitespace-pre-line">
              {landingPage.aboutText}
            </p>
          </div>
        </section>
      )}

      {/* Contact Section */}
      <section id="contato" className="py-16 px-6">
        <div className="container mx-auto max-w-4xl">
          <h2 className="text-4xl font-bold text-center mb-12">
            Onde Estamos
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div className="flex items-start gap-3">
              <MapPin className="text-gold flex-shrink-0" size={24} />
              <div>
                <h3 className="font-semibold mb-1">Endereço</h3>
                <p className="text-gray-600">{barbershop.address}</p>
              </div>
            </div>
            {landingPage.openingHours && (
              <div className="flex items-start gap-3">
                <Clock className="text-gold flex-shrink-0" size={24} />
                <div>
                  <h3 className="font-semibold mb-1">Horário</h3>
                  <p className="text-gray-600 whitespace-pre-line">
                    {landingPage.openingHours}
                  </p>
                </div>
              </div>
            )}
          </div>

          {/* Social Media */}
          {(landingPage.instagramUrl || landingPage.facebookUrl) && (
            <div className="mt-12 text-center">
              <h3 className="text-2xl font-semibold mb-6">Nos Acompanhe</h3>
              <div className="flex justify-center gap-6">
                {landingPage.instagramUrl && (
                  <a
                    href={landingPage.instagramUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-gold hover:text-gold/80 transition"
                  >
                    <Instagram size={32} />
                  </a>
                )}
                {landingPage.facebookUrl && (
                  <a
                    href={landingPage.facebookUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-gold hover:text-gold/80 transition"
                  >
                    <Facebook size={32} />
                  </a>
                )}
              </div>
            </div>
          )}
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-black text-white py-8 px-6 text-center">
        <p>© 2025 {barbershop.name} - Todos os direitos reservados</p>
        <a
          href="/admin/login"
          className="text-sm text-gray-400 hover:text-white mt-2 inline-block"
        >
          Área Admin
        </a>
      </footer>

      {/* Floating WhatsApp Button */}
      <WhatsAppButton
        phoneNumber={landingPage.whatsappNumber}
        floating
      />

      {/* Floating Schedule Button (quando tem seleção) */}
      {hasSelection && (
        <button
          onClick={handleSchedule}
          className="fixed bottom-24 right-6 bg-primary text-white px-6 py-3 rounded-full shadow-lg hover:scale-105 transition-transform z-50"
        >
          Agendar {selectedIds.size} serviço{selectedIds.size > 1 ? 's' : ''} •
          R$ {totalPrice.toFixed(2)}
        </button>
      )}
    </div>
  );
};
```

### 2.6. Router e Página Principal

**`pages/LandingPage.tsx`**

```typescript
import React from 'react';
import { useParams } from 'react-router-dom';
import { useLandingPageData } from '@/hooks/useLandingPageData';
import { Template1Classic } from '@/templates/Template1Classic';
import { Template2Modern } from '@/templates/Template2Modern';
// Importar outros templates...

const TEMPLATE_COMPONENTS: Record<number, React.FC<any>> = {
  1: Template1Classic,
  2: Template2Modern,
  // 3, 4, 5...
};

export const LandingPage: React.FC = () => {
  const { code } = useParams<{ code: string }>();
  const { data, isLoading, error } = useLandingPageData(code!);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-gray-600">Carregando...</p>
        </div>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-red-500 mb-2">
            Landing page não encontrada
          </h1>
          <p className="text-gray-600">
            Verifique o código e tente novamente.
          </p>
        </div>
      </div>
    );
  }

  const TemplateComponent = TEMPLATE_COMPONENTS[data.landingPage.templateId] || Template1Classic;

  return <TemplateComponent data={data} />;
};
```

**`App.tsx`**

```typescript
import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LandingPage } from './pages/LandingPage';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/barbearia/:code" element={<LandingPage />} />
          {/* Outras rotas... */}
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
```

## 3. API Service (Integração Backend)

**`services/api/landing-page.api.ts`**

```typescript
import axios from 'axios';
import {
  LandingPageConfig,
  UpdateLandingPageRequest,
} from '@/types/landing-page.types';

const API_URL = import.meta.env.VITE_API_URL;

export const landingPageApi = {
  getConfig: async (barbershopId: string): Promise<LandingPageConfig> => {
    const { data } = await axios.get(
      `${API_URL}/admin/landing-pages/${barbershopId}`
    );
    return data;
  },

  updateConfig: async (
    barbershopId: string,
    payload: UpdateLandingPageRequest
  ): Promise<void> => {
    await axios.put(`${API_URL}/admin/landing-pages/${barbershopId}`, payload);
  },

  uploadLogo: async (barbershopId: string, file: File): Promise<string> => {
    const formData = new FormData();
    formData.append('logo', file);

    const { data } = await axios.post(
      `${API_URL}/admin/landing-pages/${barbershopId}/logo`,
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      }
    );

    return data.logoUrl;
  },
};
```

## 4. Tailwind CSS Config (Templates)

**`barbapp-public/tailwind.config.js`**

```javascript
/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        gold: '#D4AF37',
        'gold-dark': '#B8941E',
      },
      fontFamily: {
        serif: ['Playfair Display', 'serif'],
        sans: ['Inter', 'sans-serif'],
      },
    },
  },
  plugins: [],
};
```

## 5. Deploy e Build

### barbapp-admin
```json
// package.json - scripts
{
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview"
  }
}
```

### barbapp-public
```json
// package.json - scripts
{
  "scripts": {
    "dev": "vite --port 3001",
    "build": "tsc && vite build",
    "preview": "vite preview"
  }
}
```

## 6. Testes (Sugestão)

### Testes Unitários (Vitest)

```typescript
// __tests__/useServiceSelection.test.ts
import { renderHook, act } from '@testing-library/react';
import { useServiceSelection } from '@/hooks/useServiceSelection';

const mockServices = [
  { id: '1', name: 'Corte', price: 35, duration: 30 },
  { id: '2', name: 'Barba', price: 25, duration: 20 },
];

describe('useServiceSelection', () => {
  it('should toggle service selection', () => {
    const { result } = renderHook(() => useServiceSelection(mockServices));

    act(() => {
      result.current.toggleService('1');
    });

    expect(result.current.selectedIds.has('1')).toBe(true);
    expect(result.current.totalPrice).toBe(35);
    expect(result.current.totalDuration).toBe(30);
  });

  it('should calculate total correctly', () => {
    const { result } = renderHook(() => useServiceSelection(mockServices));

    act(() => {
      result.current.toggleService('1');
      result.current.toggleService('2');
    });

    expect(result.current.totalPrice).toBe(60);
    expect(result.current.totalDuration).toBe(50);
  });
});
```

### Testes E2E (Playwright)

```typescript
// e2e/landing-page.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Landing Page Pública', () => {
  test('should display barbershop information', async ({ page }) => {
    await page.goto('/barbearia/XYZ123AB');

    await expect(page.locator('h1')).toContainText('Barbearia do João');
    await expect(page.locator('#servicos')).toBeVisible();
  });

  test('should select services and navigate to booking', async ({ page }) => {
    await page.goto('/barbearia/XYZ123AB');

    await page.locator('[data-service-id="1"]').click();
    await page.locator('[data-service-id="2"]').click();

    await expect(page.locator('text=Agendar 2 serviços')).toBeVisible();

    await page.locator('text=Agendar 2 serviços').click();

    await expect(page).toHaveURL(/agendar\?servicos=1,2/);
  });

  test('should open WhatsApp on button click', async ({ page, context }) => {
    await page.goto('/barbearia/XYZ123AB');

    const [newPage] = await Promise.all([
      context.waitForEvent('page'),
      page.locator('[aria-label="Contato via WhatsApp"]').click(),
    ]);

    expect(newPage.url()).toContain('wa.me');
  });
});
```

## 7. Checklist de Implementação

### Fase 1 - Admin (Landing Page Editor)
- [ ] Setup de tipos e interfaces
- [ ] Hook `useLandingPage`
- [ ] Hook `useLogoUpload`
- [ ] Componente `TemplateGallery`
- [ ] Componente `LogoUploader`
- [ ] Componente `ServiceManager`
- [ ] Componente `LandingPageForm`
- [ ] Página `LandingPageEditor`
- [ ] Integração com API backend
- [ ] Testes unitários

### Fase 2 - Público (Landing Page)
- [ ] Setup projeto `barbapp-public`
- [ ] Hook `useLandingPageData`
- [ ] Hook `useServiceSelection`
- [ ] Componente `ServiceCard`
- [ ] Componente `WhatsAppButton`
- [ ] Template 1 - Clássico
- [ ] Template 2 - Moderno
- [ ] Template 3 - Vintage
- [ ] Template 4 - Urbano
- [ ] Template 5 - Premium
- [ ] Router e integração
- [ ] Testes E2E

### Fase 3 - Integração
- [ ] Criação automática de landing page no cadastro
- [ ] Preview em tempo real no admin
- [ ] Otimização de performance (lazy loading, cache)
- [ ] Deploy separado (barbapp-public como subdomínio)
- [ ] Analytics básico (opcional)

---

**Data de Criação**: 2025-10-20  
**Versão**: 1.0  
**Status**: Pronto para Implementação
