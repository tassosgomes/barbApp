import { useQuery } from '@tanstack/react-query';
import { landingPageApi } from '@/services/api/landing-page.api';
import { TEMPLATES } from '../constants/templates';
import type { Template, UseTemplatesResult } from '../types/landing-page.types';

const QUERY_KEYS = {
  templates: ['landing-page-templates'] as const,
} as const;

export function useTemplates(): UseTemplatesResult {
  const {
    data: templates = TEMPLATES,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: QUERY_KEYS.templates,
    queryFn: () => landingPageApi.getTemplates(),
    staleTime: 60 * 60 * 1000, // 1 hour
    initialData: TEMPLATES,
    retry: 2,
  });

  const getTemplateById = (id: number): Template | undefined => {
    return templates.find(template => template.id === id);
  };

  const getTemplatesByTheme = (theme: string): Template[] => {
    return templates.filter(template => template.theme === theme);
  };

  const getAvailableThemes = (): string[] => {
    return Array.from(new Set(templates.map(template => template.theme)));
  };

  return {
    // Data
    templates,
    
    // Loading states
    isLoading,
    
    // Error states
    error,
    
    // Actions
    refetch,
    
    // Utils
    getTemplateById,
    getTemplatesByTheme,
    getAvailableThemes,
  };
}

export { QUERY_KEYS };