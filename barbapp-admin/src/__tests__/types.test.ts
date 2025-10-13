import { describe, it, expect } from 'vitest';
import type { Barbershop, PaginatedResponse } from '@/types';

describe('Type Definitions', () => {
  it('should compile barbershop types correctly', () => {
    const barbershop: Barbershop = {
      id: '1',
      name: 'Test Barber',
      document: '12345678901234',
      phone: '(11) 99999-9999',
      ownerName: 'João Silva',
      email: 'test@barber.com',
      code: 'ABC123XY',
      isActive: true,
      address: {
        zipCode: '01000-000',
        street: 'Main St',
        number: '123',
        neighborhood: 'Downtown',
        city: 'São Paulo',
        state: 'SP',
      },
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    };

    expect(barbershop).toBeDefined();
    expect(barbershop.id).toBe('1');
    expect(barbershop.document).toBe('12345678901234');
    expect(barbershop.ownerName).toBe('João Silva');
    expect(barbershop.code).toBe('ABC123XY');
  });

  it('should compile paginated response types correctly', () => {
    const response: PaginatedResponse<Barbershop> = {
      items: [],
      pageNumber: 1,
      pageSize: 20,
      totalPages: 1,
      totalCount: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    };

    expect(response).toBeDefined();
  });
});
