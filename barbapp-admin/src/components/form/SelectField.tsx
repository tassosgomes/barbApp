import { FieldError, UseFormRegister, UseFormSetValue } from 'react-hook-form';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

export interface SelectOption {
  value: string;
  label: string;
}

interface SelectFieldProps {
  label: string;
  name: string;
  placeholder?: string;
  options: SelectOption[];
  error?: FieldError;
  register: UseFormRegister<any>; // eslint-disable-line @typescript-eslint/no-explicit-any
  setValue: UseFormSetValue<any>; // eslint-disable-line @typescript-eslint/no-explicit-any
  className?: string;
  required?: boolean;
}

export function SelectField({
  label,
  name,
  placeholder = 'Selecione...',
  options,
  error,
  register,
  setValue,
  className = '',
  required = false,
}: SelectFieldProps) {
  return (
    <div className={`space-y-2 ${className}`}>
      <Label htmlFor={name}>
        {label}
        {required && <span className="text-red-500 ml-1">*</span>}
      </Label>
      <Select
        onValueChange={(value) => setValue(name, value)}
        {...register(name)}
      >
        <SelectTrigger className={error ? 'border-red-500' : ''}>
          <SelectValue placeholder={placeholder} />
        </SelectTrigger>
        <SelectContent>
          {options.map((option) => (
            <SelectItem key={option.value} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      {error && (
        <p className="text-sm text-red-500">{error.message}</p>
      )}
    </div>
  );
}