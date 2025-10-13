import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { userEvent } from "@testing-library/user-event";
import { Pagination } from "@/components/ui/pagination";

describe("Pagination", () => {
  const defaultProps = {
    currentPage: 2,
    totalPages: 5,
    onPageChange: vi.fn(),
    hasPreviousPage: true,
    hasNextPage: true,
  };

  it("should render current page and total pages", () => {
    render(<Pagination {...defaultProps} />);

    expect(screen.getByText("Página 2 de 5")).toBeInTheDocument();
  });

  it("should call onPageChange with previous page when previous button is clicked", async () => {
    const user = userEvent.setup();
    const onPageChange = vi.fn();

    render(<Pagination {...defaultProps} onPageChange={onPageChange} />);

    await user.click(screen.getByRole("button", { name: /anterior/i }));
    expect(onPageChange).toHaveBeenCalledWith(1);
  });

  it("should call onPageChange with next page when next button is clicked", async () => {
    const user = userEvent.setup();
    const onPageChange = vi.fn();

    render(<Pagination {...defaultProps} onPageChange={onPageChange} />);

    await user.click(screen.getByRole("button", { name: /próxima/i }));
    expect(onPageChange).toHaveBeenCalledWith(3);
  });

  it("should disable previous button when hasPreviousPage is false", () => {
    render(<Pagination {...defaultProps} hasPreviousPage={false} />);

    const previousButton = screen.getByRole("button", { name: /anterior/i });
    expect(previousButton).toBeDisabled();
  });

  it("should disable next button when hasNextPage is false", () => {
    render(<Pagination {...defaultProps} hasNextPage={false} />);

    const nextButton = screen.getByRole("button", { name: /próxima/i });
    expect(nextButton).toBeDisabled();
  });

  it("should apply custom className", () => {
    render(<Pagination {...defaultProps} className="custom-class" />);

    const container = screen.getByText("Página 2 de 5").parentElement;
    expect(container).toHaveClass("custom-class");
  });
});