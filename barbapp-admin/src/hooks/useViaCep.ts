import { useState, useCallback } from 'react';
import { fetchAddressByCep, AddressData, ViaCepError } from '@/services/viacep.service';

interface UseViaCepReturn {
  searchCep: (cep: string) => Promise<void>;
  loading: boolean;
  error: string | null;
  data: AddressData | null;
  clearError: () => void;
  clearData: () => void;
}

/**
 * Custom hook for integrating with ViaCEP API
 * Manages loading, error, and data states for address lookup
 * 
 * @example
 * const { searchCep, loading, error, data } = useViaCep();
 * 
 * // Search for address by CEP
 * await searchCep('01310-100');
 * 
 * // Check results
 * if (data) {
 *   console.log(data.street, data.city);
 * }
 */
export function useViaCep(): UseViaCepReturn {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [data, setData] = useState<AddressData | null>(null);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const clearData = useCallback(() => {
    setData(null);
  }, []);

  const searchCep = useCallback(async (cep: string) => {
    if (!cep || cep.replace(/\D/g, '').length !== 8) {
      return;
    }

    setLoading(true);
    setError(null);
    setData(null);

    try {
      const addressData = await fetchAddressByCep(cep);
      setData(addressData);
    } catch (err) {
      if (err instanceof ViaCepError) {
        setError(err.message);
      } else {
        setError('Erro ao buscar CEP. Tente novamente.');
      }
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    searchCep,
    loading,
    error,
    data,
    clearError,
    clearData,
  };
}
