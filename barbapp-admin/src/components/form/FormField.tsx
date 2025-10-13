import React from 'react';
import { FieldError, UseFormRegister } from 'react-hook-form';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

interface FormFieldProps {
  label: string;
  name: string;
  type?: string;
  placeholder?: string;
  error?: FieldError;
  register: UseFormRegister<any>; // eslint-disable-line @typescript-eslint/no-explicit-any
  className?: string;
  required?: boolean;
  children?: React.ReactNode; // For custom input components
}

export function FormField({
  label,
  name,
  type = 'text',
  placeholder,
  error,
  register,
  className = '',
  required = false,
  children,
}: FormFieldProps) {
  return (
    <div className={`space-y-2 ${className}`}>
      <Label htmlFor={name}>
        {label}
        {required && <span className="text-red-500 ml-1">*</span>}
      </Label>
      {children ? (
        React.cloneElement(children as React.ReactElement, {
          id: name,
          ...((children as React.ReactElement).props.onChange ? {} : register(name)), // Don't override onChange if child has it
          className: error ? 'border-red-500' : '',
        })
      ) : (
        <Input
          id={name}
          type={type}
          placeholder={placeholder}
          {...register(name)}
          className={error ? 'border-red-500' : ''}
        />
      )}
      {error && (
        <p className="text-sm text-red-500">{error.message}</p>
      )}
    </div>
  );
}