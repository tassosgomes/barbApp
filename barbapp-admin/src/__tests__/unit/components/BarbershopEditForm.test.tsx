/* eslint-disable @typescript-eslint/no-explicit-any */
import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BarbershopEditForm } from '@/components/barbershop/BarbershopEditForm';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { UseFormRegister, FieldErrors, UseFormSetValue } from 'react-hook-form';
import { BarbershopEditFormData } from '@/schemas/barbershop.schema';

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

vi.mock('@/components/form/MaskedInput', () => ({
  MaskedInput: ({ mask, onChange, placeholder, value }: any) => {
    const [inputValue, setInputValue] = React.useState(value || '');

    return (
      <input
        placeholder={placeholder}
        value={inputValue}
        onChange={(e) => {
          const newValue = e.target.value;
          setInputValue(newValue);

          let maskedValue = newValue;
          if (mask === 'phone') {
            const cleaned = newValue.replace(/\D/g, '');
            if (cleaned.length <= 10) {
              maskedValue = cleaned.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
            } else {
              maskedValue = cleaned.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
            }
          } else if (mask === 'cep') {
            const cleaned = newValue.replace(/\D/g, '');
            maskedValue = cleaned.replace(/(\d{5})(\d{3})/, '$1-$2');
          }
          onChange?.(maskedValue);
        }}
      />
    );
  },
}));

describe('BarbershopEditForm', () => {
  const mockRegister: UseFormRegister<BarbershopEditFormData> = vi.fn();
  const mockErrors: FieldErrors<BarbershopEditFormData> = {};
  const mockSetValue: UseFormSetValue<BarbershopEditFormData> = vi.fn();
  const mockWatch = vi.fn();

  const mockReadOnlyData = {
    document: '123.456.789-01',
    code: 'ABC12345',
  };

  beforeEach(() => {
    vi.clearAllMocks();
    mockWatch.mockReturnValue('');
  });

  it('renders all form fields correctly', () => {
    render(
      <BarbershopEditForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
        readOnlyData={mockReadOnlyData}
      />
    );

    expect(screen.getByText('Informações Gerais')).toBeInTheDocument();
    expect(screen.getByText('Endereço')).toBeInTheDocument();

    // Check general information fields (no document field in edit form)
    expect(screen.getByText(/proprietário/i)).toBeInTheDocument();
    expect(screen.getByText(/email/i)).toBeInTheDocument();
    expect(screen.getByText(/telefone/i)).toBeInTheDocument();

    // Check read-only fields
    expect(screen.getByText('Documento (CPF/CNPJ)')).toBeInTheDocument();
    expect(screen.getByText('Código')).toBeInTheDocument();
    expect(screen.getByDisplayValue('123.456.789-01')).toBeInTheDocument();
    expect(screen.getByDisplayValue('ABC12345')).toBeInTheDocument();

    // Check address fields
    expect(screen.getByPlaceholderText('99999-999')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Rua, Avenida, etc.')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('123')).toBeInTheDocument();
    expect(screen.getByText(/complemento/i)).toBeInTheDocument();
    expect(screen.getByText(/bairro/i)).toBeInTheDocument();
    expect(screen.getByText(/cidade/i)).toBeInTheDocument();
    expect(screen.getByText(/estado/i)).toBeInTheDocument();
  });

  it('shows CEP hint text', () => {
    render(
      <BarbershopEditForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
        readOnlyData={mockReadOnlyData}
      />
    );

    expect(screen.getByText('Digite o CEP para preencher o endereço automaticamente')).toBeInTheDocument();
  });

  it('calls searchCep when CEP has 8 digits', async () => {
    mockWatch.mockReturnValue('12345-678');

    render(
      <BarbershopEditForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
        readOnlyData={mockReadOnlyData}
      />
    );

    await waitFor(() => {
      expect(mockSearchCep).toHaveBeenCalledWith('12345-678');
    });
  });

  it('does not call searchCep when CEP has less than 8 digits', () => {
    mockWatch.mockReturnValue('12345-67');

    render(
      <BarbershopEditForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
        readOnlyData={mockReadOnlyData}
      />
    );

    expect(mockSearchCep).not.toHaveBeenCalled();
  });

  it('displays field errors when provided', () => {
    const errorsWithMessages: FieldErrors<BarbershopEditFormData> = {
      name: { type: 'required', message: 'Nome é obrigatório' },
      email: { type: 'pattern', message: 'Email inválido' },
      address: {
        street: { type: 'required', message: 'Logradouro é obrigatório' },
      },
    };

    render(
      <BarbershopEditForm
        register={mockRegister}
        errors={errorsWithMessages}
        setValue={mockSetValue}
        watch={mockWatch}
        readOnlyData={mockReadOnlyData}
      />
    );

    expect(screen.getByText('Nome é obrigatório')).toBeInTheDocument();
    expect(screen.getByText('Email inválido')).toBeInTheDocument();
    expect(screen.getByText('Logradouro é obrigatório')).toBeInTheDocument();
  });

  it('calls setValue when MaskedInput onChange is triggered', async () => {
    const user = userEvent.setup();

    render(
      <BarbershopEditForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
        readOnlyData={mockReadOnlyData}
      />
    );

    // Find phone MaskedInput and simulate typing
    const phoneInput = screen.getByPlaceholderText('(99) 99999-9999');
    await user.type(phoneInput, '11987654321');

    // Wait for setValue to be called with the final value
    await waitFor(() => {
      expect(mockSetValue).toHaveBeenCalledWith('phone', expect.stringMatching(/\(\d{2}\) \d{5}-\d{4}/));
    });

    // Find CEP MaskedInput and simulate typing
    const cepInput = screen.getByPlaceholderText('99999-999');
    await user.type(cepInput, '12345678');

    expect(mockSetValue).toHaveBeenCalledWith('address.zipCode', '12345-678');
  });

  it('renders read-only fields with correct values', () => {
    render(
      <BarbershopEditForm
        register={mockRegister}
        errors={mockErrors}
        setValue={mockSetValue}
        watch={mockWatch}
        readOnlyData={mockReadOnlyData}
      />
    );

    const documentInput = screen.getByDisplayValue('123.456.789-01');
    const codeInput = screen.getByDisplayValue('ABC12345');

    expect(documentInput).toHaveAttribute('readonly');
    expect(codeInput).toHaveAttribute('readonly');
    expect(documentInput).toHaveClass('bg-gray-50', 'cursor-not-allowed');
    expect(codeInput).toHaveClass('bg-gray-50', 'cursor-not-allowed');
  });
});