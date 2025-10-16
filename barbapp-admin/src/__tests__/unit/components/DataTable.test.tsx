import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import { DataTable } from "@/components/ui/data-table";
import type { PaginatedResponse } from "@/types";

interface TestItem {
  id: string;
  name: string;
  value: number;
}

describe("DataTable", () => {
  const mockColumns = [
    { key: "name", header: "Name" },
    { key: "value", header: "Value" },
  ];

  const mockData: PaginatedResponse<TestItem> = {
    items: [
      { id: "1", name: "Item 1", value: 10 },
      { id: "2", name: "Item 2", value: 20 },
    ],
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    totalCount: 2,
    hasPreviousPage: false,
    hasNextPage: false,
  };

  it("should render table with data", () => {
    render(<DataTable data={mockData} columns={mockColumns} />);

    expect(screen.getByText("Name")).toBeInTheDocument();
    expect(screen.getByText("Value")).toBeInTheDocument();
    expect(screen.getByText("Item 1")).toBeInTheDocument();
    expect(screen.getByText("Item 2")).toBeInTheDocument();
    expect(screen.getByText("10")).toBeInTheDocument();
    expect(screen.getByText("20")).toBeInTheDocument();
  });

  it("should render custom cell content with render function", () => {
    const columnsWithRender = [
      { key: "name", header: "Name" },
      {
        key: "value",
        header: "Value",
        render: (item: TestItem) => `$${item.value}`,
      },
    ];

    render(<DataTable data={mockData} columns={columnsWithRender} />);

    expect(screen.getByText("$10")).toBeInTheDocument();
    expect(screen.getByText("$20")).toBeInTheDocument();
  });

  it("should show loading skeleton when isLoading is true", () => {
    render(<DataTable data={mockData} columns={mockColumns} isLoading={true} />);

    // Check for skeleton elements (they have animate-pulse class)
    const skeletons = screen.getAllByText("", { selector: '[class*="animate-pulse"]' });
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("should show empty message when no data", () => {
    const emptyData: PaginatedResponse<TestItem> = {
      ...mockData,
      items: [],
      totalCount: 0,
    };

    render(<DataTable data={emptyData} columns={mockColumns} />);

    expect(screen.getByText("Nenhum item encontrado")).toBeInTheDocument();
  });

  it("should show custom empty message", () => {
    const emptyData: PaginatedResponse<TestItem> = {
      ...mockData,
      items: [],
      totalCount: 0,
    };

    render(
      <DataTable
        data={emptyData}
        columns={mockColumns}
        emptyMessage="No items available"
      />
    );

    expect(screen.getByText("No items available")).toBeInTheDocument();
  });

  it("should render pagination when there are multiple pages", () => {
    const multiPageData: PaginatedResponse<TestItem> = {
      ...mockData,
      totalPages: 3,
      hasNextPage: true,
    };

    render(<DataTable data={multiPageData} columns={mockColumns} />);

    expect(screen.getByText("Página 1 de 3")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /próxima/i })).toBeInTheDocument();
  });

  it("should not render pagination for single page", () => {
    render(<DataTable data={mockData} columns={mockColumns} />);

    expect(screen.queryByText(/página/i)).not.toBeInTheDocument();
  });

  it("should call onPageChange when page changes", async () => {
    const user = userEvent.setup();
    const onPageChange = vi.fn();

    const multiPageData: PaginatedResponse<TestItem> = {
      ...mockData,
      totalPages: 3,
      hasNextPage: true,
    };

    render(
      <DataTable
        data={multiPageData}
        columns={mockColumns}
        onPageChange={onPageChange}
      />
    );

    await user.click(screen.getByRole("button", { name: /próxima/i }));

    expect(onPageChange).toHaveBeenCalledWith(2);
  });

  it.skip("should apply custom className", () => {
    render(
      <DataTable
        data={mockData}
        columns={mockColumns}
        className="custom-class"
      />
    );

    // The className is applied to the root div
    const rootDiv = screen.getByText("Item 1").parentElement?.parentElement?.parentElement;
    expect(rootDiv).toHaveClass("custom-class");
  });

  it("should apply column className", () => {
    const columnsWithClass = [
      { key: "name", header: "Name", className: "name-column" },
      { key: "value", header: "Value" },
    ];

    render(<DataTable data={mockData} columns={columnsWithClass} />);

    const nameHeader = screen.getByText("Name");
    expect(nameHeader).toHaveClass("name-column");
  });
});