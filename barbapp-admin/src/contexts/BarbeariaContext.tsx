import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { barbeariaInfoSchema } from '@/schemas/adminBarbearia.schema';
import { TokenManager, UserType } from '@/services/tokenManager';

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
 * React Context for barbershop state management
 * IMPORTANTE: Usa TokenManager para gerenciar contexto
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
 * IMPORTANTE: Usa TokenManager para evitar conflitos
 */
export function BarbeariaProvider({ children }: BarbeariaProviderProps) {
  const [barbearia, setBarbeariaState] = useState<BarbeariaContextData | null>(null);
  const [isLoaded, setIsLoaded] = useState(false);

  /**
   * Load barbershop data from localStorage on component mount
   * Validates data structure using Zod schema
   * Usa TokenManager para obter contexto
   */
  useEffect(() => {
    try {
      const stored = TokenManager.getContext<{
        id: string;
        nome: string;
        codigo: string;
        isActive: boolean;
      }>(UserType.ADMIN_BARBEARIA);
      
      if (stored) {
        // Validate structure with Zod
        const validated = barbeariaInfoSchema.parse(stored);
        setBarbeariaState({
          barbeariaId: validated.id,
          nome: validated.nome,
          codigo: validated.codigo,
          isActive: validated.isActive,
        });
      }
    } catch (error) {
      console.error('Erro ao carregar contexto da barbearia:', error);
      TokenManager.removeContext(UserType.ADMIN_BARBEARIA);
    } finally {
      setIsLoaded(true);
    }
  }, []);

  /**
   * Synchronize state changes across browser tabs
   * Listens for storage events and updates local state accordingly
   * NOTA: TokenManager não expõe a chave diretamente, então mantemos referência local
   */
  useEffect(() => {
    const STORAGE_KEY = 'admin_barbearia_context'; // Key used by TokenManager
    
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
   * Usa TokenManager para gerenciar contexto
   * 
   * @param data - Barbershop context data
   */
  const setBarbearia = (data: BarbeariaContextData) => {
    setBarbeariaState(data);
    TokenManager.setContext(UserType.ADMIN_BARBEARIA, {
      id: data.barbeariaId,
      nome: data.nome,
      codigo: data.codigo,
      isActive: data.isActive,
    });
  };

  /**
   * Clear barbershop data from context and localStorage
   * Usa TokenManager para limpeza completa
   */
  const clearBarbearia = () => {
    setBarbeariaState(null);
    TokenManager.removeContext(UserType.ADMIN_BARBEARIA);
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