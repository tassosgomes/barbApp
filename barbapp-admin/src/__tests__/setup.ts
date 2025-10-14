import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';

// Set environment variables for tests
process.env.VITE_API_URL = 'http://localhost:5070/api';
process.env.VITE_APP_NAME = 'BarbApp Admin';

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
};

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
});

// Mock window.location
delete (window as unknown as Record<string, unknown>).location;
(window as unknown as Record<string, unknown>).location = { href: '' } as Location;

// Suppress DataCloneError warnings from Vitest (related to Axios serialization)
const originalWarn = console.warn;
console.warn = (...args) => {
  if (args[0]?.includes?.('DataCloneError') || args[0]?.includes?.('could not be cloned')) {
    return; // Suppress these specific warnings
  }
  originalWarn.apply(console, args);
};

afterEach(() => {
  cleanup();
  vi.clearAllMocks();
});