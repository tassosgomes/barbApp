/**
 * PreviewPanel Component
 * 
 * Componente que exibe uma pré-visualização em tempo real da landing page pública.
 * Permite alternar entre visualizações mobile e desktop e renderiza dinamicamente
 * o template selecionado com os dados da configuração.
 * 
 * @see PRD: Seção 6 - Preview da Landing Page no Painel Admin
 * @see techspec-frontend.md: Seção 1.5 - PreviewPanel
 * @version 1.0
 * @date 2025-10-21
 */

import React, { useState, useMemo } from 'react';
import { Monitor, Smartphone, ExternalLink } from 'lucide-react';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { LandingPageConfig, PreviewDevice } from '../types/landing-page.types';
import { Template1Classic } from './templates/Template1Classic';
import { Template2Modern } from './templates/Template2Modern';
import { Template3Vintage } from './templates/Template3Vintage';
import { Template4Urban } from './templates/Template4Urban';
import { Template5Premium } from './templates/Template5Premium';

interface PreviewPanelProps {
  /** Configuração da landing page para renderizar */
  config?: LandingPageConfig;
  /** Se deve renderizar em modo tela cheia */
  fullScreen?: boolean;
  /** Dispositivo inicial para preview */
  device?: PreviewDevice;
  /** Callback quando dispositivo muda */
  onDeviceChange?: (device: PreviewDevice) => void;
}

/**
 * Mapeamento de template IDs para componentes
 */
const TEMPLATE_COMPONENTS: Record<number, React.ComponentType<{ config: LandingPageConfig }>> = {
  1: Template1Classic,
  2: Template2Modern,
  3: Template3Vintage,
  4: Template4Urban,
  5: Template5Premium,
};

/**
 * Componente PreviewPanel
 */
export const PreviewPanel: React.FC<PreviewPanelProps> = ({
  config,
  fullScreen = false,
  device: initialDevice = 'desktop',
  onDeviceChange,
}) => {
  const [currentDevice, setCurrentDevice] = useState<PreviewDevice>(initialDevice);

  const handleDeviceChange = (device: PreviewDevice) => {
    setCurrentDevice(device);
    onDeviceChange?.(device);
  };

  const TemplateComponent = useMemo(() => {
    if (!config?.templateId) {
      return TEMPLATE_COMPONENTS[1];
    }
    return TEMPLATE_COMPONENTS[config.templateId] || TEMPLATE_COMPONENTS[1];
  }, [config?.templateId]);

  const previewContainerClasses = useMemo(() => {
    const baseClasses = 'mx-auto transition-all duration-300 bg-white shadow-lg rounded-lg overflow-hidden';
    
    switch (currentDevice) {
      case 'mobile':
        return cn(baseClasses, 'w-[375px] min-h-[667px]');
      case 'tablet':
        return cn(baseClasses, 'w-[768px] min-h-[1024px]');
      case 'desktop':
      default:
        return cn(baseClasses, 'w-full min-h-[600px]');
    }
  }, [currentDevice]);

  const wrapperClasses = useMemo(() => {
    if (fullScreen) {
      return 'w-full h-screen p-6 bg-gray-100 overflow-auto';
    }
    return 'w-full h-full';
  }, [fullScreen]);

  if (!config) {
    return (
      <Card className={wrapperClasses}>
        <CardContent className="flex items-center justify-center h-full min-h-[400px]">
          <div className="text-center">
            <p className="text-muted-foreground">
              Nenhuma configuração disponível para preview
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className={wrapperClasses}>
      <Card className="h-full">
        <CardHeader className="border-b">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold">Preview da Landing Page</h3>
            
            <div className="flex items-center gap-2">
              {/* Device Toggle Buttons */}
              <div className="flex items-center gap-1 border rounded-md p-1">
                <Button
                  variant={currentDevice === 'mobile' ? 'default' : 'ghost'}
                  size="sm"
                  onClick={() => handleDeviceChange('mobile')}
                  className="gap-2"
                  title="Visualização Mobile (375px)"
                >
                  <Smartphone size={16} />
                  <span className="hidden sm:inline">Mobile</span>
                </Button>
                <Button
                  variant={currentDevice === 'desktop' ? 'default' : 'ghost'}
                  size="sm"
                  onClick={() => handleDeviceChange('desktop')}
                  className="gap-2"
                  title="Visualização Desktop (100%)"
                >
                  <Monitor size={16} />
                  <span className="hidden sm:inline">Desktop</span>
                </Button>
              </div>

              {/* Open in New Tab Button */}
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  const url = `/barbearia/${config.barbershopId}`;
                  window.open(url, '_blank');
                }}
                className="gap-2"
                title="Abrir em nova aba"
              >
                <ExternalLink size={16} />
                <span className="hidden sm:inline">Abrir</span>
              </Button>
            </div>
          </div>

          {/* Device Info */}
          <div className="flex items-center gap-2 text-xs text-muted-foreground mt-2">
            <span>
              {currentDevice === 'mobile' && 'Mobile (375px × 667px)'}
              {currentDevice === 'tablet' && 'Tablet (768px × 1024px)'}
              {currentDevice === 'desktop' && 'Desktop (100%)'}
            </span>
            <span>•</span>
            <span>Template: {config.templateId}</span>
          </div>
        </CardHeader>

        <CardContent className="p-6 bg-gray-100">
          <div
            className={previewContainerClasses}
            style={{
              pointerEvents: 'none',
              userSelect: 'none',
            }}
            role="presentation"
            aria-label="Preview da landing page (apenas visualização)"
          >
            <TemplateComponent config={config} />
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

PreviewPanel.displayName = 'PreviewPanel';

export default PreviewPanel;
