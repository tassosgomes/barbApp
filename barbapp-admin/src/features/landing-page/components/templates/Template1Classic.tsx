/**
 * Template 1 - Clássico
 * 
 * Template placeholder para preview da landing page.
 * Este é um componente temporário que será substituído pela implementação completa.
 * 
 * Tema: Elegante e tradicional
 * Cores: Preto, Dourado, Branco
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import React from 'react';
import { LandingPageConfig } from '../../types/landing-page.types';
import { BaseTemplatePreview } from './BaseTemplatePreview';

interface Template1ClassicProps {
  config: LandingPageConfig;
}

export const Template1Classic: React.FC<Template1ClassicProps> = ({ config }) => {
  return (
    <BaseTemplatePreview
      config={config}
      templateName="Clássico"
      primaryColor="#1A1A1A"
      secondaryColor="#D4AF37"
      accentColor="#FFFFFF"
    />
  );
};

Template1Classic.displayName = 'Template1Classic';

export default Template1Classic;
