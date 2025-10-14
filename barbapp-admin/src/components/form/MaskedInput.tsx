import React, { forwardRef, useCallback } from 'react';
import { Input } from '@/components/ui/input';
import { applyPhoneMask, applyZipCodeMask, applyDocumentMask } from '@/utils/formatters';

export type MaskType = 'phone' | 'cep' | 'document';

interface MaskedInputProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'onChange'> {
  mask: MaskType;
  onChange?: (value: string) => void;
}

export const MaskedInput = forwardRef<HTMLInputElement, MaskedInputProps>(
  ({ mask, onChange, value, ...props }, ref) => {
    // Apply mask to initial value if provided
    const maskedValue = value ? (
      mask === 'phone' ? applyPhoneMask(String(value)) :
      mask === 'cep' ? applyZipCodeMask(String(value)) :
      mask === 'document' ? applyDocumentMask(String(value)) :
      String(value)
    ) : value;

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
        value={maskedValue}
        onChange={handleChange}
      />
    );
  }
);

MaskedInput.displayName = 'MaskedInput';