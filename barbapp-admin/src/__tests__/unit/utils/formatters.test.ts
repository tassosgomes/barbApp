import { describe, it, expect } from 'vitest';
import {
  applyPhoneMask,
  applyZipCodeMask,
  applyDocumentMask,
  formatDate,
} from '@/utils/formatters';

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

  describe('applyDocumentMask', () => {
    describe('CPF formatting', () => {
      it('should format CPF correctly', () => {
        expect(applyDocumentMask('12345678901')).toBe('123.456.789-01');
        expect(applyDocumentMask('98765432100')).toBe('987.654.321-00');
      });

      it('should handle incomplete CPF', () => {
        expect(applyDocumentMask('123')).toBe('123');
        expect(applyDocumentMask('1234')).toBe('123.4');
        expect(applyDocumentMask('12345')).toBe('123.45');
        expect(applyDocumentMask('123456')).toBe('123.456');
        expect(applyDocumentMask('1234567')).toBe('123.456.7');
        expect(applyDocumentMask('12345678')).toBe('123.456.78');
        expect(applyDocumentMask('123456789')).toBe('123.456.789');
        expect(applyDocumentMask('1234567890')).toBe('123.456.789-0');
      });

      it('should remove non-numeric characters from CPF', () => {
        expect(applyDocumentMask('123.456.789-01')).toBe('123.456.789-01');
        expect(applyDocumentMask('123 456 789 01')).toBe('123.456.789-01');
      });
    });

    describe('CNPJ formatting', () => {
      it('should format CNPJ correctly', () => {
        expect(applyDocumentMask('12345678901234')).toBe('12.345.678/9012-34');
        expect(applyDocumentMask('98765432109876')).toBe('98.765.432/1098-76');
      });

      it('should handle incomplete CNPJ (12+ digits)', () => {
        // When transitioning from CPF to CNPJ length (12+ digits)
        expect(applyDocumentMask('123456789012')).toBe('12.345.678/9012');
        expect(applyDocumentMask('1234567890123')).toBe('12.345.678/9012-3');
      });

      it('should handle intermediate lengths (5-11 digits) with partial formatting', () => {
        // These lengths could be CPF being typed, so formatting varies
        expect(applyDocumentMask('12')).toBe('12');
        expect(applyDocumentMask('123')).toBe('123');
        expect(applyDocumentMask('1234')).toBe('123.4');
        expect(applyDocumentMask('12345')).toBe('123.45');
        expect(applyDocumentMask('123456')).toBe('123.456');
        expect(applyDocumentMask('1234567')).toBe('123.456.7');
        expect(applyDocumentMask('12345678')).toBe('123.456.78');
        expect(applyDocumentMask('123456789')).toBe('123.456.789');
        expect(applyDocumentMask('1234567890')).toBe('123.456.789-0');
        expect(applyDocumentMask('12345678901')).toBe('123.456.789-01');
      });

      it('should remove non-numeric characters from CNPJ', () => {
        expect(applyDocumentMask('12.345.678/9012-34')).toBe('12.345.678/9012-34');
        expect(applyDocumentMask('12 345 678 9012 34')).toBe('12.345.678/9012-34');
      });
    });

    it('should limit to maximum 14 digits (CNPJ length)', () => {
      expect(applyDocumentMask('123456789012345678')).toBe('12.345.678/9012-34');
    });

    it('should handle empty string', () => {
      expect(applyDocumentMask('')).toBe('');
    });
  });

  describe('formatDate', () => {
    it('should format ISO date to pt-BR format', () => {
      const isoDate = '2024-03-15T14:30:00.000Z';
      const formatted = formatDate(isoDate);
      // The result will depend on timezone, but should match pt-BR format
      expect(formatted).toMatch(/\d{2}\/\d{2}\/\d{4}, \d{2}:\d{2}/);
    });

    it('should handle different ISO date formats', () => {
      const isoDate1 = '2024-06-15T12:00:00Z';
      const formatted1 = formatDate(isoDate1);
      // Should match pt-BR format regardless of timezone conversion
      expect(formatted1).toMatch(/\d{2}\/\d{2}\/\d{4}, \d{2}:\d{2}/);

      const isoDate2 = '2023-12-25T10:30:00Z';
      const formatted2 = formatDate(isoDate2);
      expect(formatted2).toMatch(/\d{2}\/\d{2}\/\d{4}, \d{2}:\d{2}/);
    });
  });
});