import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import userEvent from '@testing-library/user-event';
import { ServiceForm } from '@/pages/Services/ServiceForm';
import type { BarbershopService } from '@/types';

// Mock data
const mockService: BarbershopService = {
  id: '1',
  name: 'Corte de Cabelo',
  description: 'Corte masculino completo',
  durationMinutes: 30,
  price: 25.0,
  isActive: true,
};

describe('ServiceForm', () => {
  it('renders form fields correctly', () => {
    const onSubmit = vi.fn();
    const onCancel = vi.fn();

    render(<ServiceForm onSubmit={onSubmit} onCancel={onCancel} />);

    expect(screen.getByLabelText('Nome *')).toBeInTheDocument();
    expect(screen.getByLabelText('Descrição *')).toBeInTheDocument();
    expect(screen.getByLabelText('Duração (minutos) *')).toBeInTheDocument();
    expect(screen.getByLabelText('Preço *')).toBeInTheDocument();
    expect(screen.getByText('Criar')).toBeInTheDocument();
    expect(screen.getByText('Cancelar')).toBeInTheDocument();
  });

  it('pre-fills form fields when editing', () => {
    const onSubmit = vi.fn();
    const onCancel = vi.fn();

    render(
      <ServiceForm
        service={mockService}
        onSubmit={onSubmit}
        onCancel={onCancel}
      />
    );

    expect(screen.getByDisplayValue('Corte de Cabelo')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Corte masculino completo')).toBeInTheDocument();
    expect(screen.getByDisplayValue('30')).toBeInTheDocument();
    expect(screen.getByDisplayValue('R$ 25,00')).toBeInTheDocument();
    expect(screen.getByText('Atualizar')).toBeInTheDocument();
  });

  it('formats price as Brazilian currency', () => {
    const onSubmit = vi.fn();
    const onCancel = vi.fn();

    render(<ServiceForm onSubmit={onSubmit} onCancel={onCancel} />);

    // Check that the price input displays formatted currency
    expect(screen.getByDisplayValue('R$ 0,00')).toBeInTheDocument();
  });

  it('calls onCancel when cancel button is clicked', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    const onCancel = vi.fn();

    render(<ServiceForm onSubmit={onSubmit} onCancel={onCancel} />);

    const cancelButton = screen.getByText('Cancelar');
    await user.click(cancelButton);

    expect(onCancel).toHaveBeenCalledOnce();
  });

  it('disables form when loading', () => {
    const onSubmit = vi.fn();
    const onCancel = vi.fn();

    render(
      <ServiceForm
        onSubmit={onSubmit}
        onCancel={onCancel}
        isLoading={true}
      />
    );

    expect(screen.getByLabelText('Nome *')).toBeDisabled();
    expect(screen.getByLabelText('Descrição *')).toBeDisabled();
    expect(screen.getByLabelText('Duração (minutos) *')).toBeDisabled();
    expect(screen.getByLabelText('Preço *')).toBeDisabled();
    expect(screen.getByText('Salvando...')).toBeInTheDocument();
  });

  it('validates required fields', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    const onCancel = vi.fn();

    render(<ServiceForm onSubmit={onSubmit} onCancel={onCancel} />);

    const submitButton = screen.getByText('Criar');
    await user.click(submitButton);

    // Form should not be submitted without required fields
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('validates duration is positive', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    const onCancel = vi.fn();

    render(<ServiceForm onSubmit={onSubmit} onCancel={onCancel} />);

    const nameInput = screen.getByLabelText('Nome *');
    const descriptionInput = screen.getByLabelText('Descrição *');
    const durationInput = screen.getByLabelText('Duração (minutos) *');

    await user.type(nameInput, 'Novo Serviço');
    await user.type(descriptionInput, 'Descrição do serviço');
    await user.clear(durationInput);
    await user.type(durationInput, '-5');

    const submitButton = screen.getByText('Criar');
    await user.click(submitButton);

    // Form should not be submitted with negative duration
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('submits form with valid data', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    const onCancel = vi.fn();

    render(<ServiceForm onSubmit={onSubmit} onCancel={onCancel} />);

    const nameInput = screen.getByLabelText('Nome *');
    const descriptionInput = screen.getByLabelText('Descrição *');
    const durationInput = screen.getByLabelText('Duração (minutos) *');

    await user.type(nameInput, 'Novo Serviço');
    await user.type(descriptionInput, 'Descrição do serviço');
    await user.clear(durationInput);
    await user.type(durationInput, '30');

    const submitButton = screen.getByText('Criar');
    await user.click(submitButton);

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledOnce();
    });

    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({
        name: 'Novo Serviço',
        description: 'Descrição do serviço',
        durationMinutes: 30,
      })
    );
  });
});
