// Using project's Select UI; no Button required here
import { Select, SelectTrigger, SelectContent, SelectItem, SelectValue } from '@/components/ui/select';
import { Building2, Check } from 'lucide-react';
import { useBarbershopContext } from '@/hooks/useBarbershopContext';

export function BarbershopSelector() {
  const { currentBarbershop, availableBarbershops, selectBarbershop } = useBarbershopContext();

  return (
    <Select defaultValue={currentBarbershop?.id} onValueChange={(value) => {
      const sel = availableBarbershops.find(b => b.id === value);
      if (sel) selectBarbershop(sel);
    }}>
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
