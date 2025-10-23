import React from 'react';
import { useParams } from 'react-router-dom';
import { useLandingPageData } from '@/hooks/useLandingPageData';
import { LoadingState, ErrorState } from '@/components';

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

  // TODO: Implement template selection based on data.landingPage.templateId
  return (
    <div className="min-h-screen bg-white">
      <h1>{data.barbershop.name}</h1>
      <p>Template ID: {data.landingPage.templateId}</p>
      {/* Placeholder for template component */}
    </div>
  );
};