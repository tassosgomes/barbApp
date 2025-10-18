/**
 * Types for Admin Barbearia functionality
 */

/**
 * Basic information returned when validating a barbershop code
 * This is the minimal data needed for login validation
 */
export interface BarbeariaInfo {
  id: string;
  nome: string;
  codigo: string;
  isActive: boolean;
}

/**
 * Return type for useBarbeariaCode hook
 */
export interface UseBarbeariaCodeReturn {
  codigo: string | undefined;
  barbeariaInfo: BarbeariaInfo | null;
  isLoading: boolean;
  error: Error | null;
  isValidating: boolean;
}

/**
 * Admin Barbearia authentication data
 */
export interface AdminBarbeariaAuth {
  token: string;
  tipoUsuario: 'AdminBarbearia';
  barbeariaId: string;
  nomeBarbearia: string;
  codigo: string;
  expiresAt: string;
}

/**
 * Admin Barbearia session stored in localStorage
 */
export interface AdminBarbeariaSession {
  token: string;
  barbeariaId: string;
  nomeBarbearia: string;
  codigo: string;
}