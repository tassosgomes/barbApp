import { useState, useEffect } from 'react';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop, BarbershopFilters, PaginatedResponse } from '@/types';

export function useBarbershops(filters: BarbershopFilters) {
  const [data, setData] = useState<PaginatedResponse<Barbershop> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function fetchData() {
      try {
        setLoading(true);
        setError(null);
        const response = await barbershopService.getAll(filters);
        if (!cancelled) {
          setData(response);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err as Error);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    fetchData();

    return () => {
      cancelled = true;
    };
  }, [filters]);

  return { data, loading, error };
}