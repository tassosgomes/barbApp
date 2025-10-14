import { useState, useEffect, useCallback } from 'react';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop, BarbershopFilters, PaginatedResponse } from '@/types';

export function useBarbershops(filters: BarbershopFilters) {
  const [data, setData] = useState<PaginatedResponse<Barbershop> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await barbershopService.getAll(filters);
      setData(response);
    } catch (err) {
      setError(err as Error);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return { data, loading, error, refetch: fetchData };
}