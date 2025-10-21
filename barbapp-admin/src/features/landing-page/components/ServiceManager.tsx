import React, { useState, useEffect } from 'react';
import { DragDropContext, Droppable, Draggable, DropResult } from '@hello-pangea/dnd';
import { GripVertical, Eye, EyeOff } from 'lucide-react';
import { Checkbox } from '@/components/ui/checkbox';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { LandingPageService } from '../types/landing-page.types';
import { ServiceManagerProps } from '../types/landing-page.types';

export const ServiceManager: React.FC<ServiceManagerProps> = ({
  services,
  onChange,
  disabled = false,
}) => {
  // Estado local para gerenciar a lista de serviços
  const [localServices, setLocalServices] = useState<LandingPageService[]>(services);
  const [selectedServices, setSelectedServices] = useState<Set<string>>(new Set());

  // Sincronizar estado local quando props mudam
  useEffect(() => {
    setLocalServices(services);
  }, [services]);

  // Handler para drag and drop
  const handleDragEnd = (result: DropResult) => {
    if (!result.destination || disabled) return;

    const items = Array.from(localServices);
    const [reorderedItem] = items.splice(result.source.index, 1);
    items.splice(result.destination.index, 0, reorderedItem);

    // Atualizar displayOrder baseado na nova posição
    const updatedServices = items.map((service, index) => ({
      ...service,
      displayOrder: index + 1,
    }));

    setLocalServices(updatedServices);
    onChange(updatedServices);
  };

  // Handler para alternar visibilidade de um serviço
  const toggleVisibility = (serviceId: string) => {
    if (disabled) return;

    const updatedServices = localServices.map(service =>
      service.serviceId === serviceId
        ? { ...service, isVisible: !service.isVisible }
        : service
    );

    setLocalServices(updatedServices);
    onChange(updatedServices);
  };

  // Handler para selecionar todos os serviços
  const selectAll = () => {
    const allIds = new Set(localServices.map(service => service.serviceId));
    setSelectedServices(allIds);
  };

  // Handler para desmarcar todos os serviços
  const deselectAll = () => {
    setSelectedServices(new Set());
  };

  // Handler para alternar seleção de um serviço
  const toggleServiceSelection = (serviceId: string) => {
    const newSelected = new Set(selectedServices);
    if (newSelected.has(serviceId)) {
      newSelected.delete(serviceId);
    } else {
      newSelected.add(serviceId);
    }
    setSelectedServices(newSelected);
  };

  // Handler para ações em lote nos serviços selecionados
  const handleBulkAction = (action: 'show' | 'hide') => {
    if (selectedServices.size === 0) return;

    const updatedServices = localServices.map(service => {
      if (selectedServices.has(service.serviceId)) {
        return {
          ...service,
          isVisible: action === 'show',
        };
      }
      return service;
    });

    setLocalServices(updatedServices);
    onChange(updatedServices);
    setSelectedServices(new Set());
  };

  // Formatar preço
  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(price);
  };

  // Formatar duração
  const formatDuration = (minutes: number) => {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;

    if (hours > 0) {
      return `${hours}h${mins > 0 ? ` ${mins}min` : ''}`;
    }
    return `${mins}min`;
  };

  return (
    <div className="space-y-4">
      {/* Header com controles de seleção */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <h3 className="text-lg font-semibold">Serviços da Landing Page</h3>
          <span className="text-sm text-muted-foreground">
            {localServices.length} serviço{localServices.length !== 1 ? 's' : ''}
          </span>
        </div>

        {selectedServices.size > 0 && (
          <div className="flex items-center space-x-2">
            <span className="text-sm text-muted-foreground">
              {selectedServices.size} selecionado{selectedServices.size !== 1 ? 's' : ''}
            </span>
            <Button
              variant="outline"
              size="sm"
              onClick={() => handleBulkAction('show')}
              disabled={disabled}
            >
              <Eye className="w-4 h-4 mr-1" />
              Mostrar
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => handleBulkAction('hide')}
              disabled={disabled}
            >
              <EyeOff className="w-4 h-4 mr-1" />
              Ocultar
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={deselectAll}
            >
              Limpar seleção
            </Button>
          </div>
        )}

        {selectedServices.size === 0 && (
          <div className="flex items-center space-x-2">
            <Button
              variant="outline"
              size="sm"
              onClick={selectAll}
              disabled={disabled || localServices.length === 0}
            >
              Selecionar todos
            </Button>
          </div>
        )}
      </div>

      {/* Lista de serviços com drag and drop */}
      <DragDropContext onDragEnd={handleDragEnd}>
        <Droppable droppableId="services">
          {(provided) => (
            <div
              {...provided.droppableProps}
              ref={provided.innerRef}
              className="space-y-2"
            >
              {localServices.map((service, index) => (
                <Draggable
                  key={service.serviceId}
                  draggableId={service.serviceId}
                  index={index}
                  isDragDisabled={disabled}
                >
                  {(provided, snapshot) => (
                    <Card
                      ref={provided.innerRef}
                      {...provided.draggableProps}
                      className={`p-4 transition-shadow ${
                        snapshot.isDragging ? 'shadow-lg' : ''
                      } ${disabled ? 'opacity-50' : ''}`}
                    >
                      <div className="flex items-center space-x-4">
                        {/* Handle de drag */}
                        <div
                          {...provided.dragHandleProps}
                          className={`cursor-grab active:cursor-grabbing ${
                            disabled ? 'cursor-not-allowed' : ''
                          }`}
                        >
                          <GripVertical className="w-5 h-5 text-muted-foreground" />
                        </div>

                        {/* Checkbox de seleção */}
                        <Checkbox
                          checked={selectedServices.has(service.serviceId)}
                          onCheckedChange={() => toggleServiceSelection(service.serviceId)}
                          disabled={disabled}
                        />

                        {/* Informações do serviço */}
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center space-x-2">
                            <h4 className="font-medium truncate">{service.serviceName}</h4>
                            <span className="text-sm text-muted-foreground">
                              {formatPrice(service.price)}
                            </span>
                            <span className="text-sm text-muted-foreground">
                              • {formatDuration(service.duration)}
                            </span>
                          </div>
                          {service.description && (
                            <p className="text-sm text-muted-foreground mt-1 truncate">
                              {service.description}
                            </p>
                          )}
                        </div>

                        {/* Controles de visibilidade */}
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => toggleVisibility(service.serviceId)}
                          disabled={disabled}
                          className="shrink-0"
                        >
                          {service.isVisible ? (
                            <Eye className="w-4 h-4" />
                          ) : (
                            <EyeOff className="w-4 h-4" />
                          )}
                        </Button>
                      </div>
                    </Card>
                  )}
                </Draggable>
              ))}
              {provided.placeholder}
            </div>
          )}
        </Droppable>
      </DragDropContext>

      {/* Mensagem quando não há serviços */}
      {localServices.length === 0 && (
        <Card className="p-8 text-center">
          <p className="text-muted-foreground">
            Nenhum serviço configurado para esta landing page.
          </p>
        </Card>
      )}
    </div>
  );
};