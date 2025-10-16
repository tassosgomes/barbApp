export interface Barber {
  id: string;
  name: string;
  email: string;
  phoneFormatted: string;
  services: ServiceSummary[];
  isActive: boolean;
  createdAt: string;
}

export interface ServiceSummary {
  id: string;
  name: string;
}

export interface CreateBarberRequest {
  name: string;
  email: string;
  password: string;
  phone: string;
  serviceIds: string[];
}

export interface UpdateBarberRequest {
  name: string;
  phone: string;
  serviceIds: string[];
}