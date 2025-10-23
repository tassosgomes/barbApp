import React from 'react';
import { Clock, DollarSign } from 'lucide-react';
import type { PublicService } from '@/types/landing-page.types';

interface ServiceCardProps {
  service: PublicService;
  isSelected: boolean;
  onToggle: () => void;
}

export const ServiceCard: React.FC<ServiceCardProps> = ({
  service,
  isSelected,
  onToggle,
}) => {
  return (
    <div
      className={`border-2 rounded-lg p-4 cursor-pointer transition-all hover:shadow-md ${
        isSelected ? 'border-primary bg-primary/5' : 'border-gray-200'
      }`}
      onClick={onToggle}
    >
      <div className="flex items-start justify-between mb-3">
        <h3 className="text-lg font-semibold">{service.name}</h3>
        <input
          type="checkbox"
          checked={isSelected}
          onChange={(e) => {
            e.stopPropagation();
            onToggle();
          }}
          className="w-5 h-5 cursor-pointer"
        />
      </div>
      {service.description && (
        <p className="text-sm text-gray-600 mb-3">{service.description}</p>
      )}
      <div className="flex items-center gap-4 text-sm text-gray-500">
        <div className="flex items-center gap-1">
          <Clock size={16} />
          <span>{service.duration}min</span>
        </div>
        <div className="flex items-center gap-1 font-semibold text-primary">
          <DollarSign size={16} />
          <span>R$ {service.price.toFixed(2)}</span>
        </div>
      </div>
    </div>
  );
};