import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';

interface ResendCredentialsModalProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  barbershopName?: string;
  barbershopEmail?: string;
  isLoading?: boolean;
}

export function ResendCredentialsModal({
  open,
  onClose,
  onConfirm,
  barbershopName,
  barbershopEmail,
  isLoading = false,
}: ResendCredentialsModalProps) {
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
        aria-describedby="resend-credentials-description"
      >
        <DialogHeader>
          <DialogTitle>Reenviar Credenciais</DialogTitle>
          <DialogDescription id="resend-credentials-description">
            {barbershopName && barbershopEmail ? (
              <>
                Deseja reenviar as credenciais de acesso da barbearia{' '}
                <strong>{barbershopName}</strong> para o e-mail{' '}
                <strong>{barbershopEmail}</strong>?
                <br />
                <br />
                Um novo e-mail será enviado com as instruções de acesso ao sistema.
              </>
            ) : (
              <>
                Deseja reenviar as credenciais de acesso desta barbearia?
                <br />
                <br />
                Um novo e-mail será enviado com as instruções de acesso ao sistema.
              </>
            )}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button
            variant="outline"
            onClick={handleCancel}
            disabled={isLoading}
            aria-label="Cancelar reenvio de credenciais"
          >
            Cancelar
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={isLoading}
            aria-label="Confirmar reenvio de credenciais"
          >
            {isLoading ? 'Enviando...' : 'Reenviar Credenciais'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
