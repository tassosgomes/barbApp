import { useState, useMemo } from 'react';
import type { PublicService } from '@/types/landing-page.types';

export const useServiceSelection = (services: PublicService[]) => {
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  const toggleService = (serviceId: string) => {
    setSelectedIds((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(serviceId)) {
        newSet.delete(serviceId);
      } else {
        newSet.add(serviceId);
      }
      return newSet;
    });
  };

  const selectedServices = useMemo(
    () => services.filter((s) => selectedIds.has(s.id)),
    [services, selectedIds]
  );

  const totalPrice = useMemo(
    () => selectedServices.reduce((sum, s) => sum + s.price, 0),
    [selectedServices]
  );

  const totalDuration = useMemo(
    () => selectedServices.reduce((sum, s) => sum + s.durationMinutes, 0),
    [selectedServices]
  );

  return {
    selectedIds,
    selectedServices,
    totalPrice,
    totalDuration,
    toggleService,
    hasSelection: selectedIds.size > 0,
  };
};