import { useQuery } from '@tanstack/react-query';
import axios from 'axios';
import type { PublicLandingPage } from '@/types/landing-page.types';

const API_URL = import.meta.env.VITE_API_URL;

export const useLandingPageData = (code: string) => {
  return useQuery<PublicLandingPage>({
    queryKey: ['publicLandingPage', code],
    queryFn: async () => {
      const { data } = await axios.get(
        `${API_URL}/public/barbershops/${code}/landing-page`
      );
      return data;
    },
    staleTime: 5 * 60 * 1000, // Cache 5 minutos
    retry: 1,
  });
};