import { render, screen } from '@testing-library/react';
import { userEvent } from '@testing-library/user-event';
import { describe, it, expect } from 'vitest';
import { MaskedInput } from '@/components/form/MaskedInput';

describe('MaskedInput', () => {
  it('should render input with forwarded props', () => {
    render(<MaskedInput mask="phone" placeholder="Enter phone" data-testid="phone-input" />);

    const input = screen.getByTestId('phone-input');
    expect(input).toBeInTheDocument();
    expect(input).toHaveAttribute('placeholder', 'Enter phone');
  });

  it('should apply phone mask correctly', async () => {
    const user = userEvent.setup();
    let value = '';
    const onChange = (val: string) => {
      value = val;
    };

    render(<MaskedInput mask="phone" data-testid="phone-input" onChange={onChange} />);

    const input = screen.getByTestId('phone-input');

    await user.type(input, '11999999999');
    expect(value).toBe('(11) 99999-9999');
  });

  it('should apply CEP mask correctly', async () => {
    const user = userEvent.setup();
    let value = '';
    const onChange = (val: string) => {
      value = val;
    };

    render(<MaskedInput mask="cep" data-testid="cep-input" onChange={onChange} />);

    const input = screen.getByTestId('cep-input');

    await user.type(input, '01310100');
    expect(value).toBe('01310-100');
  });

  it('should handle incomplete phone numbers', async () => {
    const user = userEvent.setup();
    let value = '';
    const onChange = (val: string) => {
      value = val;
    };

    render(<MaskedInput mask="phone" data-testid="phone-input" onChange={onChange} />);

    const input = screen.getByTestId('phone-input');

    await user.type(input, '11');
    expect(value).toBe('(11');

    await user.clear(input);
    await user.type(input, '119');
    expect(value).toBe('(11) 9');

    await user.clear(input);
    await user.type(input, '11999');
    expect(value).toBe('(11) 999');
  });

  it('should handle incomplete CEP', async () => {
    const user = userEvent.setup();
    let value = '';
    const onChange = (val: string) => {
      value = val;
    };

    render(<MaskedInput mask="cep" data-testid="cep-input" onChange={onChange} />);

    const input = screen.getByTestId('cep-input');

    await user.type(input, '01310');
    expect(value).toBe('01310');

    await user.clear(input);
    await user.type(input, '013101');
    expect(value).toBe('01310-1');
  });

  it('should remove non-numeric characters before applying mask', async () => {
    const user = userEvent.setup();
    let value = '';
    const onChange = (val: string) => {
      value = val;
    };

    render(<MaskedInput mask="phone" data-testid="phone-input" onChange={onChange} />);

    const input = screen.getByTestId('phone-input');

    await user.type(input, '(11) 99999-9999');
    expect(value).toBe('(11) 99999-9999');
  });

  it('should forward ref correctly', () => {
    const ref = { current: null };
    render(<MaskedInput mask="phone" ref={ref} data-testid="phone-input" />);

    const input = screen.getByTestId('phone-input');
    expect(ref.current).toBe(input);
  });
});