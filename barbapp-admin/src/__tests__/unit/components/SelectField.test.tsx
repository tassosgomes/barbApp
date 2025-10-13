import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { useForm } from 'react-hook-form';
import { SelectField, type SelectOption } from '@/components/form/SelectField';

const mockOptions: SelectOption[] = [
  { value: 'option1', label: 'Option 1' },
  { value: 'option2', label: 'Option 2' },
  { value: 'option3', label: 'Option 3' },
];

function TestSelectField({ error }: { error?: boolean }) {
  const { register, setValue } = useForm();

  return (
    <SelectField
      label="Test Select"
      name="testSelect"
      options={mockOptions}
      register={register}
      setValue={setValue}
      error={error ? { type: 'manual', message: 'Test error' } : undefined}
      required
    />
  );
}

describe('SelectField', () => {
  it('should render label with required indicator', () => {
    render(<TestSelectField />);

    const label = screen.getByText('Test Select');
    expect(label).toBeInTheDocument();
    expect(label.tagName).toBe('LABEL');
    
    const asterisk = label.querySelector('span');
    expect(asterisk).toBeInTheDocument();
    expect(asterisk).toHaveTextContent('*');
    expect(asterisk).toHaveClass('text-red-500');
  });

  it('should render select trigger with placeholder', () => {
    render(<TestSelectField />);

    const trigger = screen.getByRole('combobox');
    expect(trigger).toBeInTheDocument();
    expect(screen.getByText('Selecione...')).toBeInTheDocument();
  });

  it('should display error message when error is provided', () => {
    render(<TestSelectField error />);

    const errorMessage = screen.getByText('Test error');
    expect(errorMessage).toBeInTheDocument();
    expect(errorMessage).toHaveClass('text-red-500');
  });

  it('should apply error styling to select when error exists', () => {
    render(<TestSelectField error />);

    const trigger = screen.getByRole('combobox');
    expect(trigger).toHaveClass('border-red-500');
  });

  it('should accept custom placeholder', () => {
    function TestSelectWithCustomPlaceholder() {
      const { register, setValue } = useForm();

      return (
        <SelectField
          label="Test Select"
          name="testSelect"
          options={mockOptions}
          register={register}
          setValue={setValue}
          placeholder="Choose an option"
        />
      );
    }

    render(<TestSelectWithCustomPlaceholder />);

    expect(screen.getByText('Choose an option')).toBeInTheDocument();
  });
});