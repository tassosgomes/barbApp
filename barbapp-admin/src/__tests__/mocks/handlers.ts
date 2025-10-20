import { http, HttpResponse } from 'msw';
import type { CreateBarbershopRequest, UpdateBarbershopRequest } from '../../types/barbershop';
import type { LoginRequest } from '../../types/auth';
import type { CreateBarberRequest, UpdateBarberRequest } from '../../types/barber';
import type { CreateServiceRequest, UpdateServiceRequest } from '../../types/service';

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

const mockBarbers = [
  {
    id: '1',
    name: 'João Silva',
    email: 'joao@test.com',
    phoneFormatted: '(11) 99999-9999',
    services: [
      {
        id: '1',
        name: 'Corte de Cabelo',
      },
    ],
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    name: 'Maria Santos',
    email: 'maria@test.com',
    phoneFormatted: '(11) 88888-8888',
    services: [
      {
        id: '1',
        name: 'Corte de Cabelo',
      },
      {
        id: '2',
        name: 'Barba',
      },
    ],
    isActive: false,
    createdAt: '2024-01-02T00:00:00Z',
  },
];

const mockServices = [
  {
    id: '1',
    name: 'Corte de Cabelo',
    description: 'Corte masculino completo com lavagem',
    durationMinutes: 30,
    price: 25.00,
    isActive: true,
  },
  {
    id: '2',
    name: 'Barba',
    description: 'Aparação e modelagem da barba',
    durationMinutes: 20,
    price: 15.00,
    isActive: true,
  },
  {
    id: '3',
    name: 'Sobrancelha',
    description: 'Depilação e modelagem das sobrancelhas',
    durationMinutes: 10,
    price: 10.00,
    isActive: false,
  },
];

