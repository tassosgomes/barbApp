import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { BarbershopTableSkeleton } from "@/components/barbershop/BarbershopTableSkeleton";

describe("BarbershopTableSkeleton", () => {
  it("should render 5 skeleton rows", () => {
    render(<BarbershopTableSkeleton />);

    const skeletons = screen.getAllByRole("generic"); // Skeleton components are divs
    expect(skeletons).toHaveLength(5);
  });

  it("should have correct skeleton styling", () => {
    render(<BarbershopTableSkeleton />);

    const skeletons = screen.getAllByRole("generic");
    skeletons.forEach((skeleton) => {
      expect(skeleton).toHaveClass("h-16", "w-full");
    });
  });
});