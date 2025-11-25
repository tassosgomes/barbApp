/// <reference types="vitest" />
import { render, screen, fireEvent } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { SelectBarbershopPage } from '@/pages/barber/SelectBarbershopPage';
import { vi } from 'vitest';

vi.mock('@/hooks/useBarbershops', () => ({
  useBarbershops: () => ({ data: { items: [ { id: '1', name: 'A' }, { id: '2', name: 'B' } ] } }),
}));

vi.mock('@/hooks/useBarbershopContext', () => ({
  useBarbershopContext: () => ({
    availableBarbershops: [ { id: '1', name: 'A' }, { id: '2', name: 'B' } ],
    selectBarbershop: vi.fn(),
  }),
}));

describe('SelectBarbershopPage', () => {
  it('renders list and navigates after selection', async () => {
    const { container } = render(
      <MemoryRouter initialEntries={["/barber/select-barbershop"]}>
        <Routes>
          <Route path="/barber/select-barbershop" element={<SelectBarbershopPage />} />
          <Route path="/barber/schedule" element={<div>Schedule</div>} />
        </Routes>
      </MemoryRouter>
    );

    const buttons = await screen.findAllByRole('button', { name: /selecionar barbearia/i });
    expect(buttons.length).toBeGreaterThan(0);

    // Click the first barbershop
    fireEvent.click(buttons[0]);

    // After clicking, navigation should go to /barber/schedule and render 'Schedule'
    expect(await screen.findByText('Schedule')).toBeInTheDocument();
  });
});
