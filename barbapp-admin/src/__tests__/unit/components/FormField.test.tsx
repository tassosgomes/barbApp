import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { useForm } from 'react-hook-form';
import { FormField } from '@/components/form/FormField';

function TestFormField({ error }: { error?: boolean }) {
  const { register } = useForm();

  return (
    <FormField
      label="Test Label"
      name="testField"
      register={register}
      error={error ? { type: 'manual', message: 'Test error' } : undefined}
      required
    />
  );
}

describe('FormField', () => {
  it('should render label with required indicator', () => {
    render(<TestFormField />);

    const label = screen.getByText('Test Label');
    expect(label).toBeInTheDocument();
    expect(label.tagName).toBe('LABEL');
    
    const asterisk = label.querySelector('span');
    expect(asterisk).toBeInTheDocument();
    expect(asterisk).toHaveTextContent('*');
    expect(asterisk).toHaveClass('text-red-500');
  });

  it('should render input with correct attributes', () => {
    render(<TestFormField />);

    const input = screen.getByLabelText(/Test Label/);
    expect(input).toBeInTheDocument();
    expect(input).toHaveAttribute('id', 'testField');
    expect(input).toHaveAttribute('type', 'text');
  });

  it('should display error message when error is provided', () => {
    render(<TestFormField error />);

    const errorMessage = screen.getByText('Test error');
    expect(errorMessage).toBeInTheDocument();
    expect(errorMessage).toHaveClass('text-red-500');
  });

  it('should apply error styling to input when error exists', () => {
    render(<TestFormField error />);

    const input = screen.getByLabelText(/Test Label/);
    expect(input).toHaveClass('border-red-500');
  });

  it('should accept custom input component via children', () => {
    function TestFormWithChildren() {
      const { register } = useForm();

      return (
        <FormField
          label="Test Label"
          name="testField"
          register={register}
        >
          <textarea data-testid="custom-input" />
        </FormField>
      );
    }

    render(<TestFormWithChildren />);

    const customInput = screen.getByTestId('custom-input');
    expect(customInput).toBeInTheDocument();
    expect(customInput).toHaveAttribute('id', 'testField');
  });
});