import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { StatusBadge } from "@/components/ui/status-badge";

describe("StatusBadge", () => {
  it("should render active status correctly", () => {
    render(<StatusBadge isActive={true} />);

    const badge = screen.getByText("Ativo");
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass("bg-green-100", "text-green-800");
  });

  it("should render inactive status correctly", () => {
    render(<StatusBadge isActive={false} />);

    const badge = screen.getByText("Inativo");
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass("bg-gray-100", "text-gray-800");
  });

  it("should apply custom className", () => {
    render(<StatusBadge isActive={true} className="custom-class" />);

    const badge = screen.getByText("Ativo");
    expect(badge).toHaveClass("custom-class");
  });
});