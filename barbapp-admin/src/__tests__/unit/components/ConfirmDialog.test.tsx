import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import { ConfirmDialog } from "@/components/ui/confirm-dialog";

describe("ConfirmDialog", () => {
  const defaultProps = {
    open: true,
    onClose: vi.fn(),
    onConfirm: vi.fn(),
    title: "Test Title",
    description: "Test description",
  };

  it("should render modal with provided title and description", () => {
    render(<ConfirmDialog {...defaultProps} />);

    expect(screen.getByText("Test Title")).toBeInTheDocument();
    expect(screen.getByText("Test description")).toBeInTheDocument();
  });

  it("should not render modal when open is false", () => {
    render(<ConfirmDialog {...defaultProps} open={false} />);

    expect(screen.queryByText("Test Title")).not.toBeInTheDocument();
  });

  it("should call onClose when cancel button is clicked", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();

    render(<ConfirmDialog {...defaultProps} onClose={onClose} />);

    await user.click(screen.getByRole("button", { name: /cancelar/i }));

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it("should call onConfirm when confirm button is clicked", async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn();

    render(<ConfirmDialog {...defaultProps} onConfirm={onConfirm} />);

    await user.click(screen.getByRole("button", { name: /confirmar/i }));

    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it("should use custom button texts", () => {
    render(
      <ConfirmDialog
        {...defaultProps}
        confirmText="Delete"
        cancelText="Keep"
      />
    );

    expect(screen.getByRole("button", { name: "Keep" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Delete" })).toBeInTheDocument();
  });

  it("should apply correct button variant", () => {
    render(<ConfirmDialog {...defaultProps} confirmVariant="destructive" />);

    const confirmButton = screen.getByRole("button", { name: /confirmar/i });
    expect(confirmButton).toHaveClass("bg-destructive");
  });

  it("should show loading state when isLoading is true", () => {
    render(<ConfirmDialog {...defaultProps} isLoading={true} />);

    expect(screen.getByText("Processando...")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancelar/i })).toBeDisabled();
    expect(screen.getByRole("button", { name: /confirmar/i })).toBeDisabled();
  });

  it("should render ReactNode description", () => {
    const descriptionNode = <span>Custom <strong>description</strong></span>;

    render(
      <ConfirmDialog
        {...defaultProps}
        description={descriptionNode}
      />
    );

    expect(screen.getByText("Custom")).toBeInTheDocument();
    expect(screen.getByText("description")).toBeInTheDocument();
  });

  it("should have proper accessibility attributes", () => {
    render(<ConfirmDialog {...defaultProps} />);

    const dialog = screen.getByRole("dialog");
    expect(dialog).toBeInTheDocument();

    const description = screen.getByText("Test description");
    expect(description).toHaveAttribute("id", "confirm-description");

    expect(dialog).toHaveAttribute("aria-describedby", "confirm-description");
  });

  it("should have proper ARIA labels on buttons", () => {
    render(<ConfirmDialog {...defaultProps} />);

    expect(screen.getByRole("button", { name: "Cancelar" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Confirmar" })).toBeInTheDocument();
  });
});