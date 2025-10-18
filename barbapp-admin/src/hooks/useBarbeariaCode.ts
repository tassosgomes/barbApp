import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { barbershopService } from '@/services/barbershop.service';
import { codigoSchema } from '@/schemas/adminBarbearia.schema';
import type { UseBarbeariaCodeReturn } from '@/types/adminBarbearia';

/**
 * Custom hook to extract and validate barbershop code from URL
 *
 * This hook:
 * - Extracts the `:codigo` parameter from the current route using useParams()
 * - Validates the code format locally using Zod schema (8 uppercase alphanumeric chars)
 * - Makes an API call to validate the code with the backend
 * - Returns loading states, validation results, and error handling
 *
 * @returns Object containing code, validation state, and barbershop info
 *
 * @example
 * ```tsx
 * function LoginAdminBarbearia() {
 *   const { codigo, barbeariaInfo, isLoading, error } = useBarbeariaCode();
 *
 *   if (isLoading) return <div>Validating code...</div>;
 *   if (error) return <div>Error: {error.message}</div>;
 *   if (!barbeariaInfo) return <div>Invalid code</div>;
 *
 *   return <div>Welcome to {barbeariaInfo.nome}</div>;
 * }
 * ```
 */
export function useBarbeariaCode(): UseBarbeariaCodeReturn {
  const { codigo } = useParams<{ codigo: string }>();

  // Local validation of code format
  const isValidFormat = codigo ? codigoSchema.safeParse(codigo).success : false;

  const {
    data: barbeariaInfo,
    isLoading,
    error,
    isFetching,
  } = useQuery({
    queryKey: ['barbearia-code', codigo],
    queryFn: () => barbershopService.validateCode(codigo!),
    enabled: !!codigo && isValidFormat, // Only run if code exists and has valid format
    retry: false, // Don't retry on 404/403 errors
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
    gcTime: 10 * 60 * 1000, // Garbage collection after 10 minutes
  });

  return {
    codigo,
    barbeariaInfo: barbeariaInfo ?? null,
    isLoading,
    error: error as Error | null,
    isValidating: isFetching,
  };
}