import { describe, it, expect } from 'vitest';
import {
  translateAppointmentStatus,
  getAppointmentStatusVariant,
  getAppointmentStatusClass,
} from '../agendamento';

describe('agendamento types', () => {
  describe('translateAppointmentStatus', () => {
    it('traduz status Pending para Pendente', () => {
      expect(translateAppointmentStatus('Pending')).toBe('Pendente');
    });

    it('traduz status Confirmed para Confirmado', () => {
      expect(translateAppointmentStatus('Confirmed')).toBe('Confirmado');
    });

    it('traduz status Completed para Concluído', () => {
      expect(translateAppointmentStatus('Completed')).toBe('Concluído');
    });

    it('traduz status Cancelled para Cancelado', () => {
      expect(translateAppointmentStatus('Cancelled')).toBe('Cancelado');
    });

    it('retorna o status original se não encontrar tradução', () => {
      expect(translateAppointmentStatus('Unknown')).toBe('Unknown');
    });
  });

  describe('getAppointmentStatusVariant', () => {
    it('retorna secondary para Pending', () => {
      expect(getAppointmentStatusVariant('Pending')).toBe('secondary');
    });

    it('retorna default para Confirmed', () => {
      expect(getAppointmentStatusVariant('Confirmed')).toBe('default');
    });

    it('retorna outline para Completed', () => {
      expect(getAppointmentStatusVariant('Completed')).toBe('outline');
    });

    it('retorna destructive para Cancelled', () => {
      expect(getAppointmentStatusVariant('Cancelled')).toBe('destructive');
    });

    it('retorna default para status desconhecido', () => {
      expect(getAppointmentStatusVariant('Unknown')).toBe('default');
    });
  });

  describe('getAppointmentStatusClass', () => {
    it('retorna classe amarela para Pending', () => {
      const className = getAppointmentStatusClass('Pending');
      expect(className).toContain('yellow');
    });

    it('retorna classe azul para Confirmed', () => {
      const className = getAppointmentStatusClass('Confirmed');
      expect(className).toContain('blue');
    });

    it('retorna classe verde para Completed', () => {
      const className = getAppointmentStatusClass('Completed');
      expect(className).toContain('green');
    });

    it('retorna classe vermelha para Cancelled', () => {
      const className = getAppointmentStatusClass('Cancelled');
      expect(className).toContain('red');
    });

    it('retorna string vazia para status desconhecido', () => {
      expect(getAppointmentStatusClass('Unknown')).toBe('');
    });
  });
});
