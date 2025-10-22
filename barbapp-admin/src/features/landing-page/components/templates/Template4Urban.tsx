/**
 * Template 4 - Urbano
 * 
 * Template placeholder para preview da landing page.
 * Este é um componente temporário que será substituído pela implementação completa.
 * 
 * Tema: Street/Hip-hop
 * Cores: Preto, Vermelho vibrante, Cinza
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import React from 'react';
import { LandingPageConfig } from '../../types/landing-page.types';
import { BaseTemplatePreview } from './BaseTemplatePreview';

interface Template4UrbanProps {
  config: LandingPageConfig;
}

export const Template4Urban: React.FC<Template4UrbanProps> = ({ config }) => {
  return (
    <BaseTemplatePreview
      config={config}
      templateName="Urbano"
      primaryColor="#000000"
      secondaryColor="#E74C3C"
      accentColor="#95A5A6"
    />
  );
};

Template4Urban.displayName = 'Template4Urban';

export default Template4Urban;
