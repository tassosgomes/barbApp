/**
 * Template 5 - Premium
 * 
 * Template placeholder para preview da landing page.
 * Este é um componente temporário que será substituído pela implementação completa.
 * 
 * Tema: Luxuoso e sofisticado
 * Cores: Preto, Dourado metálico, Cinza escuro
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import React from 'react';
import { LandingPageConfig } from '../../types/landing-page.types';
import { BaseTemplatePreview } from './BaseTemplatePreview';

interface Template5PremiumProps {
  config: LandingPageConfig;
}

export const Template5Premium: React.FC<Template5PremiumProps> = ({ config }) => {
  return (
    <BaseTemplatePreview
      config={config}
      templateName="Premium"
      primaryColor="#1C1C1C"
      secondaryColor="#C9A961"
      accentColor="#2E2E2E"
    />
  );
};

Template5Premium.displayName = 'Template5Premium';

export default Template5Premium;
