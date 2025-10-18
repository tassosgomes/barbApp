import { render, screen, act, renderHook } from '@testing-library/react';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { BarbeariaProvider, useBarbearia } from '../BarbeariaContext';

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

// Mock window.addEventListener and removeEventListener for storage events
const addEventListenerMock = vi.fn();
const removeEventListenerMock = vi.fn();
Object.defineProperty(window, 'addEventListener', {
  value: addEventListenerMock,
});
Object.defineProperty(window, 'removeEventListener', {
  value: removeEventListenerMock,
});

describe('BarbeariaContext', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorageMock.clear();
  });

  describe('BarbeariaProvider', () => {
    it('should provide barbearia data to children', () => {
      const TestComponent = () => {
        const { barbearia, setBarbearia } = useBarbearia();

        return (
          <div>
            <span>{barbearia?.nome ?? 'No barbearia'}</span>
            <button
              onClick={() =>
                setBarbearia({
                  barbeariaId: '123',
                  nome: 'Test Barbearia',
                  codigo: 'TEST1234',
                  isActive: true,
                })
              }
            >
              Set
            </button>
          </div>
        );
      };

      render(
        <BarbeariaProvider>
          <TestComponent />
        </BarbeariaProvider>
      );

      expect(screen.getByText('No barbearia')).toBeInTheDocument();

      act(() => {
        screen.getByText('Set').click();
      });

      expect(screen.getByText('Test Barbearia')).toBeInTheDocument();
    });

    it('should persist to localStorage', () => {
      const TestComponent = () => {
        const { setBarbearia } = useBarbearia();

        return (
          <button
            onClick={() =>
              setBarbearia({
                barbeariaId: '123',
                nome: 'Test',
                codigo: 'TEST1234',
                isActive: true,
              })
            }
          >
            Set
          </button>
        );
      };

      render(
        <BarbeariaProvider>
          <TestComponent />
        </BarbeariaProvider>
      );

      act(() => {
        screen.getByText('Set').click();
      });

      expect(localStorageMock.setItem).toHaveBeenCalledWith(
        'admin_barbearia_context',
        JSON.stringify({
          id: '123',
          nome: 'Test',
          codigo: 'TEST1234',
          isActive: true,
        })
      );
    });

    it('should clear barbearia on logout', () => {
      const TestComponent = () => {
        const { barbearia, setBarbearia, clearBarbearia } = useBarbearia();

        return (
          <div>
            <span>{barbearia?.nome ?? 'No barbearia'}</span>
            <button
              onClick={() =>
                setBarbearia({
                  barbeariaId: '123',
                  nome: 'Test Barbearia',
                  codigo: 'TEST1234',
                  isActive: true,
                })
              }
            >
              Set
            </button>
            <button onClick={clearBarbearia}>Clear</button>
          </div>
        );
      };

      render(
        <BarbeariaProvider>
          <TestComponent />
        </BarbeariaProvider>
      );

      // Set barbearia first
      act(() => {
        screen.getByText('Set').click();
      });

      expect(screen.getByText('Test Barbearia')).toBeInTheDocument();

      // Clear barbearia
      act(() => {
        screen.getByText('Clear').click();
      });

      expect(screen.getByText('No barbearia')).toBeInTheDocument();
      expect(localStorageMock.removeItem).toHaveBeenCalledWith('admin_barbearia_context');
    });

    it('should handle invalid localStorage data gracefully', () => {
      localStorageMock.getItem.mockReturnValue('invalid json');

      const TestComponent = () => {
        const { barbearia, isLoaded } = useBarbearia();

        if (!isLoaded) return <div>Loading...</div>;

        return <span>{barbearia?.nome ?? 'No barbearia'}</span>;
      };

      // Mock console.error to avoid test output pollution
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      render(
        <BarbeariaProvider>
          <TestComponent />
        </BarbeariaProvider>
      );

      expect(screen.getByText('No barbearia')).toBeInTheDocument();
      expect(localStorageMock.removeItem).toHaveBeenCalledWith('admin_barbearia_context');
      expect(consoleErrorSpy).toHaveBeenCalled();

      consoleErrorSpy.mockRestore();
    });

    it('should add and remove storage event listeners', () => {
      const TestComponent = () => <div>Test</div>;

      const { unmount } = render(
        <BarbeariaProvider>
          <TestComponent />
        </BarbeariaProvider>
      );

      expect(addEventListenerMock).toHaveBeenCalledWith('storage', expect.any(Function));

      unmount();

      expect(removeEventListenerMock).toHaveBeenCalledWith('storage', expect.any(Function));
    });
  });

  describe('useBarbearia hook', () => {
    it('should return context value when used within provider', () => {
      const { result } = renderHook(() => useBarbearia(), {
        wrapper: BarbeariaProvider,
      });

      expect(result.current).toHaveProperty('barbearia');
      expect(result.current).toHaveProperty('setBarbearia');
      expect(result.current).toHaveProperty('clearBarbearia');
      expect(result.current).toHaveProperty('isLoaded');
      expect(typeof result.current.setBarbearia).toBe('function');
      expect(typeof result.current.clearBarbearia).toBe('function');
    });
  });
});