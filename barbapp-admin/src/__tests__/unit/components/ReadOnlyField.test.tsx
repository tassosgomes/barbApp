import { render, screen } from '@testing-library/react';
import { ReadOnlyField } from '@/components/form/ReadOnlyField';
import { describe, it, expect } from 'vitest';

describe('ReadOnlyField', () => {
  it('renders label and value correctly', () => {
    render(<ReadOnlyField label="Test Label" value="Test Value" />);

    expect(screen.getByText('Test Label')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Test Value')).toBeInTheDocument();
  });

  it('applies custom className', () => {
    const { container } = render(
      <ReadOnlyField label="Test Label" value="Test Value" className="custom-class" />
    );

    const fieldContainer = container.firstChild;
    expect(fieldContainer).toHaveClass('custom-class');
  });

  it('renders input as read-only', () => {
    render(<ReadOnlyField label="Test Label" value="Test Value" />);

    const input = screen.getByDisplayValue('Test Value');
    expect(input).toHaveAttribute('readonly');
    expect(input).toHaveClass('bg-gray-50', 'cursor-not-allowed');
  });

  it('handles empty value', () => {
    render(<ReadOnlyField label="Test Label" value="" />);

    expect(screen.getByText('Test Label')).toBeInTheDocument();
    expect(screen.getByDisplayValue('')).toBeInTheDocument();
  });

  it('handles special characters in value', () => {
    const specialValue = 'Test@#$%^&*()_+{}|:<>?[]\\;\'",./';
    render(<ReadOnlyField label="Special Label" value={specialValue} />);

    expect(screen.getByText('Special Label')).toBeInTheDocument();
    expect(screen.getByDisplayValue(specialValue)).toBeInTheDocument();
  });
});