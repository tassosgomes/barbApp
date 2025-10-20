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
    render(
      <QueryClientProvider client={queryClient}>
        <BarbershopSelector />
      </QueryClientProvider>
    );

    // Clicar no trigger para abrir o dropdown
    const trigger = screen.getByRole('combobox');
    fireEvent.click(trigger);

    // Verificar se todas as barbearias estão listadas
    await waitFor(() => {
      expect(screen.getByText('Barbearia A')).toBeInTheDocument();
      expect(screen.getByText('Barbearia B')).toBeInTheDocument();
      expect(screen.getByText('Barbearia C')).toBeInTheDocument();
    });
  });

  it('deve chamar selectBarbershop ao selecionar uma barbearia', async () => {
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
      const option = screen.getByText('Barbearia B');
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

    render(
      <QueryClientProvider client={queryClient}>
        <BarbershopSelector />
      </QueryClientProvider>
    );

    // Abrir dropdown
    const trigger = screen.getByRole('combobox');
    fireEvent.click(trigger);

    // Verificar presença do check mark (ícone de Check)
    await waitFor(() => {
      const checkIcons = screen.getAllByTestId('lucide-check');
      expect(checkIcons.length).toBeGreaterThan(0);
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
