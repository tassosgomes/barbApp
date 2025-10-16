import '@testing-library/jest-dom/vitest';
import { beforeAll, afterAll, afterEach, vi } from 'vitest';
import { server } from './mocks/server';

// Stub environment variables for tests
vi.stubEnv('VITE_API_URL', 'http://localhost:5000/api');
vi.stubEnv('VITE_APP_NAME', 'BarbApp Admin');

// Mock localStorage and sessionStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
};
const sessionStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
};

global.localStorage = localStorageMock as unknown as Storage;
global.sessionStorage = sessionStorageMock as unknown as Storage;

// Mock window.location
Object.defineProperty(window, 'location', {
  value: { href: '' },
  writable: true,
});

// Polyfill for Radix UI components that require pointer capture and scroll into view
if (typeof Element.prototype.hasPointerCapture === 'undefined') {
  Element.prototype.hasPointerCapture = vi.fn(() => false);
}

if (typeof Element.prototype.scrollIntoView === 'undefined') {
  Element.prototype.scrollIntoView = vi.fn();
}

if (typeof Element.prototype.releasePointerCapture === 'undefined') {
  Element.prototype.releasePointerCapture = vi.fn();
}

if (typeof Element.prototype.setPointerCapture === 'undefined') {
  Element.prototype.setPointerCapture = vi.fn();
}

// Suppress DataCloneError warnings from Vitest (related to Axios serialization)
const originalWarn = console.warn;
console.warn = (...args) => {
  if (args[0]?.includes?.('DataCloneError') || args[0]?.includes?.('could not be cloned')) {
    return; // Suppress these specific warnings
  }
  originalWarn.apply(console, args);
};

// Suppress Radix UI test environment errors
const originalError = console.error;
console.error = (...args) => {
  if (args[0]?.includes?.('hasPointerCapture') || args[0]?.includes?.('scrollIntoView')) {
    return; // Suppress Radix UI test environment errors
  }
  originalError.apply(console, args);
};

// Establish API mocking before all tests
beforeAll(() => {
  console.log('Starting MSW server...');
  server.listen({ onUnhandledRequest: 'error' });
  console.log('MSW server started');
});

// Reset any request handlers that we may add during the tests,
// so they don't affect other tests
afterEach(() => {
  server.resetHandlers();
  vi.clearAllMocks();
});

// Clean up after all tests are done
afterAll(() => {
  console.log('Closing MSW server...');
  server.close();
  console.log('MSW server closed');
});