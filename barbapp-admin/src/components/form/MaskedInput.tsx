import React, { forwardRef } from 'react';
import { Input } from '@/components/ui/input';
import { applyPhoneMask, applyZipCodeMask } from '@/utils/formatters';

export type MaskType = 'phone' | 'cep';

interface MaskedInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  mask: MaskType;
}

export const MaskedInput = forwardRef<HTMLInputElement, MaskedInputProps>(
  ({ mask, onChange, ...props }, ref) => {
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      let maskedValue = e.target.value;

      if (mask === 'phone') {
        maskedValue = applyPhoneMask(e.target.value);
      } else if (mask === 'cep') {
        maskedValue = applyZipCodeMask(e.target.value);
      }

      // Create a synthetic event with the masked value
      const syntheticEvent = {
        ...e,
        target: {
          ...e.target,
          value: maskedValue,
        },
      };

      onChange?.(syntheticEvent);
    };

    return (
      <Input
        {...props}
        ref={ref}
        onChange={handleChange}
      />
    );
  }
);

MaskedInput.displayName = 'MaskedInput';