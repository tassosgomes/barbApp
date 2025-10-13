import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { LoadingSpinner } from "@/components/ui/loading-spinner";

describe("LoadingSpinner", () => {
  it("should render with default size", () => {
    render(<LoadingSpinner />);

    const spinner = screen.getByRole("generic"); // div element
    expect(spinner).toHaveClass("animate-spin", "h-6", "w-6");
  });

  it("should render with small size", () => {
    render(<LoadingSpinner size="sm" />);

    const spinner = screen.getByRole("generic");
    expect(spinner).toHaveClass("h-4", "w-4");
  });

  it("should render with large size", () => {
    render(<LoadingSpinner size="lg" />);

    const spinner = screen.getByRole("generic");
    expect(spinner).toHaveClass("h-8", "w-8");
  });

  it("should apply custom className", () => {
    render(<LoadingSpinner className="custom-class" />);

    const spinner = screen.getByRole("generic");
    expect(spinner).toHaveClass("custom-class");
  });
});