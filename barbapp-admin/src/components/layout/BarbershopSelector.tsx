import { Select, SelectTrigger, SelectContent, SelectItem, SelectValue } from '@/components/ui/select';
import { Building2, Check } from 'lucide-react';
import { useBarbershopContext } from '@/hooks/useBarbershopContext';

/**
 * Seletor de contexto de barbearia para barbeiros
 * 
 * Permite que barbeiros que trabalham em múltiplas barbearias troquem
 * facilmente entre diferentes estabelecimentos. Ao trocar de contexto,
 * todas as queries são invalidadas para refletir os dados da nova barbearia.
 * 
 * @example
 * ```tsx
 * // No Header para barbeiros
 * {user.role === 'Barbeiro' && <BarbershopSelector />}
 * ```
 */
export function BarbershopSelector() {
  const { currentBarbershop, availableBarbershops, selectBarbershop } = useBarbershopContext();

  return (
    <Select 
      value={currentBarbershop?.id} 
      onValueChange={(value) => {
        const sel = availableBarbershops.find(b => b.id === value);
        if (sel) {
          void selectBarbershop(sel);
        }
      }}
    >
      <SelectTrigger className="min-w-[200px]">
        <Building2 className="mr-2 h-4 w-4" />
        <SelectValue placeholder={currentBarbershop?.name || 'Selecione a barbearia'} />
      </SelectTrigger>
      <SelectContent>
        {availableBarbershops.map((barbershop) => (
          <SelectItem key={barbershop.id} value={barbershop.id}>
            <div className="flex items-center w-full">
              <span className="truncate">{barbershop.name}</span>
              {currentBarbershop?.id === barbershop.id && (
                <Check className="ml-auto h-4 w-4" />
              )}
            </div>
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