const mockAppointments = [
  {
    id: '1',
    barberId: '1',
    barberName: 'João Silva',
    customerId: '1',
    customerName: 'Cliente 1',
    startTime: '2024-10-16T09:00:00Z',
    endTime: '2024-10-16T09:30:00Z',
    serviceTitle: 'Corte de Cabelo',
    status: 'Confirmed',
  },
  {
    id: '2',
    barberId: '1',
    barberName: 'João Silva',
    customerId: '2',
    customerName: 'Cliente 2',
    startTime: '2024-10-16T10:00:00Z',
    endTime: '2024-10-16T10:20:00Z',
    serviceTitle: 'Barba',
    status: 'Pending',
  },
  {
    id: '3',
    barberId: '2',
    barberName: 'Maria Santos',
    customerId: '3',
    customerName: 'Cliente 3',
    startTime: '2024-10-16T14:00:00Z',
    endTime: '2024-10-16T14:30:00Z',
    serviceTitle: 'Corte de Cabelo',
    status: 'Cancelled',
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

  // GET /api/barbers - List barbers
  http.get('*/api/barbers', ({ request }) => {
    console.log('MSW: Handling GET /api/barbers request:', request.url);
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');
    const searchName = url.searchParams.get('searchName');
    const isActive = url.searchParams.get('isActive');

    let filteredBarbers = mockBarbers;

    // Filter by search name
    if (searchName) {
      filteredBarbers = filteredBarbers.filter(
        (barber) =>
          barber.name.toLowerCase().includes(searchName.toLowerCase()) ||
          barber.email.toLowerCase().includes(searchName.toLowerCase())
      );
    }

    // Filter by active status
    if (isActive !== null) {
      filteredBarbers = filteredBarbers.filter(
        (barber) => barber.isActive === (isActive === 'true')
      );
    }

    // Pagination
    const totalCount = filteredBarbers.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const startIndex = (page - 1) * pageSize;
    const paginatedItems = filteredBarbers.slice(startIndex, startIndex + pageSize);

    console.log('MSW: Returning barbers data:', { items: paginatedItems.length, totalCount });
    return HttpResponse.json({
      items: paginatedItems,
      pageNumber: page,
      pageSize,
      totalPages,
      totalCount,
      hasPreviousPage: page > 1,
      hasNextPage: page < totalPages,
    });
  }),

  // GET /api/barbers/:id - Get barber by ID
  http.get('*/api/barbers/:id', ({ params }) => {
    const { id } = params;
    const barber = mockBarbers.find((b) => b.id === id);

    if (!barber) {
      return new HttpResponse(null, { status: 404 });
    }

    return HttpResponse.json(barber);
  }),

  // POST /api/barbers - Create barber
  http.post('*/api/barbers', async ({ request }) => {
    const body = await request.json() as CreateBarberRequest;
    const newBarber = {
      id: Date.now().toString(),
      name: body.name,
      email: body.email,
      phoneFormatted: body.phone,
      services: [], // Would need to resolve service IDs to service summaries
      isActive: true,
      createdAt: new Date().toISOString(),
    };

    mockBarbers.push(newBarber);
    return HttpResponse.json(newBarber, { status: 201 });
  }),

  // PUT /api/barbers/:id - Update barber
  http.put('*/api/barbers/:id', async ({ request, params }) => {
    const { id } = params;
    const body = await request.json() as UpdateBarberRequest;
    const index = mockBarbers.findIndex((b) => b.id === id);

    if (index === -1) {
      return new HttpResponse(null, { status: 404 });
    }

    mockBarbers[index] = {
      ...mockBarbers[index],
      name: body.name,
      phoneFormatted: body.phone,
      services: [], // Would need to resolve service IDs
    };

    return HttpResponse.json(mockBarbers[index]);
  }),

  // DELETE /api/barbers/:id - Delete barber (deactivate)
  http.delete('*/api/barbers/:id', ({ params }) => {
    const { id } = params;
    const barber = mockBarbers.find((b) => b.id === id);

    if (!barber) {
      return new HttpResponse(null, { status: 404 });
    }

    barber.isActive = false;
    return new HttpResponse(null, { status: 204 });
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

  // GET /api/barbershop-services - List services
  http.get('*/api/barbershop-services', ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const pageSize = parseInt(url.searchParams.get('pageSize') || '20');
    const searchName = url.searchParams.get('searchName');
    const isActive = url.searchParams.get('isActive');

    let filteredServices = mockServices;

    // Filter by search name
    if (searchName) {
      filteredServices = filteredServices.filter(
        (service) => service.name.toLowerCase().includes(searchName.toLowerCase())
      );
    }

    // Filter by active status
    if (isActive !== null) {
      filteredServices = filteredServices.filter(
        (service) => service.isActive === (isActive === 'true')
      );
    }

    // Pagination
    const totalCount = filteredServices.length;
    const startIndex = (page - 1) * pageSize;
    const paginatedItems = filteredServices.slice(startIndex, startIndex + pageSize);

    return HttpResponse.json({
      services: paginatedItems,
      totalCount,
      page,
      pageSize,
    });
  }),

  // GET /api/barbershop-services/:id - Get service by ID
  http.get('*/api/barbershop-services/:id', ({ params }) => {
    const { id } = params;
    const service = mockServices.find((s) => s.id === id);

    if (!service) {
      return new HttpResponse(null, { status: 404 });
    }

    return HttpResponse.json(service);
  }),

  // POST /api/barbershop-services - Create service
  http.post('*/api/barbershop-services', async ({ request }) => {
    const body = await request.json() as CreateServiceRequest;
    const newService = {
      id: Date.now().toString(),
      name: body.name,
      description: body.description,
      durationMinutes: body.durationMinutes,
      price: body.price,
      isActive: true,
    };

    mockServices.push(newService);
    return HttpResponse.json(newService, { status: 201 });
  }),

  // PUT /api/barbershop-services/:id - Update service
  http.put('*/api/barbershop-services/:id', async ({ request, params }) => {
    const { id } = params;
    const body = await request.json() as UpdateServiceRequest;
    const index = mockServices.findIndex((s) => s.id === id);

    if (index === -1) {
      return new HttpResponse(null, { status: 404 });
    }

    mockServices[index] = {
      ...mockServices[index],
      name: body.name,
      description: body.description,
      durationMinutes: body.durationMinutes,
      price: body.price,
    };

    return HttpResponse.json(mockServices[index]);
  }),

  // DELETE /api/barbershop-services/:id - Delete service (deactivate)
  http.delete('*/api/barbershop-services/:id', ({ params }) => {
    const { id } = params;
    const service = mockServices.find((s) => s.id === id);

    if (!service) {
      return new HttpResponse(null, { status: 404 });
    }

    service.isActive = false;
    return new HttpResponse(null, { status: 204 });
  }),

  // GET /api/barbers/schedule - List schedule appointments
  http.get('*/api/barbers/schedule', ({ request }) => {
    const url = new URL(request.url);
    const date = url.searchParams.get('date');
    const barberId = url.searchParams.get('barberId');
    const status = url.searchParams.get('status');

    let filteredAppointments = mockAppointments;

    // Filter by date (YYYY-MM-DD)
    if (date) {
      filteredAppointments = filteredAppointments.filter(
        (appointment) => appointment.startTime.startsWith(date)
      );
    }

    // Filter by barber
    if (barberId) {
      filteredAppointments = filteredAppointments.filter(
        (appointment) => appointment.barberId === barberId
      );
    }

    // Filter by status
    if (status) {
      filteredAppointments = filteredAppointments.filter(
        (appointment) => appointment.status === status
      );
    }

    return HttpResponse.json({
      appointments: filteredAppointments,
    });
  }),
];