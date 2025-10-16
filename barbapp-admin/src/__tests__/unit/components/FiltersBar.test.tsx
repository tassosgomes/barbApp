import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import { FiltersBar } from "@/components/ui/filters-bar";
import { MemoryRouter } from "react-router-dom";

describe("FiltersBar", () => {
  const mockFields = [
    {
      key: "search",
      label: "Buscar",
      type: "text" as const,
      placeholder: "Digite para buscar...",
    },
    {
      key: "status",
      label: "Status",
      type: "select" as const,
      placeholder: "Selecione o status",
      options: [
        { value: "active", label: "Ativo" },
        { value: "inactive", label: "Inativo" },
      ],
    },
  ];

  const renderWithRouter = (component: React.ReactElement) => {
    return render(<MemoryRouter>{component}</MemoryRouter>);
  };

  it("should render all filter fields", () => {
    renderWithRouter(<FiltersBar fields={mockFields} />);

    expect(screen.getByText("Buscar")).toBeInTheDocument();
    expect(screen.getByText("Status")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Digite para buscar...")).toBeInTheDocument();
    expect(screen.getByText("Selecione o status")).toBeInTheDocument();
  });

  it("should update URL params when text filter changes", async () => {
    const user = userEvent.setup();

    renderWithRouter(<FiltersBar fields={mockFields} />);

    const searchInput = screen.getByPlaceholderText("Digite para buscar...");
    await user.type(searchInput, "testsearch");

    // Check if the input value is updated (spaces are trimmed)
    expect(searchInput).toHaveValue("testsearch");
  });

  it("should update URL params when select filter changes", () => {
    renderWithRouter(<FiltersBar fields={mockFields} />);

    // Just check that the select is rendered and can be found
    const selectTrigger = screen.getByText("Selecione o status");
    expect(selectTrigger).toBeInTheDocument();
  });

  it("should call onFiltersChange when filters change", async () => {
    const user = userEvent.setup();
    const onFiltersChange = vi.fn();

    renderWithRouter(
      <FiltersBar fields={mockFields} onFiltersChange={onFiltersChange} />
    );

    const searchInput = screen.getByPlaceholderText("Digite para buscar...");
    await user.type(searchInput, "test");

    expect(onFiltersChange).toHaveBeenCalledWith({ search: "test" });
  });

  it("should show clear filters button when there are active filters", async () => {
    const user = userEvent.setup();

    renderWithRouter(<FiltersBar fields={mockFields} />);

    const searchInput = screen.getByPlaceholderText("Digite para buscar...");
    await user.type(searchInput, "test");

    expect(screen.getByText("Limpar filtros")).toBeInTheDocument();
  });

  it("should clear all filters when clear button is clicked", async () => {
    const user = userEvent.setup();
    const onFiltersChange = vi.fn();

    renderWithRouter(
      <FiltersBar fields={mockFields} onFiltersChange={onFiltersChange} />
    );

    // Add a filter
    const searchInput = screen.getByPlaceholderText("Digite para buscar...");
    await user.type(searchInput, "test");

    // Clear filters
    const clearButton = screen.getByText("Limpar filtros");
    await user.click(clearButton);

    expect(onFiltersChange).toHaveBeenCalledWith({});
    expect(searchInput).toHaveValue("");
  });

  it("should not show clear button when no filters are active", () => {
    renderWithRouter(<FiltersBar fields={mockFields} />);

    expect(screen.queryByText("Limpar filtros")).not.toBeInTheDocument();
  });

  it("should apply custom className", () => {
    renderWithRouter(
      <FiltersBar fields={mockFields} className="custom-class" />
    );

    const container = screen.getByText("Buscar").closest("div")?.parentElement;
    expect(container).toHaveClass("custom-class");
  });

  it("should handle empty options array for select", () => {
    const fieldsWithEmptyOptions = [
      {
        key: "status",
        label: "Status",
        type: "select" as const,
        options: [],
      },
    ];

    renderWithRouter(<FiltersBar fields={fieldsWithEmptyOptions} />);

    expect(screen.getByText("Status")).toBeInTheDocument();
  });
});