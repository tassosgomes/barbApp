/// <reference types="vitest" />
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BarbershopSelector } from '@/components/layout/BarbershopSelector';
import { vi, describe, it, expect, beforeEach } from 'vitest';

// Mock do hook useBarbershopContext
const mockSelectBarbershop = vi.fn();
const mockContext = {
  currentBarbershop: null,
  selectBarbershop: mockSelectBarbershop,
  isSelected: false,
  availableBarbershops: [
    { id: '1', name: 'Barbearia A' },
    { id: '2', name: 'Barbearia B' },
    { id: '3', name: 'Barbearia C' },
  ],
};

vi.mock('@/hooks/useBarbershopContext', () => ({
  useBarbershopContext: () => mockContext,
}));

describe('BarbershopSelector', () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });

  beforeEach(() => {
    mockSelectBarbershop.mockClear();
  });

  it('deve renderizar o seletor com placeholder quando nenhuma barbearia está selecionada', () => {
    render(
      <QueryClientProvider client={queryClient}>
        <BarbershopSelector />
      </QueryClientProvider>
    );

    expect(screen.getByText('Selecione a barbearia')).toBeInTheDocument();
  });

  it('deve exibir o nome da barbearia atual quando selecionada', () => {
    mockContext.currentBarbershop = { id: '1', name: 'Barbearia A' };

    render(
      <QueryClientProvider client={queryClient}>
        <BarbershopSelector />
      </QueryClientProvider>
    );

    expect(screen.getByText('Barbearia A')).toBeInTheDocument();
  });

  it('deve listar todas as barbearias disponíveis ao abrir o dropdown', async () => {
    // Reset currentBarbershop to avoid duplicate text issues
    mockContext.currentBarbershop = null;

    render(
      <QueryClientProvider client={queryClient}>
        <BarbershopSelector />
      </QueryClientProvider>
    );

    // Clicar no trigger para abrir o dropdown
    const trigger = screen.getByRole('combobox');
    fireEvent.click(trigger);

    // Verificar se todas as barbearias estão listadas usando role para evitar ambiguidade
    await waitFor(() => {
      const options = screen.getAllByRole('option');
      expect(options).toHaveLength(3);
      expect(screen.getByText('Barbearia A')).toBeInTheDocument();
      expect(screen.getByText('Barbearia B')).toBeInTheDocument();
      expect(screen.getByText('Barbearia C')).toBeInTheDocument();
    });
  });

  it('deve chamar selectBarbershop ao selecionar uma barbearia', async () => {
    // Reset currentBarbershop to avoid duplicate text issues
    mockContext.currentBarbershop = null;

    render(
      <QueryClientProvider client={queryClient}>
        <BarbershopSelector />
      </QueryClientProvider>
    );

    // Abrir dropdown
    const trigger = screen.getByRole('combobox');
    fireEvent.click(trigger);

    // Selecionar uma barbearia
    await waitFor(() => {
      const option = screen.getByRole('option', { name: /Barbearia B/i });
      fireEvent.click(option);
    });

    // Verificar que selectBarbershop foi chamado com a barbearia correta
    expect(mockSelectBarbershop).toHaveBeenCalledWith({
      id: '2',
      name: 'Barbearia B',
    });
  });

  it('deve exibir check mark na barbearia atualmente selecionada', async () => {
    mockContext.currentBarbershop = { id: '2', name: 'Barbearia B' };

    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <BarbershopSelector />
      </QueryClientProvider>
    );

    // Abrir dropdown
    const trigger = screen.getByRole('combobox');
    fireEvent.click(trigger);

    // Verificar presença do check mark (ícone de Check via classe lucide-check)
    await waitFor(() => {
      const checkIcon = container.querySelector('.lucide-check') 
        || document.querySelector('.lucide-check');
      expect(checkIcon).toBeInTheDocument();
    });
  });

  it('deve ter largura mínima de 200px', () => {
    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <BarbershopSelector />
      </QueryClientProvider>
    );

    const trigger = container.querySelector('[class*="min-w-"]');
    expect(trigger).toBeInTheDocument();
    expect(trigger).toHaveClass('min-w-[200px]');
  });
});
