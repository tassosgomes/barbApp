import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { userEvent } from "@testing-library/user-event";
import { BarbershopTable } from "@/components/barbershop/BarbershopTable";
import type { Barbershop } from "@/types/barbershop";

const mockBarbershops: Barbershop[] = [
  {
    id: "1",
    name: "Barbearia Teste",
    document: "12.345.678/0001-90",
    phone: "(11) 99999-9999",
    ownerName: "João Silva",
    email: "teste@email.com",
    code: "ABC123XY",
    isActive: true,
    address: {
      zipCode: "01000-000",
      street: "Rua Teste",
      number: "123",
      neighborhood: "Centro",
      city: "São Paulo",
      state: "SP",
    },
    createdAt: "2024-01-01T00:00:00Z",
    updatedAt: "2024-01-01T00:00:00Z",
  },
];

describe("BarbershopTable", () => {
  const defaultProps = {
    barbershops: mockBarbershops,
    onView: vi.fn(),
    onEdit: vi.fn(),
    onDeactivate: vi.fn(),
    onReactivate: vi.fn(),
    onCopyCode: vi.fn(),
    onResendCredentials: vi.fn(),
  };

  it("should render barbershop data correctly", () => {
    render(<BarbershopTable {...defaultProps} />);

    expect(screen.getByText("Barbearia Teste")).toBeInTheDocument();
    expect(screen.getByText("ABC123XY")).toBeInTheDocument();
    expect(screen.getByText("São Paulo - SP")).toBeInTheDocument();
    expect(screen.getByText("Ativo")).toBeInTheDocument();
    expect(screen.getByText("31/12/2023")).toBeInTheDocument();
  });

  it("should call onView when view button is clicked", async () => {
    const user = userEvent.setup();
    const onView = vi.fn();

    render(<BarbershopTable {...defaultProps} onView={onView} />);

    await user.click(screen.getByRole("button", { name: /ver/i }));
    expect(onView).toHaveBeenCalledWith("1");
  });

  it("should call onEdit when edit button is clicked", async () => {
    const user = userEvent.setup();
    const onEdit = vi.fn();

    render(<BarbershopTable {...defaultProps} onEdit={onEdit} />);

    await user.click(screen.getByRole("button", { name: /editar/i }));
    expect(onEdit).toHaveBeenCalledWith("1");
  });

  it("should call onDeactivate when deactivate button is clicked for active barbershop", async () => {
    const user = userEvent.setup();
    const onDeactivate = vi.fn();

    render(<BarbershopTable {...defaultProps} onDeactivate={onDeactivate} />);

    await user.click(screen.getByRole("button", { name: /desativar/i }));
    expect(onDeactivate).toHaveBeenCalledWith("1");
  });

  it("should call onReactivate when reactivate button is clicked for inactive barbershop", async () => {
    const user = userEvent.setup();
    const onReactivate = vi.fn();
    const inactiveBarbershop = { ...mockBarbershops[0], isActive: false };

    render(
      <BarbershopTable
        {...defaultProps}
        barbershops={[inactiveBarbershop]}
        onReactivate={onReactivate}
      />
    );

    await user.click(screen.getByRole("button", { name: /reativar/i }));
    expect(onReactivate).toHaveBeenCalledWith("1");
  });

  it("should call onCopyCode when code button is clicked", async () => {
    const user = userEvent.setup();
    const onCopyCode = vi.fn();

    render(<BarbershopTable {...defaultProps} onCopyCode={onCopyCode} />);

    await user.click(screen.getByText("ABC123XY"));
    expect(onCopyCode).toHaveBeenCalledWith("ABC123XY");
  });

  it("should call onResendCredentials when resend credentials button is clicked", async () => {
    const user = userEvent.setup();
    const onResendCredentials = vi.fn();

    render(<BarbershopTable {...defaultProps} onResendCredentials={onResendCredentials} />);

    await user.click(screen.getByRole("button", { name: /reenviar credenciais/i }));
    expect(onResendCredentials).toHaveBeenCalledWith("1");
  });
});