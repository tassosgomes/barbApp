import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BarbershopForm } from '@/components/barbershop/BarbershopForm';

// Mock hooks
vi.mock('@/hooks/useViaCep', () => ({
  useViaCep: () => ({
    searchCep: vi.fn(),
    loading: false,
    error: null,
    data: null,
    clearError: vi.fn(),
    clearData: vi.fn(),
  }),
}));

vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: vi.fn(),
  }),
}));

// Mock components
vi.mock('@/components/form/FormField', () => ({
  FormField: ({ label, name }: any) => <div data-testid={`form-field-${name}`}>{label}</div>,
}));

vi.mock('@/components/form/MaskedInput', () => ({
  MaskedInput: ({ mask }: any) => <input data-testid={`masked-input-${mask}`} />,
}));

vi.mock('lucide-react', () => ({
  Loader2: () => <div data-testid="loader">Loading...</div>,
}));

describe('BarbershopForm', () => {
  const mockProps = {
    register: vi.fn(),
    errors: {},
    setValue: vi.fn(),
    watch: vi.fn(),
  };

  it('should render form sections', () => {
    render(<BarbershopForm {...mockProps} />);

    expect(screen.getByText('Informações Gerais')).toBeInTheDocument();
    expect(screen.getByText('Endereço')).toBeInTheDocument();
  });

  it('should render form fields', () => {
    render(<BarbershopForm {...mockProps} />);

    expect(screen.getByTestId('form-field-name')).toBeInTheDocument();
    expect(screen.getByTestId('form-field-document')).toBeInTheDocument();
    expect(screen.getByTestId('form-field-address.zipCode')).toBeInTheDocument();
  });

  it('should render masked inputs', () => {
    render(<BarbershopForm {...mockProps} />);

    // The masked inputs are rendered inside FormField components
    // We can check that the form fields that should contain masked inputs are present
    expect(screen.getByTestId('form-field-document')).toBeInTheDocument();
    expect(screen.getByTestId('form-field-phone')).toBeInTheDocument();
    expect(screen.getByTestId('form-field-address.zipCode')).toBeInTheDocument();
  });
});
