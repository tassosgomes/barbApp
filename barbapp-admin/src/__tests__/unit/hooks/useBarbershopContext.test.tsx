/// <reference types="vitest" />
import { render, fireEvent, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useBarbershopContext } from '@/hooks/useBarbershopContext';
import { vi } from 'vitest';

vi.mock('@/services/auth.service', () => ({
  authService: {
    trocarContexto: vi.fn().mockResolvedValue({ token: 'new-token' }),
  },
}));

vi.mock('@/hooks/useBarbershops', () => ({
  useBarbershops: () => ({ data: { items: [] } }),
}));

vi.mock('@/services/tokenManager', () => ({
  TokenManager: {
    setToken: vi.fn(),
  },
  UserType: { BARBEIRO: 'barbeiro' },
}));

import { authService } from '@/services/auth.service';
import { TokenManager } from '@/services/tokenManager';

const qc = new QueryClient();

function TestComponent() {
  const { selectBarbershop } = useBarbershopContext();
  return (
    <button onClick={() => selectBarbershop({ id: '1', name: 'Test' })} data-testid="select-btn">
      Select
    </button>
  );
}

describe('useBarbershopContext', () => {
  it('should call trocarContexto and set token when selecting a barbershop', async () => {
    render(
      <QueryClientProvider client={qc}>
        <TestComponent />
      </QueryClientProvider>
    );

    const btn = screen.getByTestId('select-btn');
    await fireEvent.click(btn);

    await waitFor(() => {
      expect(authService.trocarContexto).toHaveBeenCalledWith('1');
      expect(TokenManager.setToken).toHaveBeenCalled();
    });
  });
});
