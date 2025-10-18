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

// Barbeiro types (aliases from Task 9.0)
export type {
	Barbeiro,
	CreateBarbeiroInput,
	UpdateBarbeiroInput,
	ListBarbeirosParams,
} from './barbeiro';

// Service types
export type {
	BarbershopService,
	CreateServiceRequest,
	UpdateServiceRequest,
} from './service';

// Servico types (aliases from Task 11.0)
export type {
	Servico,
	CreateServicoInput,
	UpdateServicoInput,
	ListServicosParams,
	ServicoSummary,
	ServicoFilters,
} from './servico';

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