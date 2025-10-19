import { describe, test, expect } from 'vitest';
import { applyPhoneMask, formatPhoneToAPI } from '../phone-utils';

describe('phone-utils', () => {
  describe('applyPhoneMask', () => {
    test('deve retornar apenas dígitos quando tiver 2 ou menos caracteres', () => {
      expect(applyPhoneMask('1')).toBe('1');
      expect(applyPhoneMask('11')).toBe('11');
    });

    test('deve aplicar máscara parcial (XX) XXXXX quando tiver entre 3 e 7 dígitos', () => {
      expect(applyPhoneMask('119')).toBe('(11) 9');
      expect(applyPhoneMask('11999')).toBe('(11) 999');
      expect(applyPhoneMask('1199999')).toBe('(11) 99999');
    });

    test('deve aplicar máscara completa (XX) XXXXX-XXXX quando tiver mais de 7 dígitos', () => {
      expect(applyPhoneMask('11999999')).toBe('(11) 99999-9');
      expect(applyPhoneMask('119999999')).toBe('(11) 99999-99');
      expect(applyPhoneMask('1199999999')).toBe('(11) 99999-999');
      expect(applyPhoneMask('11999999999')).toBe('(11) 99999-9999');
    });

    test('deve limitar a 11 dígitos mesmo se receber mais caracteres', () => {
      expect(applyPhoneMask('119999999999999')).toBe('(11) 99999-9999');
    });

    test('deve remover caracteres não numéricos antes de aplicar máscara', () => {
      expect(applyPhoneMask('(11) 99999-9999')).toBe('(11) 99999-9999');
      expect(applyPhoneMask('11 9 9999-9999')).toBe('(11) 99999-9999');
      expect(applyPhoneMask('11-99999-9999')).toBe('(11) 99999-9999');
    });

    test('deve lidar com strings vazias', () => {
      expect(applyPhoneMask('')).toBe('');
    });

    test('deve lidar com valores apenas com caracteres não numéricos', () => {
      expect(applyPhoneMask('abc-def')).toBe('');
    });
  });

  describe('formatPhoneToAPI', () => {
    test('deve converter telefone com máscara completa para formato API', () => {
      expect(formatPhoneToAPI('(11) 99999-9999')).toBe('+5511999999999');
    });

    test('deve converter telefone com máscara parcial para formato API', () => {
      expect(formatPhoneToAPI('(11) 9999')).toBe('+55119999');
    });

    test('deve adicionar +55 mesmo quando telefone já está sem máscara', () => {
      expect(formatPhoneToAPI('11999999999')).toBe('+5511999999999');
    });

    test('deve remover todos os caracteres não numéricos', () => {
      expect(formatPhoneToAPI('(11) 99999-9999')).toBe('+5511999999999');
      expect(formatPhoneToAPI('11 99999 9999')).toBe('+5511999999999');
      expect(formatPhoneToAPI('11-99999-9999')).toBe('+5511999999999');
    });

    test('deve lidar com strings vazias', () => {
      expect(formatPhoneToAPI('')).toBe('+55');
    });

    test('deve lidar com valores apenas com caracteres não numéricos', () => {
      expect(formatPhoneToAPI('abc-def')).toBe('+55');
    });

    test('deve funcionar com números de telefone incompletos', () => {
      expect(formatPhoneToAPI('(11) 9')).toBe('+55119');
      expect(formatPhoneToAPI('(11) 99')).toBe('+551199');
    });
  });
});
