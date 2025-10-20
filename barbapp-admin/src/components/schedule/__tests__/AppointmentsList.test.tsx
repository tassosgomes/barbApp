import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AppointmentsList } from '../AppointmentsList';
import { AppointmentStatus, Appointment } from '@/types/appointment';

const mockAppointments: Appointment[] = [
  {
    id: 'apt-1',
    customerName: 'João Silva',
    serviceTitle: 'Corte de Cabelo',
    startTime: '2025-10-20T10:00:00Z',
    endTime: '2025-10-20T10:30:00Z',
    status: AppointmentStatus.Pending,
  },
  {
    id: 'apt-2',
    customerName: 'Maria Santos',
    serviceTitle: 'Barba',
    startTime: '2025-10-20T09:00:00Z',
    endTime: '2025-10-20T09:20:00Z',
    status: AppointmentStatus.Confirmed,
  },
  {
    id: 'apt-3',
    customerName: 'Pedro Costa',
    serviceTitle: 'Corte + Barba',
    startTime: '2025-10-20T11:00:00Z',
    endTime: '2025-10-20T11:45:00Z',
    status: AppointmentStatus.Completed,
  },
];

describe('AppointmentsList', () => {
  describe('Renderização de Lista', () => {
    it('deve renderizar todos os agendamentos', () => {
      render(<AppointmentsList appointments={mockAppointments} />);

      expect(screen.getByText('João Silva')).toBeInTheDocument();
      expect(screen.getByText('Maria Santos')).toBeInTheDocument();
      expect(screen.getByText('Pedro Costa')).toBeInTheDocument();
    });

    it('deve ordenar agendamentos por horário (cronologicamente)', () => {
      render(<AppointmentsList appointments={mockAppointments} />);

      const cards = screen.getAllByRole('article');
      
      // Primeiro deve ser Maria (09:00), depois João (10:00), depois Pedro (11:00)
      expect(cards[0]).toHaveTextContent('Maria Santos');
      expect(cards[1]).toHaveTextContent('João Silva');
      expect(cards[2]).toHaveTextContent('Pedro Costa');
    });

    it('deve passar callbacks para os cards', () => {
      const onConfirm = vi.fn();
      const onCancel = vi.fn();
      const onComplete = vi.fn();
      const onAppointmentClick = vi.fn();

      render(
        <AppointmentsList
          appointments={mockAppointments}
          onConfirm={onConfirm}
          onCancel={onCancel}
          onComplete={onComplete}
          onAppointmentClick={onAppointmentClick}
        />
      );

      // Verifica se os botões aparecem (indicando que callbacks foram passados)
      expect(screen.getByRole('button', { name: /confirmar/i })).toBeInTheDocument();
    });
  });

  describe('Estado de Loading', () => {
    it('deve exibir skeletons quando isLoading é true', () => {
      const { container } = render(<AppointmentsList isLoading={true} />);

      // Deve ter elementos skeleton
      const skeletons = container.querySelectorAll('.animate-pulse');
      expect(skeletons.length).toBeGreaterThan(0);
    });

    it('deve exibir número correto de skeletons', () => {
      const { container } = render(
        <AppointmentsList isLoading={true} loadingItemsCount={5} />
      );

      // Deve ter 5 cards skeleton
      const cards = container.querySelectorAll('.border-l-4');
      expect(cards).toHaveLength(5);
    });

    it('não deve exibir empty state quando está loading', () => {
      render(<AppointmentsList isLoading={true} />);

      expect(screen.queryByText(/nenhum agendamento/i)).not.toBeInTheDocument();
    });
  });

  describe('Empty State', () => {
    it('deve exibir mensagem quando não há agendamentos', () => {
      render(<AppointmentsList appointments={[]} />);

      expect(screen.getByText(/nenhum agendamento/i)).toBeInTheDocument();
      expect(
        screen.getByText(/não há agendamentos para este dia/i)
      ).toBeInTheDocument();
    });

    it('deve exibir empty state quando appointments é undefined', () => {
      render(<AppointmentsList />);

      expect(screen.getByText(/nenhum agendamento/i)).toBeInTheDocument();
    });

    it('deve exibir ícone de calendário no empty state', () => {
      render(<AppointmentsList appointments={[]} />);

      // Verifica se há um svg (ícone)
      const icon = screen.getByText(/nenhum agendamento/i)
        .closest('div')
        ?.querySelector('svg');
      expect(icon).toBeInTheDocument();
    });

    it('não deve exibir cards quando não há agendamentos', () => {
      const { container } = render(<AppointmentsList appointments={[]} />);

      // Não deve ter nenhum card
      const cards = container.querySelectorAll('[role="article"]');
      expect(cards).toHaveLength(0);
    });
  });

  describe('Casos Especiais', () => {
    it('deve lidar com agendamento único', () => {
      render(<AppointmentsList appointments={[mockAppointments[0]]} />);

      expect(screen.getByText('João Silva')).toBeInTheDocument();
      expect(screen.queryByText('Maria Santos')).not.toBeInTheDocument();
    });

    it('deve lidar com agendamentos no mesmo horário', () => {
      const sameTimeAppointments: Appointment[] = [
        {
          ...mockAppointments[0],
          id: 'apt-a',
          startTime: '2025-10-20T10:00:00Z',
        },
        {
          ...mockAppointments[1],
          id: 'apt-b',
          startTime: '2025-10-20T10:00:00Z',
        },
      ];

      render(<AppointmentsList appointments={sameTimeAppointments} />);

      expect(screen.getByText('João Silva')).toBeInTheDocument();
      expect(screen.getByText('Maria Santos')).toBeInTheDocument();
    });

    it('deve manter ordem correta mesmo com array desordenado', () => {
      const unorderedAppointments = [
        mockAppointments[2], // 11:00
        mockAppointments[0], // 10:00
        mockAppointments[1], // 09:00
      ];

      render(<AppointmentsList appointments={unorderedAppointments} />);

      const cards = screen.getAllByRole('article');
      
      // Deve ordenar: Maria (09:00), João (10:00), Pedro (11:00)
      expect(cards[0]).toHaveTextContent('Maria Santos');
      expect(cards[1]).toHaveTextContent('João Silva');
      expect(cards[2]).toHaveTextContent('Pedro Costa');
    });
  });

  describe('Responsividade', () => {
    it('deve usar space-y-4 para espaçamento vertical', () => {
      const { container } = render(<AppointmentsList appointments={mockAppointments} />);

      const listContainer = container.querySelector('.space-y-4');
      expect(listContainer).toBeInTheDocument();
    });
  });
});
