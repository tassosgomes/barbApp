import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import { useTemplates } from '@/features/landing-page/hooks/useTemplates';
import { landingPageApi } from '@/services/api/landing-page.api';
import { TEMPLATES } from '@/features/landing-page/constants/templates';

// Mock the API
vi.mock('@/services/api/landing-page.api', () => ({
  landingPageApi: {
    getTemplates: vi.fn(),
  },
}));

const mockApiTemplates = [
  {
    id: 1,
    name: 'Classic',
    theme: 'classic',
    description: 'Traditional and elegant design',
    previewImage: '/classic.png',
    colors: {
      primary: '#1A1A1A',
      secondary: '#D4AF37',
      accent: '#FFFFFF',
      background: '#F8F8F8',
      text: '#2C2C2C',
    },
    config: {
      primaryFont: 'Playfair Display',
      secondaryFont: 'Inter',
      headerLayout: 'center',
      serviceCardStyle: 'classic',
      animations: true,
    },
  },
  {
    id: 2,
    name: 'Modern',
    theme: 'modern',
    description: 'Clean and contemporary design',
    previewImage: '/modern.png',
    colors: {
      primary: '#2C3E50',
      secondary: '#3498DB',
      accent: '#ECF0F1',
      background: '#FFFFFF',
      text: '#2C3E50',
    },
    config: {
      primaryFont: 'Inter',
      secondaryFont: 'Roboto',
      headerLayout: 'left',
      serviceCardStyle: 'modern',
      animations: true,
    },
  },
];

describe('useTemplates', () => {
  let queryClient: QueryClient;

  const createWrapper = ({ children }: { children: React.ReactNode }) => {
    return React.createElement(QueryClientProvider, { client: queryClient }, children);
  };

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    vi.clearAllMocks();
  });

  describe('Data fetching', () => {
    it('should return static templates as initial data', () => {
      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      expect(result.current.templates).toEqual(TEMPLATES);
      expect(result.current.isLoading).toBe(false);
    });

    it('should fetch templates from API successfully', async () => {
      vi.mocked(landingPageApi.getTemplates).mockResolvedValue(mockApiTemplates);

      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(landingPageApi.getTemplates).toHaveBeenCalled();
      expect(result.current.templates).toEqual(mockApiTemplates);
    });

    it('should handle fetch errors gracefully and fallback to static templates', async () => {
      const error = new Error('Network error');
      vi.mocked(landingPageApi.getTemplates).mockRejectedValue(error);

      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      // Should still have initial data despite error
      expect(result.current.templates).toEqual(TEMPLATES);
      expect(result.current.error).toBeTruthy();
    });
  });

  describe('Utility functions', () => {
    it('should get template by id correctly', () => {
      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      const template = result.current.getTemplateById(1);
      expect(template).toBeDefined();
      expect(template?.id).toBe(1);
      expect(template?.name).toBe('Clássico');
    });

    it('should return undefined for non-existent template id', () => {
      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      const template = result.current.getTemplateById(999);
      expect(template).toBeUndefined();
    });

    it('should get templates by theme correctly', () => {
      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      const classicTemplates = result.current.getTemplatesByTheme('classic');
      expect(classicTemplates).toHaveLength(1);
      expect(classicTemplates[0].theme).toBe('classic');

      const modernTemplates = result.current.getTemplatesByTheme('modern');
      expect(modernTemplates).toHaveLength(1);
      expect(modernTemplates[0].theme).toBe('modern');
    });

    it('should return empty array for non-existent theme', () => {
      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      const templates = result.current.getTemplatesByTheme('non-existent');
      expect(templates).toEqual([]);
    });

    it('should get available themes correctly', () => {
      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      const themes = result.current.getAvailableThemes();
      expect(themes).toContain('classic');
      expect(themes).toContain('modern');
      expect(themes).toContain('vintage');
      expect(themes).toContain('urban');
      expect(themes).toContain('premium');
      expect(themes).toHaveLength(5);
    });

    it('should return unique themes only', () => {
      vi.mocked(landingPageApi.getTemplates).mockResolvedValue([
        ...mockApiTemplates,
        // Duplicate theme
        {
          id: 3,
          name: 'Another Classic',
          theme: 'classic',
          description: 'Another classic template',
          previewImage: '/classic2.png',
          colors: mockApiTemplates[0].colors,
          config: mockApiTemplates[0].config,
        },
      ]);

      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      waitFor(() => {
        const themes = result.current.getAvailableThemes();
        const uniqueThemes = Array.from(new Set(themes));
        expect(themes).toEqual(uniqueThemes);
      });
    });
  });

  describe('Refetch functionality', () => {
    it('should provide refetch function', () => {
      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      expect(typeof result.current.refetch).toBe('function');
    });

    it('should refetch templates when called', async () => {
      vi.mocked(landingPageApi.getTemplates).mockResolvedValue(mockApiTemplates);

      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      // Clear the mock to track new calls
      vi.clearAllMocks();

      // Call refetch
      await result.current.refetch();

      expect(landingPageApi.getTemplates).toHaveBeenCalledTimes(1);
    });
  });

  describe('Template validation', () => {
    it('should handle templates with all required properties', () => {
      const { result } = renderHook(() => useTemplates(), {
        wrapper: createWrapper,
      });

      result.current.templates.forEach(template => {
        expect(template).toHaveProperty('id');
        expect(template).toHaveProperty('name');
        expect(template).toHaveProperty('theme');
        expect(template).toHaveProperty('description');
        expect(template).toHaveProperty('previewImage');
        expect(template).toHaveProperty('colors');
        expect(template).toHaveProperty('config');

        // Validate colors object
        expect(template.colors).toHaveProperty('primary');
        expect(template.colors).toHaveProperty('secondary');
        expect(template.colors).toHaveProperty('accent');
        expect(template.colors).toHaveProperty('background');
        expect(template.colors).toHaveProperty('text');

        // Validate config object
        expect(template.config).toHaveProperty('primaryFont');
        expect(template.config).toHaveProperty('secondaryFont');
        expect(template.config).toHaveProperty('headerLayout');
        expect(template.config).toHaveProperty('serviceCardStyle');
        expect(template.config).toHaveProperty('animations');
      });
    });
  });
});