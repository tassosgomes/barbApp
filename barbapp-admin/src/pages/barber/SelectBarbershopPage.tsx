import { useBarbershopContext } from '@/hooks/useBarbershopContext';
import { useNavigate } from 'react-router-dom';

export function SelectBarbershopPage() {
  const { availableBarbershops, selectBarbershop } = useBarbershopContext();
  const navigate = useNavigate();

  const handleSelect = (barbershop: any) => {
    selectBarbershop(barbershop);
    navigate('/barber/schedule');
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4">
      <h1 className="text-2xl font-bold mb-6">Selecione a Barbearia</h1>
      <div className="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
        {availableBarbershops.map((barbershop) => (
          <button
            key={barbershop.id}
            className="bg-white shadow rounded-lg p-6 flex flex-col items-center hover:bg-gray-100"
            onClick={() => handleSelect(barbershop)}
          >
            <span className="text-lg font-semibold">{barbershop.name}</span>
          </button>
        ))}
      </div>
    </div>
  );
}
