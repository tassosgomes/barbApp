/**
 * LandingPageForm Component
 * 
 * Formulário principal no painel de administração para personalizar a landing page.
 * Integra LogoUploader e ServiceManager, gerenciando a submissão dos dados para a API.
 * 
 * @see PRD: tasks/prd-landing-page-barbearia/prd.md - Seção 3
 * @see TechSpec: tasks/prd-landing-page-barbearia/techspec-frontend.md - Seção 1.4
 * @version 1.0
 * @date 2025-10-22
 */

import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { LogoUploader } from './LogoUploader';
import { ServiceManager } from './ServiceManager';
import { useLandingPage } from '../hooks/useLandingPage';
import { Loader2 } from 'lucide-react';
import type { LandingPageService } from '../types/landing-page.types';

// ============================================================================
// Validation Schema
// ============================================================================

/**
 * Schema de validação do formulário usando Zod
 * 
 * Requisitos:
 * - aboutText: máx. 1000 caracteres (opcional)
 * - openingHours: máx. 500 caracteres (opcional)
 * - instagramUrl: URL válida ou vazio (opcional)
 * - facebookUrl: URL válida ou vazio (opcional)
 * - whatsappNumber: formato brasileiro +55XXXXXXXXXXX (obrigatório)
 */
const formSchema = z.object({
  aboutText: z
    .string()
    .max(1000, 'O texto sobre a barbearia deve ter no máximo 1000 caracteres')
    .optional()
    .or(z.literal('')),
  
  openingHours: z
    .string()
    .max(500, 'O horário de funcionamento deve ter no máximo 500 caracteres')
    .optional()
    .or(z.literal('')),
  
  instagramUrl: z
    .string()
    .url('URL do Instagram inválida')
    .optional()
    .or(z.literal('')),
  
  facebookUrl: z
    .string()
    .url('URL do Facebook inválida')
    .optional()
    .or(z.literal('')),
  
  whatsappNumber: z
    .string()
    .regex(
      /^\+55\d{11}$/,
      'Formato inválido. Use +55 seguido de DDD e número (ex: +5511999999999)'
    )
    .min(1, 'Número do WhatsApp é obrigatório'),
});

type FormValues = z.infer<typeof formSchema>;

// ============================================================================
// Component Props
// ============================================================================

export interface LandingPageFormProps {
  /** ID da barbearia */
  barbershopId: string;
  /** Callback executado após salvamento bem-sucedido */
  onSaveSuccess?: () => void;
  /** Callback executado ao cancelar edição */
  onCancel?: () => void;
  /** Se o formulário está em modo visualização apenas */
  readonly?: boolean;
}

// ============================================================================
// Component
// ============================================================================

