import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { userEvent } from "@testing-library/user-event";
import { ResendCredentialsModal } from "@/components/barbershop/ResendCredentialsModal";

describe("ResendCredentialsModal", () => {
  const defaultProps = {
    open: true,
    onClose: vi.fn(),
    onConfirm: vi.fn(),
    barbershopName: "Barbearia Teste",
    barbershopEmail: "teste@email.com",
    isLoading: false,
  };

  it("should render modal with barbershop information", () => {
    render(<ResendCredentialsModal {...defaultProps} />);

    expect(screen.getByRole("heading", { name: "Reenviar Credenciais" })).toBeInTheDocument();
    expect(screen.getByText(/Barbearia Teste/)).toBeInTheDocument();
    expect(screen.getByText(/teste@email.com/)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancelar reenvio de credenciais/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /confirmar reenvio de credenciais/i })).toBeInTheDocument();
  });

  it("should render modal without barbershop information", () => {
    render(<ResendCredentialsModal {...defaultProps} barbershopName={undefined} barbershopEmail={undefined} />);

    expect(screen.getByRole("heading", { name: "Reenviar Credenciais" })).toBeInTheDocument();
    expect(screen.getByText(/Deseja reenviar as credenciais de acesso desta barbearia?/)).toBeInTheDocument();
  });

  it("should call onConfirm when confirm button is clicked", async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn();

    render(<ResendCredentialsModal {...defaultProps} onConfirm={onConfirm} />);

    await user.click(screen.getByRole("button", { name: /confirmar reenvio de credenciais/i }));
    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it("should call onClose when cancel button is clicked", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();

    render(<ResendCredentialsModal {...defaultProps} onClose={onClose} />);

    await user.click(screen.getByRole("button", { name: /cancelar reenvio de credenciais/i }));
    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it("should disable buttons when loading", () => {
    render(<ResendCredentialsModal {...defaultProps} isLoading={true} />);

    expect(screen.getByRole("button", { name: /cancelar reenvio de credenciais/i })).toBeDisabled();
    expect(screen.getByRole("button", { name: /confirmar reenvio de credenciais/i })).toBeDisabled();
  });

  it("should show loading text when isLoading is true", () => {
    render(<ResendCredentialsModal {...defaultProps} isLoading={true} />);

    const confirmButton = screen.getByRole("button", { name: /confirmar reenvio de credenciais/i });
    expect(confirmButton).toBeInTheDocument();
    expect(confirmButton).toHaveTextContent("Enviando...");
  });

  it("should not render when open is false", () => {
    render(<ResendCredentialsModal {...defaultProps} open={false} />);

    expect(screen.queryByRole("heading", { name: "Reenviar Credenciais" })).not.toBeInTheDocument();
  });
});
