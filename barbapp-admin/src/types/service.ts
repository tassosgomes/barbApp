export interface BarbershopService {
  id: string;
  name: string;
  description: string;
  durationMinutes: number;
  price: number;
  isActive: boolean;
}

export interface CreateServiceRequest {
  name: string;
  description: string;
  durationMinutes: number;
  price: number;
}

export interface UpdateServiceRequest {
  name: string;
  description: string;
  durationMinutes: number;
  price: number;
}