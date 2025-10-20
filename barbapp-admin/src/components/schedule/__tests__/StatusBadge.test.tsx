import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { StatusBadge } from '../StatusBadge';
import { AppointmentStatus } from '@/types/appointment';

describe('StatusBadge', () => {
  it('deve renderizar badge de status Pendente', () => {
    render(<StatusBadge status={AppointmentStatus.Pending} />);
    
    expect(screen.getByText('Pendente')).toBeInTheDocument();
  });

  it('deve renderizar badge de status Confirmado', () => {
    render(<StatusBadge status={AppointmentStatus.Confirmed} />);
    
    expect(screen.getByText('Confirmado')).toBeInTheDocument();
  });

  it('deve renderizar badge de status Concluído', () => {
    render(<StatusBadge status={AppointmentStatus.Completed} />);
    
    expect(screen.getByText('Concluído')).toBeInTheDocument();
  });

  it('deve renderizar badge de status Cancelado', () => {
    render(<StatusBadge status={AppointmentStatus.Cancelled} />);
    
    expect(screen.getByText('Cancelado')).toBeInTheDocument();
  });

  it('deve aplicar className personalizado', () => {
    const { container } = render(
      <StatusBadge status={AppointmentStatus.Pending} className="custom-class" />
    );
    
    const badge = container.querySelector('.custom-class');
    expect(badge).toBeInTheDocument();
  });

  it('deve ter cores diferentes para cada status', () => {
    const { rerender, container } = render(
      <StatusBadge status={AppointmentStatus.Pending} />
    );
    
    // Pending - amarelo
    expect(container.querySelector('.bg-yellow-100')).toBeInTheDocument();
    
    // Confirmed - verde
    rerender(<StatusBadge status={AppointmentStatus.Confirmed} />);
    expect(container.querySelector('.bg-green-100')).toBeInTheDocument();
    
    // Completed - cinza
    rerender(<StatusBadge status={AppointmentStatus.Completed} />);
    expect(container.querySelector('.bg-gray-100')).toBeInTheDocument();
    
    // Cancelled - vermelho
    rerender(<StatusBadge status={AppointmentStatus.Cancelled} />);
    expect(container.querySelector('.bg-red-100')).toBeInTheDocument();
  });

  it('deve renderizar ícone apropriado para cada status', () => {
    const { rerender } = render(<StatusBadge status={AppointmentStatus.Pending} />);
    
    // Todos os status devem ter um ícone
    expect(screen.getByText('Pendente').previousElementSibling).toBeInTheDocument();
    
    rerender(<StatusBadge status={AppointmentStatus.Confirmed} />);
    expect(screen.getByText('Confirmado').previousElementSibling).toBeInTheDocument();
    
    rerender(<StatusBadge status={AppointmentStatus.Completed} />);
    expect(screen.getByText('Concluído').previousElementSibling).toBeInTheDocument();
    
    rerender(<StatusBadge status={AppointmentStatus.Cancelled} />);
    expect(screen.getByText('Cancelado').previousElementSibling).toBeInTheDocument();
  });
});
