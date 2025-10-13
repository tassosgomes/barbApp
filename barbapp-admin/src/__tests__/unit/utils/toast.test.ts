import { describe, it, expect, vi, beforeEach } from "vitest";
import { toast } from "@/hooks/use-toast";
import {
  showSuccessToast,
  showErrorToast,
  showInfoToast,
  showWarningToast,
} from "@/utils/toast";

// Mock the toast hook
vi.mock("@/hooks/use-toast", () => ({
  toast: vi.fn(),
}));

describe("Toast utilities", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("showSuccessToast", () => {
    it("should call toast with success configuration", () => {
      showSuccessToast("Success title", "Success description");

      expect(toast).toHaveBeenCalledWith({
        title: "Success title",
        description: "Success description",
        variant: "default",
      });
    });

    it("should call toast without description", () => {
      showSuccessToast("Success title");

      expect(toast).toHaveBeenCalledWith({
        title: "Success title",
        description: undefined,
        variant: "default",
      });
    });
  });

  describe("showErrorToast", () => {
    it("should call toast with error configuration", () => {
      showErrorToast("Error title", "Error description");

      expect(toast).toHaveBeenCalledWith({
        title: "Error title",
        description: "Error description",
        variant: "destructive",
      });
    });

    it("should call toast without description", () => {
      showErrorToast("Error title");

      expect(toast).toHaveBeenCalledWith({
        title: "Error title",
        description: undefined,
        variant: "destructive",
      });
    });
  });

  describe("showInfoToast", () => {
    it("should call toast with info configuration", () => {
      showInfoToast("Info title", "Info description");

      expect(toast).toHaveBeenCalledWith({
        title: "Info title",
        description: "Info description",
        variant: "default",
      });
    });

    it("should call toast without description", () => {
      showInfoToast("Info title");

      expect(toast).toHaveBeenCalledWith({
        title: "Info title",
        description: undefined,
        variant: "default",
      });
    });
  });

  describe("showWarningToast", () => {
    it("should call toast with warning configuration", () => {
      showWarningToast("Warning title", "Warning description");

      expect(toast).toHaveBeenCalledWith({
        title: "Warning title",
        description: "Warning description",
        variant: "default",
      });
    });

    it("should call toast without description", () => {
      showWarningToast("Warning title");

      expect(toast).toHaveBeenCalledWith({
        title: "Warning title",
        description: undefined,
        variant: "default",
      });
    });
  });
});