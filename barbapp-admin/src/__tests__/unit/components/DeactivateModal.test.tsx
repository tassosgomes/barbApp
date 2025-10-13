import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import { DeactivateModal } from "@/components/barbershop/DeactivateModal";

describe("DeactivateModal", () => {
  const defaultProps = {
    open: true,
    onClose: vi.fn(),
    onConfirm: vi.fn(),
  };

  it("should render modal when open is true", () => {
    render(<DeactivateModal {...defaultProps} />);

    expect(screen.getByRole("heading", { name: "Confirmar Desativação" })).toBeInTheDocument();
    expect(screen.getByText(/Tem certeza que deseja desativar/)).toBeInTheDocument();
  });

  it("should not render modal when open is false", () => {
    render(<DeactivateModal {...defaultProps} open={false} />);

    expect(screen.queryByText("Confirmar Desativação")).not.toBeInTheDocument();
  });

  it("should display barbershop name and code when provided", () => {
    render(
      <DeactivateModal
        {...defaultProps}
        barbershopName="Barbearia Teste"
        barbershopCode="ABC123"
      />
    );

    expect(screen.getByText(/Barbearia Teste/)).toBeInTheDocument();
    expect(screen.getByText(/ABC123/)).toBeInTheDocument();
  });

  it("should call onClose when cancel button is clicked", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();

    render(<DeactivateModal {...defaultProps} onClose={onClose} />);

    await user.click(screen.getByRole("button", { name: /cancelar/i }));

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it("should call onConfirm when confirm button is clicked", async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn();

    render(<DeactivateModal {...defaultProps} onConfirm={onConfirm} />);

    await user.click(screen.getByRole("button", { name: /confirmar desativação/i }));

    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it("should show loading state when isLoading is true", () => {
    render(<DeactivateModal {...defaultProps} isLoading={true} />);

    expect(screen.getByText("Desativando...")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancelar/i })).toBeDisabled();
    expect(screen.getByRole("button", { name: /confirmar desativação/i })).toBeDisabled();
  });

  it("should have proper accessibility attributes", () => {
    render(<DeactivateModal {...defaultProps} />);

    const dialog = screen.getByRole("dialog");
    expect(dialog).toBeInTheDocument();

    const description = screen.getByText(/esta ação não poderá ser desfeita/i);
    expect(description).toHaveAttribute("id", "deactivate-description");

    expect(dialog).toHaveAttribute("aria-describedby", "deactivate-description");
  });

  it("should have proper ARIA labels on buttons", () => {
    render(<DeactivateModal {...defaultProps} />);

    expect(screen.getByRole("button", { name: "Cancelar desativação" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Confirmar desativação da barbearia" })).toBeInTheDocument();
  });
});