import React, { forwardRef, useCallback } from 'react';
import { Input } from '@/components/ui/input';
import { applyPhoneMask, applyZipCodeMask, applyDocumentMask } from '@/utils/formatters';

export type MaskType = 'phone' | 'cep' | 'document';

interface MaskedInputProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'onChange'> {
  mask: MaskType;
  onChange?: (value: string) => void;
}

export const MaskedInput = forwardRef<HTMLInputElement, MaskedInputProps>(
  ({ mask, onChange, ...props }, ref) => {
    const handleChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
      let maskedValue = e.target.value;

      if (mask === 'phone') {
        maskedValue = applyPhoneMask(e.target.value);
      } else if (mask === 'cep') {
        maskedValue = applyZipCodeMask(e.target.value);
      } else if (mask === 'document') {
        maskedValue = applyDocumentMask(e.target.value);
      }

      // Update the input value
      e.target.value = maskedValue;

      // Call the onChange prop if provided
      onChange?.(maskedValue);
    }, [mask, onChange]);

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