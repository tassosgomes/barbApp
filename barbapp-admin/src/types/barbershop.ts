/**
 * Barbershop entity representing a registered barbershop in the system
 */
export interface Barbershop {
  id: string;
  name: string;
  document: string;
  phone: string;
  ownerName: string;
  email: string;
  code: string;
  isActive: boolean;
  address: Address;
  createdAt: string; // ISO 8601 date string
  updatedAt: string; // ISO 8601 date string
}

/**
 * Address information for barbershops
 */
export interface Address {
  zipCode: string; // Format: "99999-999"
  street: string;
  number: string;
  complement?: string; // Optional field
  neighborhood: string;
  city: string;
  state: string; // 2-letter state code (e.g., "SP")
}

/**
 * Request payload for creating a new barbershop
 */
export interface CreateBarbershopRequest {
  name: string;
  document: string;
  phone: string;
  ownerName: string;
  email: string;
  zipCode: string;
  street: string;
  number: string;
  complement?: string;
  neighborhood: string;
  city: string;
  state: string;
}

/**
 * Request payload for updating an existing barbershop
 * Same as CreateBarbershopRequest plus Id field
 * Kept as separate interface for future extensibility
 */
export interface UpdateBarbershopRequest {
  id: string;
  name: string;
  phone: string;
  ownerName: string;
  email: string;
  zipCode: string;
  street: string;
  number: string;
  complement?: string;
  neighborhood: string;
  city: string;
  state: string;
}

/**
 * Query parameters for filtering barbershops
 */
export interface BarbershopFilters {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string; // Search by name, email, or city
  isActive?: boolean; // Filter by status
}
