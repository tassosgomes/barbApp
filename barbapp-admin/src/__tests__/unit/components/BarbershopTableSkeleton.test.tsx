import { render } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { BarbershopTableSkeleton } from "@/components/barbershop/BarbershopTableSkeleton";

describe("BarbershopTableSkeleton", () => {
  it("should render skeleton rows", () => {
    render(<BarbershopTableSkeleton />);

    const skeletons = document.querySelectorAll(".animate-pulse");
    expect(skeletons.length).toBeGreaterThan(0);
  });
});