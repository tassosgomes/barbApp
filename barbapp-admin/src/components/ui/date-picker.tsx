import { forwardRef } from 'react';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';

interface DatePickerProps {
  value?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  className?: string;
  disabled?: boolean;
}

export const DatePicker = forwardRef<HTMLInputElement, DatePickerProps>(
  ({ value, onChange, placeholder = 'Selecione uma data', className, disabled, ...props }, ref) => {
    return (
      <Input
        ref={ref}
        type="date"
        value={value || ''}
        onChange={(e) => onChange?.(e.target.value)}
        placeholder={placeholder}
        className={cn('w-full', className)}
        disabled={disabled}
        {...props}
      />
    );
  }
);

DatePicker.displayName = 'DatePicker';