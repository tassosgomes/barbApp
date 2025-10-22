/**
 * LandingPageEditor Page
 * 
 * Página principal para gerenciamento da landing page no painel de administração.
 * Organiza as funcionalidades em abas (Tabs) para uma experiência de usuário clara e intuitiva.
 * 
 * Funcionalidades:
 * - Aba "Editar Informações": Formulário de edição + Preview lateral
 * - Aba "Escolher Template": Galeria de templates disponíveis
 * - Aba "Preview": Preview em tela cheia
 * - Ações rápidas: Copiar URL e Abrir Landing Page
 * 
 * @see PRD: tasks/prd-landing-page-barbearia/prd.md - Seções 2, 3, 6, 7
 * @see TechSpec: tasks/prd-landing-page-barbearia/techspec-frontend.md - Seção 1.5
 * @see Task: tasks/prd-landing-page-barbearia/18_task.md
 * @version 1.0
 * @date 2025-10-22
 */

import React, { useState, useEffect } from 'react';
import { Copy, ExternalLink, Loader2, AlertCircle } from 'lucide-react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { useLandingPage } from '@/features/landing-page/hooks/useLandingPage';
import { LandingPageForm } from '@/features/landing-page/components/LandingPageForm';
import { TemplateGallery } from '@/features/landing-page/components/TemplateGallery';
import { PreviewPanel } from '@/features/landing-page/components/PreviewPanel';
import { useToast } from '@/hooks/use-toast';
import { useBarbearia } from '@/contexts/BarbeariaContext';

// ============================================================================
// Component
// ============================================================================

export const LandingPageEditor: React.FC = () => {
  // ============================================================================
  // Hooks & State
  // ============================================================================

  const { toast } = useToast();
  const { barbearia } = useBarbearia();
  const barbershopId = barbearia?.barbeariaId || '';

  const { config, updateConfig, isLoading, error } = useLandingPage(barbershopId);

  // Estado local para o template selecionado
  const [selectedTemplate, setSelectedTemplate] = useState<number>(1);

  // Sincronizar template selecionado com a config
  useEffect(() => {
    if (config?.templateId) {
      setSelectedTemplate(config.templateId);
    }
  }, [config?.templateId]);

  // ============================================================================
  // Computed Values
  // ============================================================================

  // URL pública da landing page
  const landingPageUrl = barbearia?.codigo
    ? `${window.location.origin}/barbearia/${barbearia.codigo}`
    : '';

  // ============================================================================
  // Handlers
  // ============================================================================

  /**
   * Copia a URL da landing page para a área de transferência
   */
  const handleCopyUrl = async () => {
    try {
      await navigator.clipboard.writeText(landingPageUrl);
      toast({
        title: 'URL Copiada!',
        description: 'A URL da sua landing page foi copiada para a área de transferência.',
      });
    } catch {
      toast({
        title: 'Erro ao copiar',
        description: 'Não foi possível copiar a URL. Tente novamente.',
        variant: 'destructive',
      });
    }
  };

  /**
   * Abre a landing page em uma nova aba
   */
  const handleOpenLandingPage = () => {
    if (landingPageUrl) {
      window.open(landingPageUrl, '_blank', 'noopener,noreferrer');
    }
  };

  /**
   * Handler para mudança de template
   * Atualiza o estado local e envia para o backend
   */
  const handleTemplateChange = (templateId: number) => {
    setSelectedTemplate(templateId);
    updateConfig({ templateId });
  };

  // ============================================================================
  // Loading State
  // ============================================================================

  if (isLoading) {
    return (
      <div className="container mx-auto py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <Loader2 className="h-12 w-12 animate-spin text-primary mx-auto mb-4" />
            <p className="text-muted-foreground">Carregando configuração da landing page...</p>
          </div>
        </div>
      </div>
    );
  }

  // ============================================================================
  // Error State
  // ============================================================================

  if (error || !config) {
    const errorMessage = error && typeof error === 'object' && error !== null && 'message' in error 
      ? String((error as { message: string }).message)
      : 'Erro desconhecido';

    return (
      <div className="container mx-auto py-8">
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Não foi possível carregar a configuração da landing page.
            <span className="block mt-2 text-sm">
              {errorMessage}
            </span>
          </AlertDescription>
        </Alert>
      </div>
    );
  }

  // ============================================================================
  // Render
  // ============================================================================

  return (
    <div className="container mx-auto py-6 px-4 sm:px-6 lg:px-8 space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Landing Page</h1>
          <p className="text-muted-foreground mt-1">
            Personalize a página pública da sua barbearia
          </p>
        </div>

        {/* Ações Rápidas */}
        <div className="flex flex-wrap gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={handleCopyUrl}
            disabled={!landingPageUrl}
            className="gap-2"
          >
            <Copy size={16} />
            <span className="hidden sm:inline">Copiar URL</span>
          </Button>
          <Button
            size="sm"
            onClick={handleOpenLandingPage}
            disabled={!landingPageUrl}
            className="gap-2"
          >
            <ExternalLink size={16} />
            <span className="hidden sm:inline">Abrir Landing Page</span>
          </Button>
        </div>
      </div>

      {/* URL Box */}
      {landingPageUrl && (
        <Card>
          <CardContent className="p-4">
            <div className="space-y-2">
              <label className="text-sm font-medium text-muted-foreground">
                URL da sua Landing Page:
              </label>
              <div className="flex items-center gap-2">
                <code className="flex-1 text-sm bg-muted px-3 py-2 rounded border overflow-x-auto">
                  {landingPageUrl}
                </code>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={handleCopyUrl}
                  title="Copiar URL"
                >
                  <Copy size={16} />
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Tabs Navigation */}
      <Tabs defaultValue="edit" className="w-full">
        <TabsList className="grid w-full grid-cols-3 lg:w-auto">
          <TabsTrigger value="edit">Editar Informações</TabsTrigger>
          <TabsTrigger value="template">Escolher Template</TabsTrigger>
          <TabsTrigger value="preview">Preview</TabsTrigger>
        </TabsList>

        {/* Tab: Editar Informações */}
        <TabsContent value="edit" className="mt-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Formulário de Edição */}
            <div className="space-y-6">
              <Card>
                <CardContent className="p-6">
                  <LandingPageForm barbershopId={barbershopId} />
                </CardContent>
              </Card>
            </div>

            {/* Preview Lateral (apenas em desktop) */}
            <div className="hidden lg:block">
              <div className="sticky top-6">
                <PreviewPanel config={config} device="desktop" />
              </div>
            </div>
          </div>
        </TabsContent>

        {/* Tab: Escolher Template */}
        <TabsContent value="template" className="mt-6">
          <Card>
            <CardContent className="p-6">
              <div className="space-y-4">
                <div>
                  <h2 className="text-xl font-semibold">Escolha um Template</h2>
                  <p className="text-sm text-muted-foreground mt-1">
                    Selecione o template que melhor combina com a identidade da sua barbearia.
                    As alterações são aplicadas imediatamente.
                  </p>
                </div>
                <TemplateGallery
                  selectedTemplateId={selectedTemplate}
                  onSelectTemplate={handleTemplateChange}
                />
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Tab: Preview */}
        <TabsContent value="preview" className="mt-6">
          <PreviewPanel config={config} fullScreen />
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default LandingPageEditor;
