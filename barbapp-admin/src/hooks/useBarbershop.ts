import { useState, useEffect } from 'react';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop } from '@/types';

export function useBarbershop(id: string | undefined) {
  const [data, setData] = useState<Barbershop | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    if (!id) return;

    async function fetchData() {
      try {
        setLoading(true);
        setError(null);
        const barbershop = await barbershopService.getById(id!);
        setData(barbershop);
      } catch (err) {
        setError(err as Error);
      } finally {
        setLoading(false);
      }
    }

    fetchData();
  }, [id]);

  return { data, loading, error };
}