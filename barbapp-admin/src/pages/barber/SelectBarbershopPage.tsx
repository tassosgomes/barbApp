import { useBarbershopContext } from '@/hooks/useBarbershopContext';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent } from '@/components/ui/card';
import { Building2 } from 'lucide-react';

/**
 * Página de seleção de barbearia
 * 
 * Exibida quando barbeiro trabalha em múltiplas barbearias.
 * Permite escolher em qual estabelecimento deseja trabalhar.
 * 
 * @route /barber/select-barbershop
 */
export function SelectBarbershopPage() {
  const { availableBarbershops, selectBarbershop } = useBarbershopContext();
  const navigate = useNavigate();

  const handleSelect = (barbershop: { id: string; name: string }) => {
    void selectBarbershop(barbershop);
    navigate('/barber/schedule');
  };

  if (availableBarbershops.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen p-4">
        <div className="text-center">
          <Building2 className="mx-auto h-12 w-12 text-gray-400 mb-4" />
          <h1 className="text-2xl font-bold mb-2">Nenhuma barbearia encontrada</h1>
          <p className="text-muted-foreground">
            Você não está vinculado a nenhuma barbearia no momento.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4 bg-gray-50">
      <div className="w-full max-w-4xl">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold mb-2">Selecione a Barbearia</h1>
          <p className="text-muted-foreground">
            Em qual barbearia deseja trabalhar hoje?
          </p>
        </div>
        <div className="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
          {availableBarbershops.map((barbershop) => (
            <Card
              key={barbershop.id}
              className="hover:shadow-lg transition-shadow"
            >
              <button
                type="button"
                className="w-full cursor-pointer rounded-lg text-left focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
                onClick={() => handleSelect(barbershop)}
                aria-label={`Selecionar barbearia ${barbershop.name}`}
              >
                <CardContent className="flex flex-col items-center justify-center p-6 min-h-[150px]">
                  <Building2 className="h-10 w-10 text-primary mb-3" />
                  <span className="text-lg font-semibold text-center">{barbershop.name}</span>
                </CardContent>
              </button>
            </Card>
          ))}
        </div>
      </div>
    </div>
  );
}
