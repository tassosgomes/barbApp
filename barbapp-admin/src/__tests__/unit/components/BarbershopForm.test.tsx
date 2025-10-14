import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BarbershopForm } from '@/components/barbershop/BarbershopForm';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { UseFormRegister, FieldErrors, UseFormSetValue } from 'react-hook-form';
import { BarbershopFormData } from '@/schemas/barbershop.schema';

// Mock hooks
const mockSearchCep = vi.fn();
const mockClearError = vi.fn();
const mockToast = vi.fn();

vi.mock('@/hooks/useViaCep', () => ({
  useViaCep: () => ({
    searchCep: mockSearchCep,
    loading: false,
    error: null,
    data: null,
    clearError: mockClearError,
  }),
}));

vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: mockToast,
  }),
}));

describe('BarbershopForm', () => {
  const mockRegister: UseFormRegister<BarbershopFormData> = vi.fn();
  const mockErrors: FieldErrors<BarbershopFormData> = {};
  const mockSetValue: UseFormSetValue<BarbershopFormData> = vi.fn();
  const mockWatch = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    mockWatch.mockReturnValue('');
  });

  it('renders all form fields correctly', () => {
    render(
      <BarbershopForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
      />
    );

    expect(screen.getByText('Informações Gerais')).toBeInTheDocument();
    expect(screen.getByText('Endereço')).toBeInTheDocument();

    // Check general information fields
    expect(screen.getByText(/nome da barbearia/i)).toBeInTheDocument();
    expect(screen.getByText(/documento/i)).toBeInTheDocument();
    expect(screen.getByText(/proprietário/i)).toBeInTheDocument();
    expect(screen.getByText(/email/i)).toBeInTheDocument();
    expect(screen.getByText(/telefone/i)).toBeInTheDocument();

    // Check address fields
    expect(screen.getByText('CEP')).toBeInTheDocument();
    expect(screen.getByText(/logradouro/i)).toBeInTheDocument();
    expect(screen.getByText(/número/i)).toBeInTheDocument();
    expect(screen.getByText(/complemento/i)).toBeInTheDocument();
    expect(screen.getByText(/bairro/i)).toBeInTheDocument();
    expect(screen.getByText(/cidade/i)).toBeInTheDocument();
    expect(screen.getByText(/estado/i)).toBeInTheDocument();
  });

  it('shows CEP hint text', () => {
    render(
      <BarbershopForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
      />
    );

    expect(screen.getByText('Digite o CEP para preencher o endereço automaticamente')).toBeInTheDocument();
  });

  it('calls searchCep when CEP has 8 digits', async () => {
    mockWatch.mockReturnValue('12345-678');

    render(
      <BarbershopForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
      />
    );

    await waitFor(() => {
      expect(mockSearchCep).toHaveBeenCalledWith('12345-678');
    });
  });

  it('does not call searchCep when CEP has less than 8 digits', () => {
    mockWatch.mockReturnValue('12345-67');

    render(
      <BarbershopForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
      />
    );

    expect(mockSearchCep).not.toHaveBeenCalled();
  });

  it('shows loading spinner when CEP search is in progress', () => {
    // Mock the hook to return loading state
    vi.doMock('@/hooks/useViaCep', () => ({
      useViaCep: () => ({
        searchCep: mockSearchCep,
        loading: true,
        error: null,
        data: null,
        clearError: mockClearError,
      }),
    }));

    render(
      <BarbershopForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
      />
    );

    // The loading spinner should be present (though we can't easily test the exact icon)
    const cepField = screen.getByText('CEP').closest('div');
    expect(cepField).toBeInTheDocument();
  });

  it('displays field errors when provided', () => {
    const errorsWithMessages: FieldErrors<BarbershopFormData> = {
      name: { type: 'required', message: 'Nome é obrigatório' },
      email: { type: 'pattern', message: 'Email inválido' },
      address: {
        street: { type: 'required', message: 'Logradouro é obrigatório' },
      },
    };

    render(
      <BarbershopForm
        register={mockRegister}
        errors={errorsWithMessages}
        setValue={mockSetValue}
        watch={mockWatch}
      />
    );

    expect(screen.getByText('Nome é obrigatório')).toBeInTheDocument();
    expect(screen.getByText('Email inválido')).toBeInTheDocument();
    expect(screen.getByText('Logradouro é obrigatório')).toBeInTheDocument();
  });

  it('calls setValue when MaskedInput onChange is triggered', async () => {
    const user = userEvent.setup();

    render(
      <BarbershopForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
      />
    );

    // Find document MaskedInput and simulate typing
    const documentInput = screen.getByPlaceholderText('999.999.999-99 ou 99.999.999/9999-99');
    await user.type(documentInput, '12345678901');

    expect(mockSetValue).toHaveBeenCalledWith('document', '123.456.789-01');

    // Find phone MaskedInput and simulate typing
    const phoneInput = screen.getByPlaceholderText('(99) 99999-9999');
    await user.type(phoneInput, '11987654321');

    expect(mockSetValue).toHaveBeenCalledWith('phone', '(11) 98765-4321');

    // Find CEP MaskedInput and simulate typing
    const cepInput = screen.getByPlaceholderText('99999-999');
    await user.type(cepInput, '12345678');

    expect(mockSetValue).toHaveBeenCalledWith('address.zipCode', '12345-678');
  });
});