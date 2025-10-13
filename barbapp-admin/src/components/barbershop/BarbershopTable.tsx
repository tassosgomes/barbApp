import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { StatusBadge } from "@/components/ui/status-badge";
import type { Barbershop } from "@/types/barbershop";

interface BarbershopTableProps {
  barbershops: Barbershop[];
  onView: (id: string) => void;
  onEdit: (id: string) => void;
  onDeactivate: (id: string) => void;
  onReactivate: (id: string) => void;
  onCopyCode: (code: string) => void;
}

export function BarbershopTable({
  barbershops,
  onView,
  onEdit,
  onDeactivate,
  onReactivate,
  onCopyCode,
}: BarbershopTableProps) {
  return (
    <div className="rounded-lg border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Nome</TableHead>
            <TableHead>Código</TableHead>
            <TableHead>Cidade/UF</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Data de Criação</TableHead>
            <TableHead className="text-right">Ações</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {barbershops.map((barbershop) => (
            <TableRow key={barbershop.id}>
              <TableCell className="font-medium">{barbershop.name}</TableCell>
              <TableCell>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => onCopyCode(barbershop.code)}
                  className="h-auto p-0 font-mono text-blue-600 hover:text-blue-800"
                >
                  {barbershop.code}
                </Button>
              </TableCell>
              <TableCell>
                {barbershop.address.city} - {barbershop.address.state}
              </TableCell>
              <TableCell>
                <StatusBadge isActive={barbershop.isActive} />
              </TableCell>
              <TableCell>
                {new Date(barbershop.createdAt).toLocaleDateString("pt-BR")}
              </TableCell>
              <TableCell className="text-right">
                <div className="flex justify-end gap-2">
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => onView(barbershop.id)}
                  >
                    Ver
                  </Button>
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => onEdit(barbershop.id)}
                  >
                    Editar
                  </Button>
                  {barbershop.isActive ? (
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => onDeactivate(barbershop.id)}
                      className="text-red-600 hover:text-red-800"
                    >
                      Desativar
                    </Button>
                  ) : (
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => onReactivate(barbershop.id)}
                      className="text-green-600 hover:text-green-800"
                    >
                      Reativar
                    </Button>
                  )}
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}