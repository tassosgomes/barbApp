import { Button } from "@/components/ui/button";
import { FileX } from "lucide-react";

interface EmptyStateProps {
  title?: string;
  description?: string;
  actionLabel?: string;
  onAction?: () => void;
  className?: string;
}

export function EmptyState({
  title = "Nenhuma barbearia encontrada",
  description = "Comece cadastrando a primeira barbearia do sistema.",
  actionLabel = "+ Nova Barbearia",
  onAction,
  className,
}: EmptyStateProps) {
  return (
    <div className={`flex flex-col items-center justify-center py-12 text-center ${className}`}>
      <FileX className="mb-4 h-16 w-16 text-gray-400" />
      <h3 className="mb-2 text-lg font-semibold text-gray-900">
        {title}
      </h3>
      <p className="mb-6 text-sm text-gray-500">
        {description}
      </p>
      {onAction && (
        <Button onClick={onAction}>
          {actionLabel}
        </Button>
      )}
    </div>
  );
}