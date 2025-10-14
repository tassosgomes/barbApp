/**
 * ViaCEP API Integration Service
 * Provides address lookup functionality using Brazilian postal codes (CEP)
 * @see https://viacep.com.br/
 */

export interface ViaCepResponse {
  cep: string;
  logradouro: string;
  complemento: string;
  bairro: string;
  localidade: string;
  uf: string;
  ibge: string;
  gia: string;
  ddd: string;
  siafi: string;
  erro?: boolean;
}

export interface AddressData {
  street: string;
  neighborhood: string;
  city: string;
  state: string;
  cep: string;
}

export class ViaCepError extends Error {
  constructor(message: string, public code: 'INVALID_CEP' | 'NOT_FOUND' | 'NETWORK_ERROR') {
    super(message);
    this.name = 'ViaCepError';
  }
}

/**
 * Removes non-numeric characters from CEP string
 */
function sanitizeCep(cep: string): string {
  return cep.replace(/\D/g, '');
}

/**
 * Validates CEP format (must be 8 digits)
 */
function isValidCep(cep: string): boolean {
  const sanitized = sanitizeCep(cep);
  return /^\d{8}$/.test(sanitized);
}

/**
 * Fetches address data from ViaCEP API using postal code
 * @param cep - Brazilian postal code (with or without mask)
 * @returns Promise with address data
 * @throws ViaCepError if CEP is invalid, not found, or network error occurs
 */
export async function fetchAddressByCep(cep: string): Promise<AddressData> {
  const sanitized = sanitizeCep(cep);

  if (!isValidCep(sanitized)) {
    throw new ViaCepError('CEP inválido. Deve conter 8 dígitos.', 'INVALID_CEP');
  }

  try {
    const response = await fetch(`https://viacep.com.br/ws/${sanitized}/json/`);

    if (!response.ok) {
      throw new ViaCepError('Erro ao buscar CEP. Tente novamente.', 'NETWORK_ERROR');
    }

    const data: ViaCepResponse = await response.json();

    if (data.erro) {
      throw new ViaCepError('CEP não encontrado.', 'NOT_FOUND');
    }

    return {
      street: data.logradouro,
      neighborhood: data.bairro,
      city: data.localidade,
      state: data.uf,
      cep: data.cep,
    };
  } catch (error) {
    if (error instanceof ViaCepError) {
      throw error;
    }

    throw new ViaCepError('Erro ao buscar CEP. Verifique sua conexão.', 'NETWORK_ERROR');
  }
}