export const LandingPageForm: React.FC<LandingPageFormProps> = ({
  barbershopId,
  onSaveSuccess,
  onCancel,
  readonly = false,
}) => {
  // ============================================================================
  // Hooks
  // ============================================================================

  const { config, updateConfig, isUpdating, isLoading } = useLandingPage(barbershopId);

  // Estado local dos serviços (gerenciado pelo ServiceManager)
  const [services, setServices] = useState<LandingPageService[]>([]);

  // Track if we just finished updating
  const [wasUpdating, setWasUpdating] = useState(false);

  // Setup do formulário com react-hook-form
  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      aboutText: '',
      openingHours: '',
      instagramUrl: '',
      facebookUrl: '',
      whatsappNumber: '',
    },
  });

  // ============================================================================
  // Effects
  // ============================================================================

  /**
   * Preenche o formulário com dados da configuração existente
   */
  useEffect(() => {
    if (config) {
      form.reset({
        aboutText: config.aboutText || '',
        openingHours: config.openingHours || '',
        instagramUrl: config.instagramUrl || '',
        facebookUrl: config.facebookUrl || '',
        whatsappNumber: config.whatsappNumber || '',
      });

      // Inicializa estado dos serviços
      if (config.services) {
        setServices(config.services);
      }
    }
  }, [config, form]);

  /**
   * Chama callback de sucesso quando update termina
   */
  useEffect(() => {
    if (wasUpdating && !isUpdating && onSaveSuccess) {
      onSaveSuccess();
      setWasUpdating(false);
    }
  }, [wasUpdating, isUpdating, onSaveSuccess]);

  // ============================================================================
  // Handlers
  // ============================================================================

  /**
   * Handler de submissão do formulário
   * Combina dados do formulário com estado dos serviços e envia para API
   */
  const onSubmit = (data: FormValues) => {
    // Prepara payload com dados do formulário e serviços
    const payload = {
      ...data,
      services: services.map((service) => ({
        serviceId: service.serviceId,
        displayOrder: service.displayOrder,
        isVisible: service.isVisible,
      })),
    };

    // Marca que começou o update
    setWasUpdating(true);

    // Envia para API através do hook
    updateConfig(payload);
  };

  /**
   * Handler do botão Cancelar
   * Reseta formulário para valores originais
   */
  const handleCancel = () => {
    form.reset();
    if (config?.services) {
      setServices(config.services);
    }
    if (onCancel) {
      onCancel();
    }
  };

  // ============================================================================
  // Loading State
  // ============================================================================

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
        <span className="ml-2 text-muted-foreground">Carregando configuração...</span>
      </div>
    );
  }

  if (!config) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">
          Não foi possível carregar a configuração da landing page.
        </p>
      </div>
    );
  }

  // ============================================================================
  // Render
  // ============================================================================

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
        {/* Logo Uploader */}
        <div className="space-y-2">
          <h3 className="text-lg font-semibold">Logo da Barbearia</h3>
          <p className="text-sm text-muted-foreground">
            Faça upload do logo que aparecerá na sua landing page
          </p>
          <LogoUploader
            barbershopId={barbershopId}
            currentLogoUrl={config.logoUrl}
            disabled={readonly || isUpdating}
          />
        </div>

        {/* Sobre a Barbearia */}
        <FormField
          control={form.control}
          name="aboutText"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Sobre a Barbearia</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Conte a história da sua barbearia, seus diferenciais e o que a torna especial..."
                  rows={6}
                  maxLength={1000}
                  disabled={readonly || isUpdating}
                  {...field}
                />
              </FormControl>
              <FormDescription>
                {field.value?.length || 0}/1000 caracteres
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Horário de Funcionamento */}
        <FormField
          control={form.control}
          name="openingHours"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Horário de Funcionamento</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Segunda a Sexta: 09:00 - 19:00&#10;Sábado: 09:00 - 17:00&#10;Domingo: Fechado"
                  rows={4}
                  maxLength={500}
                  disabled={readonly || isUpdating}
                  {...field}
                />
              </FormControl>
              <FormDescription>
                {field.value?.length || 0}/500 caracteres
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* WhatsApp */}
        <FormField
          control={form.control}
          name="whatsappNumber"
          render={({ field }) => (
            <FormItem>
              <FormLabel>WhatsApp</FormLabel>
              <FormControl>
                <Input
                  type="tel"
                  placeholder="+5511999999999"
                  disabled={readonly || isUpdating}
                  {...field}
                />
              </FormControl>
              <FormDescription>
                Formato: +55 + DDD + número (ex: +5511999999999)
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Redes Sociais */}
        <div className="space-y-4">
          <h3 className="text-lg font-semibold">Redes Sociais</h3>
          <p className="text-sm text-muted-foreground">
            Links para suas redes sociais (opcional)
          </p>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Instagram */}
            <FormField
              control={form.control}
              name="instagramUrl"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Instagram</FormLabel>
                  <FormControl>
                    <Input
                      type="url"
                      placeholder="https://instagram.com/suabarbearia"
                      disabled={readonly || isUpdating}
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Facebook */}
            <FormField
              control={form.control}
              name="facebookUrl"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Facebook</FormLabel>
                  <FormControl>
                    <Input
                      type="url"
                      placeholder="https://facebook.com/suabarbearia"
                      disabled={readonly || isUpdating}
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>
        </div>

        {/* Gerenciamento de Serviços */}
        <div className="space-y-2">
          <h3 className="text-lg font-semibold">Serviços</h3>
          <p className="text-sm text-muted-foreground">
            Escolha quais serviços aparecem na landing page e defina a ordem de exibição
          </p>
          <ServiceManager
            services={services}
            onChange={setServices}
            disabled={readonly || isUpdating}
          />
        </div>

        {/* Botões de Ação */}
        {!readonly && (
          <div className="flex justify-end gap-3 pt-6 border-t">
            <Button
              type="button"
              variant="outline"
              onClick={handleCancel}
              disabled={isUpdating}
            >
              Cancelar
            </Button>
            <Button type="submit" disabled={isUpdating}>
              {isUpdating ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Salvando...
                </>
              ) : (
                'Salvar Alterações'
              )}
            </Button>
          </div>
        )}
      </form>
    </Form>
  );
};

export default LandingPageForm;
