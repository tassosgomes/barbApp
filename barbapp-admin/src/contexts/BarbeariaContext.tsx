import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { barbeariaInfoSchema } from '@/schemas/adminBarbearia.schema';

/**
 * Data structure for barbershop information stored in context
 */
export interface BarbeariaContextData {
  barbeariaId: string;
  nome: string;
  codigo: string;
  isActive: boolean;
}

/**
 * Context value interface with state and actions
 */
export interface BarbeariaContextValue {
  barbearia: BarbeariaContextData | null;
  setBarbearia: (data: BarbeariaContextData) => void;
  clearBarbearia: () => void;
  isLoaded: boolean;
}

/**
 * Local storage key for persisting barbershop context
 */
const STORAGE_KEY = 'admin_barbearia_context';

/**
 * React Context for barbershop state management
 */
const BarbeariaContext = createContext<BarbeariaContextValue | undefined>(undefined);

/**
 * Props for BarbeariaProvider component
 */
interface BarbeariaProviderProps {
  children: ReactNode;
}

/**
 * Provider component that manages barbershop context state
 * Handles persistence to localStorage and cross-tab synchronization
 */
export function BarbeariaProvider({ children }: BarbeariaProviderProps) {
  const [barbearia, setBarbeariaState] = useState<BarbeariaContextData | null>(null);
  const [isLoaded, setIsLoaded] = useState(false);

  /**
   * Load barbershop data from localStorage on component mount
   * Validates data structure using Zod schema
   */
  useEffect(() => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        const parsed = JSON.parse(stored);
        // Validate structure with Zod
        const validated = barbeariaInfoSchema.parse(parsed);
        setBarbeariaState({
          barbeariaId: validated.id,
          nome: validated.nome,
          codigo: validated.codigo,
          isActive: validated.isActive,
        });
      }
    } catch (error) {
      console.error('Erro ao carregar contexto da barbearia:', error);
      localStorage.removeItem(STORAGE_KEY);
    } finally {
      setIsLoaded(true);
    }
  }, []);

  /**
   * Synchronize state changes across browser tabs
   * Listens for storage events and updates local state accordingly
   */
  useEffect(() => {
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === STORAGE_KEY) {
        if (e.newValue) {
          try {
            const parsed = JSON.parse(e.newValue);
            const validated = barbeariaInfoSchema.parse(parsed);
            setBarbeariaState({
              barbeariaId: validated.id,
              nome: validated.nome,
              codigo: validated.codigo,
              isActive: validated.isActive,
            });
          } catch (error) {
            console.error('Erro ao sincronizar contexto:', error);
          }
        } else {
          setBarbeariaState(null);
        }
      }
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  /**
   * Set barbershop data in context and persist to localStorage
   * @param data - Barbershop context data
   */
  const setBarbearia = (data: BarbeariaContextData) => {
    setBarbeariaState(data);
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({
        id: data.barbeariaId,
        nome: data.nome,
        codigo: data.codigo,
        isActive: data.isActive,
      })
    );
  };

  /**
   * Clear barbershop data from context and localStorage
   */
  const clearBarbearia = () => {
    setBarbeariaState(null);
    localStorage.removeItem(STORAGE_KEY);
  };

  return (
    <BarbeariaContext.Provider
      value={{
        barbearia,
        setBarbearia,
        clearBarbearia,
        isLoaded,
      }}
    >
      {children}
    </BarbeariaContext.Provider>
  );
}

/**
 * Custom hook to consume BarbeariaContext
 * Must be used within BarbeariaProvider
 *
 * @returns BarbeariaContextValue
 * @throws Error if used outside of BarbeariaProvider
 *
 * @example
 * ```tsx
 * function MyComponent() {
 *   const { barbearia, setBarbearia, clearBarbearia } = useBarbearia();
 *
 *   return (
 *     <div>
 *       {barbearia ? (
 *         <h1>{barbearia.nome}</h1>
 *       ) : (
 *         <p>Nenhuma barbearia selecionada</p>
 *       )}
 *     </div>
 *   );
 * }
 * ```
 */
export function useBarbearia(): BarbeariaContextValue {
  const context = useContext(BarbeariaContext);

  if (context === undefined) {
    throw new Error('useBarbearia must be used within BarbeariaProvider');
  }

  return context;
}