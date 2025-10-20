/**
 * CancelConfirmationDialog Component
 * 
 * Dialog de confirmação antes de cancelar um agendamento.
 * Previne cancelamentos acidentais exigindo confirmação explícita.
 */

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { AlertCircle } from 'lucide-react';

export interface CancelConfirmationDialogProps {
  /** Se o dialog está aberto */
  open: boolean;
  /** Callback ao mudar o estado de abertura */
  onOpenChange: (open: boolean) => void;
  /** Callback ao confirmar o cancelamento */
  onConfirm: () => void;
  /** Nome do cliente (opcional, para personalizar a mensagem) */
  customerName?: string;
  /** Estado de loading durante a ação */
  isLoading?: boolean;
}

export function CancelConfirmationDialog({
  open,
  onOpenChange,
  onConfirm,
  customerName,
  isLoading = false,
}: CancelConfirmationDialogProps) {
  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <div className="flex items-center gap-2">
            <AlertCircle className="h-5 w-5 text-destructive" />
            <AlertDialogTitle>Cancelar Agendamento</AlertDialogTitle>
          </div>
          <AlertDialogDescription>
            {customerName ? (
              <>
                Tem certeza que deseja cancelar o agendamento de{' '}
                <span className="font-semibold">{customerName}</span>?
              </>
            ) : (
              'Tem certeza que deseja cancelar este agendamento?'
            )}
            <br />
            <br />
            Esta ação não pode ser desfeita.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isLoading}>Voltar</AlertDialogCancel>
          <AlertDialogAction
            onClick={(e) => {
              e.preventDefault();
              onConfirm();
            }}
            disabled={isLoading}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            {isLoading ? 'Cancelando...' : 'Sim, Cancelar'}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
