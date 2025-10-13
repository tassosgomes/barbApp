import { describe, it, expect } from 'vitest';
import { barbershopSchema, loginSchema } from '@/schemas/barbershop.schema';

describe('Barbershop Schema', () => {
  describe('valid inputs', () => {
    it('should validate complete barbershop data', () => {
      const validData = {
        name: 'Barbearia Teste',
        document: '12.345.678/0001-99',
        ownerName: 'João Silva',
        email: 'teste@barbapp.com',
        phone: '(11) 99999-9999',
        address: {
          street: 'Rua Teste',
          number: '123',
          complement: 'Apto 45',
          neighborhood: 'Centro',
          city: 'São Paulo',
          state: 'SP',
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(validData);
      expect(result.success).toBe(true);
      if (result.success) {
        expect(result.data.name).toBe('Barbearia Teste');
        expect(result.data.document).toBe('12.345.678/0001-99');
        expect(result.data.ownerName).toBe('João Silva');
      }
    });

    it('should transform state to uppercase', () => {
      const data = {
        name: 'Test',
        document: '123.456.789-01',
        ownerName: 'Test Owner',
        email: 'test@test.com',
        phone: '(11) 99999-9999',
        address: {
          street: 'Street',
          number: '123',
          neighborhood: 'Area',
          city: 'City',
          state: 'sp', // lowercase
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(data);
      expect(result.success).toBe(true);
      if (result.success) {
        expect(result.data.address.state).toBe('SP'); // transformed
      }
    });
  });

  describe('invalid inputs', () => {
    it('should fail validation for short name', () => {
      const invalidData = {
        name: 'AB', // too short
        document: '12.345.678/0001-99',
        ownerName: 'João Silva',
        email: 'test@test.com',
        phone: '(11) 99999-9999',
        address: {
          street: 'Street',
          number: '123',
          neighborhood: 'Area',
          city: 'City',
          state: 'SP',
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toContain('mínimo 3 caracteres');
      }
    });

    it('should fail validation for invalid email', () => {
      const invalidData = {
        name: 'Test Name',
        document: '12.345.678/0001-99',
        ownerName: 'João Silva',
        email: 'invalid-email', // invalid format
        phone: '(11) 99999-9999',
        address: {
          street: 'Street',
          number: '123',
          neighborhood: 'Area',
          city: 'City',
          state: 'SP',
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Email inválido');
      }
    });

    it('should fail validation for invalid phone format', () => {
      const invalidData = {
        name: 'Test Name',
        document: '12.345.678/0001-99',
        ownerName: 'João Silva',
        email: 'test@test.com',
        phone: '11999999999', // missing formatting
        address: {
          street: 'Street',
          number: '123',
          neighborhood: 'Area',
          city: 'City',
          state: 'SP',
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toContain('Formato esperado');
      }
    });

    it('should fail validation for invalid document format', () => {
      const invalidData = {
        name: 'Test Name',
        document: '12345678901', // invalid format
        ownerName: 'João Silva',
        email: 'test@test.com',
        phone: '(11) 99999-9999',
        address: {
          street: 'Street',
          number: '123',
          neighborhood: 'Area',
          city: 'City',
          state: 'SP',
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toContain('Documento inválido');
      }
    });

    it('should fail validation for short owner name', () => {
      const invalidData = {
        name: 'Test Name',
        document: '12.345.678/0001-99',
        ownerName: 'AB', // too short
        email: 'test@test.com',
        phone: '(11) 99999-9999',
        address: {
          street: 'Street',
          number: '123',
          neighborhood: 'Area',
          city: 'City',
          state: 'SP',
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toContain('Nome do proprietário deve ter no mínimo 3 caracteres');
      }
    });
  });
});

describe('Login Schema', () => {
  it('should validate correct login credentials', () => {
    const validData = {
      email: 'admin@barbapp.com',
      password: 'SecurePass123',
    };

    const result = loginSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should fail validation for short password', () => {
    const invalidData = {
      email: 'admin@barbapp.com',
      password: '12345', // too short
    };

    const result = loginSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain('mínimo 6 caracteres');
    }
  });
});