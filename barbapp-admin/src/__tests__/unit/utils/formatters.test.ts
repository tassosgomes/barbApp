import { describe, it, expect } from 'vitest';
import { applyPhoneMask, applyZipCodeMask } from '@/utils/formatters';

describe('formatters', () => {
  describe('applyPhoneMask', () => {
    it('should format phone number correctly', () => {
      expect(applyPhoneMask('11999999999')).toBe('(11) 99999-9999');
      expect(applyPhoneMask('11988887777')).toBe('(11) 98888-7777');
    });

    it('should handle incomplete phone numbers', () => {
      expect(applyPhoneMask('11')).toBe('(11');
      expect(applyPhoneMask('119')).toBe('(11) 9');
      expect(applyPhoneMask('11999')).toBe('(11) 999');
      expect(applyPhoneMask('119999')).toBe('(11) 9999');
      expect(applyPhoneMask('1199999')).toBe('(11) 99999');
      expect(applyPhoneMask('11999999')).toBe('(11) 99999-9');
      expect(applyPhoneMask('119999999')).toBe('(11) 99999-99');
      expect(applyPhoneMask('1199999999')).toBe('(11) 99999-999');
      expect(applyPhoneMask('11999999999')).toBe('(11) 99999-9999');
    });

    it('should remove non-numeric characters', () => {
      expect(applyPhoneMask('(11) 99999-9999')).toBe('(11) 99999-9999');
      expect(applyPhoneMask('11-99999-9999')).toBe('(11) 99999-9999');
      expect(applyPhoneMask('11 99999 9999')).toBe('(11) 99999-9999');
    });

    it('should handle empty string', () => {
      expect(applyPhoneMask('')).toBe('');
    });
  });

  describe('applyZipCodeMask', () => {
    it('should format CEP correctly', () => {
      expect(applyZipCodeMask('01310100')).toBe('01310-100');
      expect(applyZipCodeMask('99999999')).toBe('99999-999');
    });

    it('should handle incomplete CEP', () => {
      expect(applyZipCodeMask('01310')).toBe('01310');
      expect(applyZipCodeMask('013101')).toBe('01310-1');
      expect(applyZipCodeMask('0131010')).toBe('01310-10');
    });

    it('should remove non-numeric characters', () => {
      expect(applyZipCodeMask('01310-100')).toBe('01310-100');
      expect(applyZipCodeMask('01310 100')).toBe('01310-100');
    });

    it('should handle empty string', () => {
      expect(applyZipCodeMask('')).toBe('');
    });
  });
});