/**
 * TemplateGallery Component Tests
 *
 * Testes unitários para o componente TemplateGallery.
 * Verifica renderização, interações e acessibilidade.
 *
 * @version 1.0
 * @date 2025-10-21
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { TemplateGallery } from '../TemplateGallery';

describe('TemplateGallery', () => {
  const mockOnSelectTemplate = vi.fn();

  const defaultProps = {
    selectedTemplateId: 1,
    onSelectTemplate: mockOnSelectTemplate,
  };

  beforeEach(() => {
    mockOnSelectTemplate.mockClear();
  });

  it('should render all 5 templates', () => {
    render(<TemplateGallery {...defaultProps} />);

    // Verifica se todos os templates são renderizados
    expect(screen.getByText('Clássico')).toBeInTheDocument();
    expect(screen.getByText('Moderno')).toBeInTheDocument();
    expect(screen.getByText('Vintage')).toBeInTheDocument();
    expect(screen.getByText('Urbano')).toBeInTheDocument();
    expect(screen.getByText('Premium')).toBeInTheDocument();
  });

  it('should show selected template with check icon', () => {
    render(<TemplateGallery {...defaultProps} />);

    // Template 1 (Clássico) deve estar selecionado
    const selectedCard = screen.getByLabelText('Selecionar template Clássico');
    expect(selectedCard).toHaveAttribute('aria-pressed', 'true');

    // Deve ter o ícone de check
    const checkIcon = selectedCard.querySelector('svg');
    expect(checkIcon).toBeInTheDocument();
  });

  it('should call onSelectTemplate when template is clicked', () => {
    render(<TemplateGallery {...defaultProps} />);

    const modernTemplate = screen.getByLabelText('Selecionar template Moderno');
    fireEvent.click(modernTemplate);

    expect(mockOnSelectTemplate).toHaveBeenCalledWith(2);
    expect(mockOnSelectTemplate).toHaveBeenCalledTimes(1);
  });

  it('should handle keyboard navigation', () => {
    render(<TemplateGallery {...defaultProps} />);

    const vintageTemplate = screen.getByLabelText('Selecionar template Vintage');

    // Press Enter
    fireEvent.keyDown(vintageTemplate, { key: 'Enter' });
    expect(mockOnSelectTemplate).toHaveBeenCalledWith(3);

    // Press Space
    fireEvent.keyDown(vintageTemplate, { key: ' ' });
    expect(mockOnSelectTemplate).toHaveBeenCalledWith(3);
  });

  it('should display template descriptions', () => {
    render(<TemplateGallery {...defaultProps} />);

    expect(screen.getByText(/Elegante e tradicional/)).toBeInTheDocument();
    expect(screen.getByText(/Limpo e minimalista/)).toBeInTheDocument();
    expect(screen.getByText(/Estilo retrô anos 50\/60/)).toBeInTheDocument();
    expect(screen.getByText(/Visual street\/hip-hop/)).toBeInTheDocument();
    expect(screen.getByText(/Luxuoso e sofisticado/)).toBeInTheDocument();
  });

  it('should show color palette for each template', () => {
    render(<TemplateGallery {...defaultProps} />);

    // Verifica se as paletas de cores estão presentes
    const colorPalettes = screen.getAllByText('Cores:');
    expect(colorPalettes).toHaveLength(5);
  });

  it('should display theme badges', () => {
    render(<TemplateGallery {...defaultProps} />);

    expect(screen.getByText('classic')).toBeInTheDocument();
    expect(screen.getByText('modern')).toBeInTheDocument();
    expect(screen.getByText('vintage')).toBeInTheDocument();
    expect(screen.getByText('urban')).toBeInTheDocument();
    expect(screen.getByText('premium')).toBeInTheDocument();
  });

  it('should show loading state when loading prop is true', () => {
    render(<TemplateGallery {...defaultProps} loading={true} />);

    // Deve mostrar skeletons de carregamento
    const skeletonCards = document.querySelectorAll('.animate-pulse');
    expect(skeletonCards.length).toBe(5);
  });

  it('should have proper accessibility attributes', () => {
    render(<TemplateGallery {...defaultProps} />);

    const templates = screen.getAllByRole('button');
    expect(templates).toHaveLength(5);

    templates.forEach((template, index) => {
      expect(template).toHaveAttribute('aria-label', `Selecionar template ${['Clássico', 'Moderno', 'Vintage', 'Urbano', 'Premium'][index]}`);
      expect(template).toHaveAttribute('aria-pressed');
      expect(template).toHaveAttribute('tabIndex', '0');
    });
  });

  it('should render preview images with proper alt text', () => {
    render(<TemplateGallery {...defaultProps} />);

    const images = screen.getAllByAltText(/Preview do template/);
    expect(images).toHaveLength(5);

    expect(images[0]).toHaveAttribute('alt', 'Preview do template Clássico');
    expect(images[1]).toHaveAttribute('alt', 'Preview do template Moderno');
  });

  it('should have responsive grid classes', () => {
    const { container } = render(<TemplateGallery {...defaultProps} />);

    const grid = container.querySelector('.grid');
    expect(grid).toHaveClass('grid-cols-1', 'md:grid-cols-2', 'lg:grid-cols-3');
  });
});