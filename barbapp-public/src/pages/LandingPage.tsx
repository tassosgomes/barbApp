import React from 'react';
import { useParams } from 'react-router-dom';
import { useLandingPageData } from '@/hooks/useLandingPageData';
import { LoadingState, ErrorState } from '@/components';
import { Template1Classic, Template2Modern, Template3Vintage, Template4Urban, Template5Premium } from '@/templates';
import type { PublicLandingPage } from '@/types/landing-page.types';

interface TemplateComponentProps {
  data: PublicLandingPage;
}

type TemplateComponent = React.FC<TemplateComponentProps>;

const TEMPLATE_COMPONENTS: Record<number, TemplateComponent> = {
  1: Template1Classic,
  2: Template2Modern,
  3: Template3Vintage,
  4: Template4Urban,
  5: Template5Premium,
};

export const LandingPage: React.FC = () => {
  const { code } = useParams<{ code: string }>();
  const { data, isLoading, error } = useLandingPageData(code!);

  if (isLoading) {
    return <LoadingState />;
  }

  if (error || !data) {
    return (
      <ErrorState
        title="Landing page não encontrada"
        message="Verifique o código e tente novamente."
      />
    );
  }

  const TemplateComponent = TEMPLATE_COMPONENTS[data.landingPage.templateId] || Template1Classic;

  return <TemplateComponent data={data} />;
};