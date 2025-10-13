import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { userEvent } from "@testing-library/user-event";
import { EmptyState } from "@/components/barbershop/EmptyState";

describe("EmptyState", () => {
  it("should render default content", () => {
    render(<EmptyState />);

    expect(screen.getByText("Nenhuma barbearia encontrada")).toBeInTheDocument();
    expect(screen.getByText("Comece cadastrando a primeira barbearia do sistema.")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "+ Nova Barbearia" })).toBeInTheDocument();
  });

  it("should render custom content", () => {
    const onAction = vi.fn();

    render(
      <EmptyState
        title="Custom Title"
        description="Custom description"
        actionLabel="Custom Action"
        onAction={onAction}
      />
    );

    expect(screen.getByText("Custom Title")).toBeInTheDocument();
    expect(screen.getByText("Custom description")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Custom Action" })).toBeInTheDocument();
  });

  it("should call onAction when action button is clicked", async () => {
    const user = userEvent.setup();
    const onAction = vi.fn();

    render(<EmptyState onAction={onAction} />);

    await user.click(screen.getByRole("button", { name: "+ Nova Barbearia" }));
    expect(onAction).toHaveBeenCalled();
  });

  it("should not render action button when onAction is not provided", () => {
    render(<EmptyState />);

    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });

  it("should apply custom className", () => {
    render(<EmptyState className="custom-class" />);

    const container = screen.getByText("Nenhuma barbearia encontrada").parentElement;
    expect(container).toHaveClass("custom-class");
  });
});