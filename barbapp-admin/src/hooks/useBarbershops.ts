import { useState, useEffect, useMemo } from 'react';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop, BarbershopFilters, PaginatedResponse } from '@/types';

export function useBarbershops(filters: BarbershopFilters) {
  const [data, setData] = useState<PaginatedResponse<Barbershop> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  // Memoize filters to prevent unnecessary re-renders
  const memoizedFilters = useMemo(() => filters, [
    filters.pageNumber,
    filters.pageSize,
    filters.searchTerm,
    filters.isActive,
  ]);

  useEffect(() => {
    let cancelled = false;

    async function fetchData() {
      try {
        setLoading(true);
        setError(null);
        const response = await barbershopService.getAll(memoizedFilters);
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
  }, [memoizedFilters]);

  return { data, loading, error };
}