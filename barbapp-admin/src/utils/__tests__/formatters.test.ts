import { describe, it, expect } from 'vitest';
import { formatCurrency, formatDuration, formatDateTime, formatDateOnly, formatTimeOnly } from '../formatters';

describe('formatters', () => {
  describe('formatCurrency', () => {
    it('formata valores decimais corretamente', () => {
      expect(formatCurrency(35.5)).toBe('R$\u00a035,50');
      expect(formatCurrency(100.99)).toBe('R$\u00a0100,99');
      expect(formatCurrency(1000.5)).toBe('R$\u00a01.000,50');
    });

    it('formata valores inteiros com centavos zerados', () => {
      expect(formatCurrency(35)).toBe('R$\u00a035,00');
      expect(formatCurrency(100)).toBe('R$\u00a0100,00');
    });

    it('formata valores com muitas casas decimais arredondando', () => {
      expect(formatCurrency(35.999)).toBe('R$\u00a036,00');
      expect(formatCurrency(35.994)).toBe('R$\u00a035,99');
    });

    it('formata zero corretamente', () => {
      expect(formatCurrency(0)).toBe('R$\u00a00,00');
    });

    it('formata valores grandes com separador de milhar', () => {
      expect(formatCurrency(1000)).toBe('R$\u00a01.000,00');
      expect(formatCurrency(10000.5)).toBe('R$\u00a010.000,50');
      expect(formatCurrency(100000)).toBe('R$\u00a0100.000,00');
    });

    it('formata valores negativos', () => {
      expect(formatCurrency(-35.5)).toBe('-R$\u00a035,50');
      expect(formatCurrency(-100)).toBe('-R$\u00a0100,00');
    });
  });

  describe('formatDuration', () => {
    it('formata durações apenas em minutos (menos de 60min)', () => {
      expect(formatDuration(15)).toBe('15min');
      expect(formatDuration(30)).toBe('30min');
      expect(formatDuration(45)).toBe('45min');
      expect(formatDuration(59)).toBe('59min');
    });

    it('formata durações em horas exatas', () => {
      expect(formatDuration(60)).toBe('1h');
      expect(formatDuration(120)).toBe('2h');
      expect(formatDuration(180)).toBe('3h');
    });

    it('formata durações em horas e minutos', () => {
      expect(formatDuration(90)).toBe('1h 30min');
      expect(formatDuration(75)).toBe('1h 15min');
      expect(formatDuration(135)).toBe('2h 15min');
      expect(formatDuration(150)).toBe('2h 30min');
    });

    it('formata durações longas', () => {
      expect(formatDuration(240)).toBe('4h');
      expect(formatDuration(245)).toBe('4h 5min');
      expect(formatDuration(480)).toBe('8h');
    });

    it('formata zero minutos', () => {
      expect(formatDuration(0)).toBe('0min');
    });

    it('formata 1 minuto', () => {
      expect(formatDuration(1)).toBe('1min');
    });

    it('formata 1 hora e 1 minuto', () => {
      expect(formatDuration(61)).toBe('1h 1min');
    });
  });

  describe('formatDateTime', () => {
    it('formata data e hora corretamente', () => {
      const isoDate = '2024-01-15T14:30:00Z';
      const formatted = formatDateTime(isoDate);
      // Formato PT-BR: dd/MM/yyyy, HH:mm (com vírgula)
      expect(formatted).toMatch(/\d{2}\/\d{2}\/\d{4},\s\d{2}:\d{2}/);
    });

    it('formata data com timezone', () => {
      const isoDate = '2024-12-25T09:00:00-03:00';
      const formatted = formatDateTime(isoDate);
      expect(formatted).toMatch(/\d{2}\/\d{2}\/\d{4},\s\d{2}:\d{2}/);
    });
  });

  describe('formatDateOnly', () => {
    it('formata apenas a data', () => {
      const isoDate = '2024-01-15';
      const formatted = formatDateOnly(isoDate);
      expect(formatted).toMatch(/\d{2}\/\d{2}\/\d{4}/);
      expect(formatted).not.toContain(':');
    });
  });

  describe('formatTimeOnly', () => {
    it('formata apenas a hora', () => {
      const isoDate = '2024-01-15T14:30:00Z';
      const formatted = formatTimeOnly(isoDate);
      expect(formatted).toMatch(/\d{2}:\d{2}/);
      expect(formatted).not.toContain('/');
    });
  });
});
