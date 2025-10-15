import { http, HttpResponse } from 'msw';
import type { CreateBarbershopRequest, UpdateBarbershopRequest } from '../../types/barbershop';
import type { LoginRequest } from '../../types/auth';

// Mock data
const mockBarbershops = [
  {
    id: '1',
    name: 'Barbearia Teste',
    document: '12.345.678/0001-90',
    phone: '(11) 99999-9999',
    ownerName: 'João Silva',
    email: 'teste@barbapp.com',
    code: 'ABC123XY',
    address: {
      street: 'Rua Teste',
      number: '123',
      complement: '',
      neighborhood: 'Centro',
      city: 'São Paulo',
      state: 'SP',
      zipCode: '01000-000',
    },
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    name: 'Barbearia Inativa',
    document: '98.765.432/0001-10',
    phone: '(11) 88888-8888',
    ownerName: 'Maria Silva',
    email: 'inativa@barbapp.com',
    code: 'DEF456ZW',
    address: {
      street: 'Rua Inativa',
      number: '456',
      complement: '',
      neighborhood: 'Bairro',
      city: 'São Paulo',
      state: 'SP',
      zipCode: '02000-000',
    },
    isActive: false,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
];

// Mock handlers
export const handlers = [
  // GET /api/barbearias - List barbershops
  http.get('*/api/barbearias', ({ request }) => {
    const url = new URL(request.url);
    const pageNumber = parseInt(url.searchParams.get('pageNumber') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');
    const searchTerm = url.searchParams.get('searchTerm');
    const isActive = url.searchParams.get('isActive');

    let filteredBarbershops = mockBarbershops;

    // Filter by search term
    if (searchTerm) {
      filteredBarbershops = filteredBarbershops.filter(
        (barbershop) =>
          barbershop.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
          barbershop.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
          barbershop.address.city.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Filter by active status
    if (isActive !== null) {
      filteredBarbershops = filteredBarbershops.filter(
        (barbershop) => barbershop.isActive === (isActive === 'true')
      );
    }

    // Pagination
    const totalCount = filteredBarbershops.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const startIndex = (pageNumber - 1) * pageSize;
    const paginatedItems = filteredBarbershops.slice(startIndex, startIndex + pageSize);

    return HttpResponse.json({
      items: paginatedItems,
      pageNumber,
      pageSize,
      totalPages,
      totalCount,
      hasPreviousPage: pageNumber > 1,
      hasNextPage: pageNumber < totalPages,
    });
  }),

  // GET /api/barbearias/:id - Get barbershop by ID
  http.get('*/api/barbearias/:id', ({ params }) => {
    const { id } = params;
    const barbershop = mockBarbershops.find((b) => b.id === id);

    if (!barbershop) {
      return new HttpResponse(null, { status: 404 });
    }

    return HttpResponse.json(barbershop);
  }),

  // POST /api/barbearias - Create barbershop
  http.post('*/api/barbearias', async ({ request }) => {
    const body = await request.json() as CreateBarbershopRequest;
    const newBarbershop = {
      id: Date.now().toString(),
      name: body.name,
      document: body.document,
      phone: body.phone,
      ownerName: body.ownerName,
      email: body.email,
      code: `NEW${Date.now().toString().slice(-6)}AB`,
      address: {
        zipCode: body.zipCode,
        street: body.street,
        number: body.number,
        complement: body.complement || '',
        neighborhood: body.neighborhood,
        city: body.city,
        state: body.state,
      },
      isActive: true,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    mockBarbershops.push(newBarbershop);
    return HttpResponse.json(newBarbershop, { status: 201 });
  }),

  // PUT /api/barbearias/:id - Update barbershop
  http.put('*/api/barbearias/:id', async ({ request, params }) => {
    const { id } = params;
    const body = await request.json() as UpdateBarbershopRequest;
    const index = mockBarbershops.findIndex((b) => b.id === id);

    if (index === -1) {
      return new HttpResponse(null, { status: 404 });
    }

    mockBarbershops[index] = {
      ...mockBarbershops[index],
      name: body.name,
      phone: body.phone,
      ownerName: body.ownerName,
      email: body.email,
      address: {
        zipCode: body.zipCode,
        street: body.street,
        number: body.number,
        complement: body.complement || '',
        neighborhood: body.neighborhood,
        city: body.city,
        state: body.state,
      },
      updatedAt: new Date().toISOString(),
    };

    return HttpResponse.json(mockBarbershops[index]);
  }),

  // PUT /api/barbearias/:id/desativar - Deactivate barbershop
  http.put('*/api/barbearias/:id/desativar', ({ params }) => {
    const { id } = params;
    const barbershop = mockBarbershops.find((b) => b.id === id);

    if (!barbershop) {
      return new HttpResponse(null, { status: 404 });
    }

    barbershop.isActive = false;
    barbershop.updatedAt = new Date().toISOString();

    return HttpResponse.json(null);
  }),

  // PUT /api/barbearias/:id/reativar - Reactivate barbershop
  http.put('*/api/barbearias/:id/reativar', ({ params }) => {
    const { id } = params;
    const barbershop = mockBarbershops.find((b) => b.id === id);

    if (!barbershop) {
      return new HttpResponse(null, { status: 404 });
    }

    barbershop.isActive = true;
    barbershop.updatedAt = new Date().toISOString();

    return HttpResponse.json(null);
  }),

  // POST /api/auth/admin-central - Login
  http.post('*/api/auth/admin-central', async ({ request }) => {
    const body = await request.json() as LoginRequest;

    // Mock successful login
    if (body?.email && body?.password) {
      return HttpResponse.json({
        token: 'mock-jwt-token',
        user: {
          id: '1',
          email: body.email,
          name: 'Admin Central',
        },
      });
    }

    return new HttpResponse(null, { status: 401 });
  }),
];