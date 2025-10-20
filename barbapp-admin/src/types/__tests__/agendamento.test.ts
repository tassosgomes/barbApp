import { describe, it, expect } from 'vitest';
import {
  translateAppointmentStatus,
  getAppointmentStatusVariant,
  getAppointmentStatusClass,
} from '../agendamento';

describe('agendamento types', () => {
  describe('translateAppointmentStatus', () => {
    it('traduz status 0 (Pending) para Pendente', () => {
      expect(translateAppointmentStatus(0)).toBe('Pendente');
    });

    it('traduz status 1 (Confirmed) para Confirmado', () => {
      expect(translateAppointmentStatus(1)).toBe('Confirmado');
    });

    it('traduz status 2 (Completed) para Concluído', () => {
      expect(translateAppointmentStatus(2)).toBe('Concluído');
    });

    it('traduz status 3 (Cancelled) para Cancelado', () => {
      expect(translateAppointmentStatus(3)).toBe('Cancelado');
    });

    it('retorna Desconhecido se não encontrar tradução', () => {
      expect(translateAppointmentStatus(99)).toBe('Desconhecido');
    });
  });

  describe('getAppointmentStatusVariant', () => {
    it('retorna secondary para 0 (Pending)', () => {
      expect(getAppointmentStatusVariant(0)).toBe('secondary');
    });

    it('retorna default para 1 (Confirmed)', () => {
      expect(getAppointmentStatusVariant(1)).toBe('default');
    });

    it('retorna outline para 2 (Completed)', () => {
      expect(getAppointmentStatusVariant(2)).toBe('outline');
    });

    it('retorna destructive para 3 (Cancelled)', () => {
      expect(getAppointmentStatusVariant(3)).toBe('destructive');
    });

    it('retorna default para status desconhecido', () => {
      expect(getAppointmentStatusVariant(99)).toBe('default');
    });
  });

  describe('getAppointmentStatusClass', () => {
    it('retorna classe amarela para 0 (Pending)', () => {
      const className = getAppointmentStatusClass(0);
      expect(className).toContain('yellow');
    });

    it('retorna classe azul para 1 (Confirmed)', () => {
      const className = getAppointmentStatusClass(1);
      expect(className).toContain('blue');
    });

    it('retorna classe verde para 2 (Completed)', () => {
      const className = getAppointmentStatusClass(2);
      expect(className).toContain('green');
    });

    it('retorna classe vermelha para 3 (Cancelled)', () => {
      const className = getAppointmentStatusClass(3);
      expect(className).toContain('red');
    });

    it('retorna string vazia para status desconhecido', () => {
      expect(getAppointmentStatusClass(99)).toBe('');
    });
  });
});
