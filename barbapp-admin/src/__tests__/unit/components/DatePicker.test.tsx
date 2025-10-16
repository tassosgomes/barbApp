import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import { DatePicker } from "@/components/ui/date-picker";

describe("DatePicker", () => {
  it("should render date input with correct type", () => {
    render(<DatePicker />);

    const input = screen.getByPlaceholderText("Selecione uma data");
    expect(input).toHaveAttribute("type", "date");
  });

  it("should display provided value", () => {
    const testDate = "2024-01-15";
    render(<DatePicker value={testDate} />);

    const input = screen.getByDisplayValue(testDate);
    expect(input).toBeInTheDocument();
  });

  it("should call onChange when value changes", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    render(<DatePicker onChange={onChange} />);

    const input = screen.getByPlaceholderText("Selecione uma data");
    await user.type(input, "2024-01-15");

    expect(onChange).toHaveBeenCalledWith("2024-01-15");
  });

  it("should display placeholder text", () => {
    const placeholder = "Select a date";
    render(<DatePicker placeholder={placeholder} />);

    const input = screen.getByPlaceholderText(placeholder);
    expect(input).toBeInTheDocument();
  });

  it("should use default placeholder when not provided", () => {
    render(<DatePicker />);

    const input = screen.getByPlaceholderText("Selecione uma data");
    expect(input).toBeInTheDocument();
  });

  it("should apply custom className", () => {
    render(<DatePicker className="custom-date-picker" />);

    const input = screen.getByPlaceholderText("Selecione uma data");
    expect(input).toHaveClass("custom-date-picker");
  });

  it("should be disabled when disabled prop is true", () => {
    render(<DatePicker disabled={true} />);

    const input = screen.getByPlaceholderText("Selecione uma data");
    expect(input).toBeDisabled();
  });

  it("should be enabled by default", () => {
    render(<DatePicker />);

    const input = screen.getByPlaceholderText("Selecione uma data");
    expect(input).not.toBeDisabled();
  });

  it("should forward ref correctly", () => {
    const ref = vi.fn();
    render(<DatePicker ref={ref} />);

    expect(ref).toHaveBeenCalled();
  });

  it("should handle empty value", () => {
    render(<DatePicker value="" />);

    const input = screen.getByPlaceholderText("Selecione uma data");
    expect(input).toHaveValue("");
  });

  it("should pass through additional props", () => {
    render(<DatePicker data-testid="date-picker" />);

    const input = screen.getByTestId("date-picker");
    expect(input).toBeInTheDocument();
  });
});