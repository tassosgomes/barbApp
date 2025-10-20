import { useEffect, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { authService } from '@/services/auth.service';
import { TokenManager, UserType } from '@/services/tokenManager';
import { barbershopService } from '@/services/barbershop.service';

export interface BarbershopContext {
	id: string;
	name: string;
}

export function useBarbershopContext() {
	const [context, setContext] = useState<BarbershopContext | null>(() => {
		const stored = sessionStorage.getItem('barbershop-context');
		return stored ? JSON.parse(stored) : null;
	});
	const queryClient = useQueryClient();
	
	// Buscar barbearias do barbeiro usando a API correta
	const { data: barbershopsData } = useQuery({
		queryKey: ['my-barbershops'],
		queryFn: barbershopService.getMyBarbershops,
		staleTime: 1000 * 60 * 5, // 5 minutos
	});

	const availableBarbershops: BarbershopContext[] = (barbershopsData || []).map((b) => ({
		id: b.id,
		name: b.nome,
	}));

		const selectBarbershop = async (barbershop: BarbershopContext) => {
			try {
				// Trocar contexto no backend para obter um token com o novo barbeariaId
				const response = await authService.trocarContexto(barbershop.id);
				TokenManager.setToken(UserType.BARBEIRO, response.token);
				setContext(barbershop);
				sessionStorage.setItem('barbershop-context', JSON.stringify(barbershop));
				// Invalidate queries to refresh data under new context
				queryClient.invalidateQueries();
			} catch (err) {
				// Fallback: setar apenas no frontend (endereço offline ou erro)
				console.error('Erro ao trocar contexto no backend, aplicando somente no frontend', err);
				setContext(barbershop);
				sessionStorage.setItem('barbershop-context', JSON.stringify(barbershop));
				queryClient.invalidateQueries();
			}
		};


		// Caso não haja contexto e exista apenas 1 barbearia, selecionar automaticamente
		useEffect(() => {
			if (!context && availableBarbershops.length === 1) {
				void selectBarbershop(availableBarbershops[0]);
			}
		}, [context, availableBarbershops]);

		return {
			currentBarbershop: context,
			selectBarbershop,
			isSelected: !!context,
			availableBarbershops,
		};
}