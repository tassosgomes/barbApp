// Barbershop types
export type {
	Barbershop,
	Address,
	CreateBarbershopRequest,
	UpdateBarbershopRequest,
	BarbershopFilters,
} from './barbershop';

// Auth types
export type {
	LoginRequest,
	LoginResponse,
	User,
} from './auth';

// Pagination types
export type {
	PaginatedResponse,
	ApiResponse,
} from './pagination';

// Barber types
export type {
	Barber,
	ServiceSummary,
	CreateBarberRequest,
	UpdateBarberRequest,
} from './barber';

// Service types
export type {
	BarbershopService,
	CreateServiceRequest,
	UpdateServiceRequest,
} from './service';

// Schedule types
export type {
	Appointment,
	AppointmentStatus,
} from './schedule';

// Filter types
export type {
	BarberFilters,
	ServiceFilters,
	ScheduleFilters,
} from './filters';

// Admin Barbearia types
export type {
	BarbeariaInfo,
	UseBarbeariaCodeReturn,
	AdminBarbeariaAuth,
	AdminBarbeariaSession,
} from './adminBarbearia';