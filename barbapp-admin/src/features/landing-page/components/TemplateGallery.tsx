/**
 * TemplateGallery Component
 *
 * Componente para exibir galeria dos 5 templates com preview e seleção.
 * Permite ao admin visualizar e escolher entre os templates disponíveis.
 *
 * @see PRD: Seção 2.2 - Escolha e Troca de Templates
 * @see Tech Spec: TemplateGallery.tsx
 * @version 1.0
 * @date 2025-10-21
 */

import React from 'react';
import { Check } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';
import { TEMPLATES } from '../constants/templates';
import { TemplateGalleryProps } from '../types/landing-page.types';

/**
 * Componente TemplateGallery
 *
 * Renderiza um grid responsivo com os 5 templates disponíveis.
 * Cada template é exibido como um card com preview, nome, descrição e paleta de cores.
 * O template selecionado é destacado com borda e ícone de check.
 *
 * @param selectedTemplateId - ID do template atualmente selecionado
 * @param onSelectTemplate - Callback chamado quando um template é selecionado
 * @param loading - Se está carregando (opcional)
 */
export const TemplateGallery: React.FC<TemplateGalleryProps> = ({
  selectedTemplateId,
  onSelectTemplate,
  loading = false,
}) => {
  if (loading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {Array.from({ length: 5 }).map((_, index) => (
          <div
            key={index}
            className="border rounded-lg p-4 animate-pulse"
          >
            <div className="w-full h-48 bg-gray-200 rounded-md mb-4"></div>
            <div className="h-6 bg-gray-200 rounded mb-2"></div>
            <div className="h-4 bg-gray-200 rounded mb-4"></div>
            <div className="flex gap-2">
              {Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className="w-8 h-8 bg-gray-200 rounded-full"></div>
              ))}
            </div>
          </div>
        ))}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {TEMPLATES.map((template) => (
        <Card
          key={template.id}
          className={`
            relative cursor-pointer transition-all duration-200 hover:shadow-lg hover:scale-[1.02]
            ${
              selectedTemplateId === template.id
                ? 'ring-2 ring-primary shadow-lg'
                : ''
            }
          `}
          onClick={() => onSelectTemplate(template.id)}
          role="button"
          tabIndex={0}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              onSelectTemplate(template.id);
            }
          }}
          aria-label={`Selecionar template ${template.name}`}
          aria-pressed={selectedTemplateId === template.id}
        >
          <CardContent className="p-4">
          {/* Indicador de seleção */}
          {selectedTemplateId === template.id && (
            <div className="absolute top-3 right-3 bg-primary text-white rounded-full p-1.5 shadow-md">
              <Check size={16} />
            </div>
          )}

          {/* Preview Image */}
          <div className="relative mb-4 overflow-hidden rounded-md">
            <img
              src={template.previewImage}
              alt={`Preview do template ${template.name}`}
              className="w-full h-48 object-cover transition-transform duration-200 hover:scale-105"
              loading="lazy"
            />
            {/* Overlay sutil no hover */}
            <div className="absolute inset-0 bg-black/0 hover:bg-black/5 transition-colors duration-200"></div>
          </div>

          {/* Template Info */}
          <div className="space-y-3">
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-1">
                {template.name}
              </h3>
              <p className="text-sm text-gray-600 leading-relaxed">
                {template.description}
              </p>
            </div>

            {/* Color Palette */}
            <div className="flex items-center gap-2">
              <span className="text-xs text-gray-500 font-medium">Cores:</span>
              <div className="flex gap-2">
                {Object.values(template.colors).slice(0, 3).map((color, idx) => (
                  <div
                    key={idx}
                    className="w-6 h-6 rounded-full border-2 border-white shadow-sm"
                    style={{ backgroundColor: color }}
                    title={`Cor ${idx + 1}: ${color}`}
                  />
                ))}
              </div>
            </div>

            {/* Theme Badge */}
            <div className="flex items-center justify-between">
              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800 capitalize">
                {template.theme}
              </span>
              {selectedTemplateId === template.id && (
                <span className="text-xs text-primary font-medium">
                  Selecionado
                </span>
              )}
            </div>
          </div>

          </CardContent>
        </Card>
      ))}
    </div>
  );
};

export default TemplateGallery;