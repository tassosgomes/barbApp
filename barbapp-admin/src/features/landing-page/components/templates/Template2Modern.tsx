/**
 * Template 2 - Moderno
 * 
 * Template placeholder para preview da landing page.
 * Este é um componente temporário que será substituído pela implementação completa.
 * 
 * Tema: Limpo e minimalista
 * Cores: Cinza escuro, Azul elétrico, Branco
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import React from 'react';
import { LandingPageConfig } from '../../types/landing-page.types';
import { BaseTemplatePreview } from './BaseTemplatePreview';

interface Template2ModernProps {
  config: LandingPageConfig;
}

export const Template2Modern: React.FC<Template2ModernProps> = ({ config }) => {
  return (
    <BaseTemplatePreview
      config={config}
      templateName="Moderno"
      primaryColor="#2C3E50"
      secondaryColor="#3498DB"
      accentColor="#ECF0F1"
    />
  );
};

Template2Modern.displayName = 'Template2Modern';

export default Template2Modern;
