import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';

interface DeactivateModalProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  barbershopName?: string;
  barbershopCode?: string;
  isLoading?: boolean;
}

export function DeactivateModal({
  open,
  onClose,
  onConfirm,
  barbershopName,
  barbershopCode,
  isLoading = false,
}: DeactivateModalProps) {
  const handleConfirm = () => {
    onConfirm();
  };

  const handleCancel = () => {
    onClose();
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent
        className="sm:max-w-[425px]"
        aria-describedby="deactivate-description"
      >
        <DialogHeader>
          <DialogTitle>Confirmar Desativação</DialogTitle>
          <DialogDescription id="deactivate-description">
            {barbershopName && barbershopCode ? (
              <>
                Tem certeza que deseja desativar a barbearia{' '}
                <strong>{barbershopName}</strong> (Código: {barbershopCode})?
                <br />
                <br />
                Esta ação não poderá ser desfeita automaticamente, mas a barbearia
                pode ser reativada posteriormente.
              </>
            ) : (
              <>
                Tem certeza que deseja desativar esta barbearia?
                <br />
                <br />
                Esta ação não poderá ser desfeita automaticamente, mas a barbearia
                pode ser reativada posteriormente.
              </>
            )}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button
            variant="outline"
            onClick={handleCancel}
            disabled={isLoading}
            aria-label="Cancelar desativação"
          >
            Cancelar
          </Button>
          <Button
            variant="destructive"
            onClick={handleConfirm}
            disabled={isLoading}
            aria-label="Confirmar desativação da barbearia"
          >
            {isLoading ? 'Desativando...' : 'Confirmar Desativação'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}