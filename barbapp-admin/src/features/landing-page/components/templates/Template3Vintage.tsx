/**
 * Template 3 - Vintage
 * 
 * Template placeholder para preview da landing page.
 * Este é um componente temporário que será substituído pela implementação completa.
 * 
 * Tema: Retrô anos 50/60
 * Cores: Marrom, Vermelho escuro, Creme
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import React from 'react';
import { LandingPageConfig } from '../../types/landing-page.types';
import { BaseTemplatePreview } from './BaseTemplatePreview';

interface Template3VintageProps {
  config: LandingPageConfig;
}

export const Template3Vintage: React.FC<Template3VintageProps> = ({ config }) => {
  return (
    <BaseTemplatePreview
      config={config}
      templateName="Vintage"
      primaryColor="#5D4037"
      secondaryColor="#B71C1C"
      accentColor="#F5E6D3"
    />
  );
};

Template3Vintage.displayName = 'Template3Vintage';

export default Template3Vintage;
