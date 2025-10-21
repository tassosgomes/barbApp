/**
 * ServiceManager Component Tests
 *
 * Testes unitários para o componente ServiceManager.
 * Verifica renderização, drag-and-drop, controles de visibilidade e ações em lote.
 *
 * @version 1.0
 * @date 2025-10-21
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ServiceManager } from '../ServiceManager';
import { LandingPageService } from '../../types/landing-page.types';

// Mock do @hello-pangea/dnd
vi.mock('@hello-pangea/dnd', () => ({
  DragDropContext: ({ children, onDragEnd }: {
    children: React.ReactNode;
    onDragEnd?: (result: unknown) => void;
  }) => (
    <div data-testid="drag-drop-context" data-ondragend={onDragEnd ? 'true' : 'false'}>
      {children}
    </div>
  ),
  Droppable: ({ children }: { children: (provided: unknown) => React.ReactNode }) => (
    <div data-testid="droppable">
      {children({ innerRef: null, droppableProps: {}, placeholder: null })}
    </div>
  ),
  Draggable: ({ children, draggableId }: {
    children: (provided: unknown, snapshot: unknown) => React.ReactNode;
    draggableId: string;
  }) => (
    <div data-testid={`draggable-${draggableId}`}>
      {children({ innerRef: null, draggableProps: {}, dragHandleProps: {} }, { isDragging: false })}
    </div>
  ),
}));

describe('ServiceManager', () => {
  const mockOnChange = vi.fn();

  const mockServices: LandingPageService[] = [
    {
      serviceId: '1',
      serviceName: 'Corte de Cabelo',
      description: 'Corte masculino completo',
      duration: 30,
      price: 25.00,
      displayOrder: 1,
      isVisible: true,
    },
    {
      serviceId: '2',
      serviceName: 'Barba',
      description: 'Aparação e modelagem',
      duration: 20,
      price: 15.00,
      displayOrder: 2,
      isVisible: false,
    },
    {
      serviceId: '3',
      serviceName: 'Corte + Barba',
      description: 'Pacote completo',
      duration: 50,
      price: 35.00,
      displayOrder: 3,
      isVisible: true,
    },
  ];

  const defaultProps = {
    services: mockServices,
    onChange: mockOnChange,
  };

  beforeEach(() => {
    mockOnChange.mockClear();
  });

  describe('Renderização básica', () => {
    it('should render all services', () => {
      render(<ServiceManager {...defaultProps} />);

      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
      expect(screen.getByText('Barba')).toBeInTheDocument();
      expect(screen.getByText('Corte + Barba')).toBeInTheDocument();
    });

    it('should display service count', () => {
      render(<ServiceManager {...defaultProps} />);

      expect(screen.getByText('3 serviços')).toBeInTheDocument();
    });

    it('should format prices correctly', () => {
      render(<ServiceManager {...defaultProps} />);

      expect(screen.getByText('R$ 25,00')).toBeInTheDocument();
      expect(screen.getByText('R$ 15,00')).toBeInTheDocument();
      expect(screen.getByText('R$ 35,00')).toBeInTheDocument();
    });

    it('should format duration correctly', () => {
      render(<ServiceManager {...defaultProps} />);

      expect(screen.getByText('• 30min')).toBeInTheDocument();
      expect(screen.getByText('• 20min')).toBeInTheDocument();
      expect(screen.getByText('• 50min')).toBeInTheDocument();
    });

    it('should display service descriptions', () => {
      render(<ServiceManager {...defaultProps} />);

      expect(screen.getByText('Corte masculino completo')).toBeInTheDocument();
      expect(screen.getByText('Aparação e modelagem')).toBeInTheDocument();
      expect(screen.getByText('Pacote completo')).toBeInTheDocument();
    });

    it('should show visibility icons correctly', () => {
      render(<ServiceManager {...defaultProps} />);

      // Verifica se existem botões com ícones (botões de visibilidade)
      const buttonsWithIcons = screen.getAllByRole('button').filter(btn =>
        btn.querySelector('svg') !== null
      );

      // Deve haver pelo menos 3 botões com ícones (grip + 3 visibilidade)
      expect(buttonsWithIcons.length).toBeGreaterThanOrEqual(3);
    });
  });

  describe('Controles de seleção', () => {
    it('should show select all button when no services selected', () => {
      render(<ServiceManager {...defaultProps} />);

      expect(screen.getByText('Selecionar todos')).toBeInTheDocument();
    });

    it('should select all services when select all is clicked', () => {
      render(<ServiceManager {...defaultProps} />);

      const selectAllButton = screen.getByText('Selecionar todos');
      fireEvent.click(selectAllButton);

      // Verifica se todos os checkboxes estão marcados
      const checkboxes = screen.getAllByRole('checkbox');
      checkboxes.forEach(checkbox => {
        expect(checkbox).toBeChecked();
      });
    });

    it('should show bulk actions when services are selected', () => {
      render(<ServiceManager {...defaultProps} />);

      // Selecionar primeiro serviço
      const firstCheckbox = screen.getAllByRole('checkbox')[0];
      fireEvent.click(firstCheckbox);

      expect(screen.getByText('1 selecionado')).toBeInTheDocument();
      expect(screen.getByText('Mostrar')).toBeInTheDocument();
      expect(screen.getByText('Ocultar')).toBeInTheDocument();
      expect(screen.getByText('Limpar seleção')).toBeInTheDocument();
    });

    it('should toggle service selection', () => {
      render(<ServiceManager {...defaultProps} />);

      const firstCheckbox = screen.getAllByRole('checkbox')[0];
      expect(firstCheckbox).not.toBeChecked();

      fireEvent.click(firstCheckbox);
      expect(firstCheckbox).toBeChecked();

      fireEvent.click(firstCheckbox);
      expect(firstCheckbox).not.toBeChecked();
    });

    it('should clear selection when clear button is clicked', () => {
      render(<ServiceManager {...defaultProps} />);

      // Selecionar todos
      const selectAllButton = screen.getByText('Selecionar todos');
      fireEvent.click(selectAllButton);

      // Limpar seleção
      const clearButton = screen.getByText('Limpar seleção');
      fireEvent.click(clearButton);

      // Verifica se nenhum checkbox está marcado
      const checkboxes = screen.getAllByRole('checkbox');
      checkboxes.forEach(checkbox => {
        expect(checkbox).not.toBeChecked();
      });
    });
  });

  describe('Controles de visibilidade', () => {
    it('should toggle service visibility when eye button is clicked', () => {
      render(<ServiceManager {...defaultProps} />);

      // Encontra o botão de visibilidade do primeiro serviço (que está visível)
      const visibilityButtons = screen.getAllByRole('button').filter(btn =>
        btn.querySelector('svg') // Tem ícone (Eye ou EyeOff)
      );

      // Clica no primeiro botão de visibilidade
      fireEvent.click(visibilityButtons[0]);

      expect(mockOnChange).toHaveBeenCalledWith([
        { ...mockServices[0], isVisible: false },
        mockServices[1],
        mockServices[2],
      ]);
    });

    it('should toggle service visibility when eye-off button is clicked', () => {
      render(<ServiceManager {...defaultProps} />);

      // Encontra o botão de visibilidade do segundo serviço (que está invisível)
      const visibilityButtons = screen.getAllByRole('button').filter(btn =>
        btn.querySelector('svg') // Tem ícone (Eye ou EyeOff)
      );

      // Clica no segundo botão de visibilidade
      fireEvent.click(visibilityButtons[1]);

      expect(mockOnChange).toHaveBeenCalledWith([
        mockServices[0],
        { ...mockServices[1], isVisible: true },
        mockServices[2],
      ]);
    });
  });

  describe('Ações em lote', () => {
    it('should show selected services when bulk actions are available', () => {
      render(<ServiceManager {...defaultProps} />);

      // Selecionar primeiro serviço
      const firstCheckbox = screen.getAllByRole('checkbox')[0];
      fireEvent.click(firstCheckbox);

      expect(screen.getByText('1 selecionado')).toBeInTheDocument();
    });

    it('should hide selected services when bulk hide is clicked', () => {
      render(<ServiceManager {...defaultProps} />);

      // Selecionar primeiro e terceiro serviços
      const checkboxes = screen.getAllByRole('checkbox');
      fireEvent.click(checkboxes[0]); // Primeiro serviço (visível)
      fireEvent.click(checkboxes[2]); // Terceiro serviço (visível)

      const hideButton = screen.getByText('Ocultar');
      fireEvent.click(hideButton);

      expect(mockOnChange).toHaveBeenCalledWith([
        { ...mockServices[0], isVisible: false },
        mockServices[1],
        { ...mockServices[2], isVisible: false },
      ]);
    });

    it('should show selected services when bulk show is clicked', () => {
      render(<ServiceManager {...defaultProps} />);

      // Selecionar segundo serviço (invisível)
      const checkboxes = screen.getAllByRole('checkbox');
      fireEvent.click(checkboxes[1]);

      const showButton = screen.getByText('Mostrar');
      fireEvent.click(showButton);

      expect(mockOnChange).toHaveBeenCalledWith([
        mockServices[0],
        { ...mockServices[1], isVisible: true },
        mockServices[2],
      ]);
    });

    it('should clear selection after bulk actions', () => {
      render(<ServiceManager {...defaultProps} />);

      // Selecionar um serviço
      const firstCheckbox = screen.getAllByRole('checkbox')[0];
      fireEvent.click(firstCheckbox);

      // Executar ação em lote
      const hideButton = screen.getByText('Ocultar');
      fireEvent.click(hideButton);

      // Verifica se seleção foi limpa
      expect(screen.queryByText('1 selecionado')).not.toBeInTheDocument();
      expect(screen.getByText('Selecionar todos')).toBeInTheDocument();
    });
  });

  describe('Estado disabled', () => {
    it('should disable all interactive elements when disabled prop is true', () => {
      render(<ServiceManager {...defaultProps} disabled={true} />);

      // Verifica se botões estão desabilitados
      const buttons = screen.getAllByRole('button');
      buttons.forEach(button => {
        expect(button).toBeDisabled();
      });

      // Verifica se checkboxes estão desabilitados
      const checkboxes = screen.getAllByRole('checkbox');
      checkboxes.forEach(checkbox => {
        expect(checkbox).toBeDisabled();
      });
    });

    it('should apply disabled styling when disabled prop is true', () => {
      render(<ServiceManager {...defaultProps} disabled={true} />);

      // Verifica se os cards têm a classe de disabled
      const cards = document.querySelectorAll('[class*="opacity-50"]');
      expect(cards.length).toBeGreaterThan(0);
    });

    it('should not call onChange when disabled', () => {
      render(<ServiceManager {...defaultProps} disabled={true} />);

      // Tenta clicar em um botão de visibilidade
      const buttons = screen.getAllByRole('button');
      const visibilityButton = buttons.find(btn => btn.querySelector('svg'));

      if (visibilityButton) {
        fireEvent.click(visibilityButton);
      }

      expect(mockOnChange).not.toHaveBeenCalled();
    });
  });

  describe('Estado vazio', () => {
    it('should show empty state message when no services', () => {
      render(<ServiceManager {...defaultProps} services={[]} />);

      expect(screen.getByText('Nenhum serviço configurado para esta landing page.')).toBeInTheDocument();
    });

    it('should disable select all button when no services', () => {
      render(<ServiceManager {...defaultProps} services={[]} />);

      const selectAllButton = screen.getByText('Selecionar todos');
      expect(selectAllButton).toBeDisabled();
    });
  });

  describe('Drag and drop', () => {
    it('should render drag drop context', () => {
      render(<ServiceManager {...defaultProps} />);

      expect(screen.getByTestId('drag-drop-context')).toBeInTheDocument();
      expect(screen.getByTestId('droppable')).toBeInTheDocument();
    });

    it('should render draggable items', () => {
      render(<ServiceManager {...defaultProps} />);

      expect(screen.getByTestId('draggable-1')).toBeInTheDocument();
      expect(screen.getByTestId('draggable-2')).toBeInTheDocument();
      expect(screen.getByTestId('draggable-3')).toBeInTheDocument();
    });

    it('should show grip vertical icons for drag handles', () => {
      render(<ServiceManager {...defaultProps} />);

      // Verifica se existem ícones de grip (GripVertical)
      const gripIcons = document.querySelectorAll('svg');
      const gripVerticalIcons = Array.from(gripIcons).filter(icon =>
        icon.classList.contains('lucide-grip-vertical') ||
        icon.getAttribute('data-icon') === 'grip-vertical'
      );

      expect(gripVerticalIcons).toHaveLength(3);
    });
  });

  describe('Sincronização de estado', () => {
    it('should update local state when services prop changes', () => {
      const { rerender } = render(<ServiceManager {...defaultProps} />);

      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();

      const newServices = [
        {
          serviceId: '4',
          serviceName: 'Lavagem',
          description: 'Lavagem completa',
          duration: 15,
          price: 10.00,
          displayOrder: 1,
          isVisible: true,
        },
      ];

      rerender(<ServiceManager {...defaultProps} services={newServices} />);

      expect(screen.getByText('Lavagem')).toBeInTheDocument();
      expect(screen.queryByText('Corte de Cabelo')).not.toBeInTheDocument();
    });
  });
});