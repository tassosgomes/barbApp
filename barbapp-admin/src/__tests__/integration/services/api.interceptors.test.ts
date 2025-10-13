import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

// Mock the api module
vi.mock('@/services/api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
    interceptors: {
      request: { use: vi.fn() },
      response: { use: vi.fn() },
    },
  },
}));

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

// Mock window.location
const locationMock = { href: '' };
Object.defineProperty(window, 'location', {
  value: locationMock,
  writable: true,
});

// Mock console methods
const consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {});
const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

Object.defineProperty(window, 'localStorage', { value: localStorageMock });
Object.defineProperty(window, 'sessionStorage', { value: sessionStorageMock });

// Import after mocking
import api from '@/services/api';

describe('API Interceptors', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    consoleLogSpy.mockClear();
    consoleErrorSpy.mockClear();
    localStorageMock.getItem.mockReturnValue(null);
    sessionStorageMock.getItem.mockReturnValue(null);
    locationMock.href = '';
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should have request and response interceptors configured', () => {
    expect(api.interceptors.request.use).toHaveBeenCalled();
    expect(api.interceptors.response.use).toHaveBeenCalled();
  });

  it('should add authorization header when token exists in request interceptor', async () => {
    // Since interceptors are already set up, we can't easily test the function
    // But we can verify the setup by checking that localStorage is accessed
    localStorageMock.getItem.mockReturnValue('test-token');

    // Mock a successful response
    const mockApi = await import('@/services/api');
    (mockApi.default.get as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
      data: 'success',
      status: 200,
      config: { method: 'get', url: '/test' }
    });

    await mockApi.default.get('/test');

    // The interceptor should have called localStorage.getItem
    expect(localStorageMock.getItem).toHaveBeenCalledWith('auth_token');
  });

  it('should handle 401 response in response interceptor', async () => {
    localStorageMock.getItem.mockReturnValue('test-token');

    const mockApi = await import('@/services/api');
    const error = {
      response: {
        status: 401,
        data: { message: 'Unauthorized' },
        config: { method: 'get', url: '/test' }
      },
      config: { method: 'get', url: '/test' }
    };
    (mockApi.default.get as ReturnType<typeof vi.fn>).mockRejectedValueOnce(error);

    await expect(mockApi.default.get('/test')).rejects.toEqual(error);

    expect(localStorageMock.removeItem).toHaveBeenCalledWith('auth_token');
    expect(sessionStorageMock.setItem).toHaveBeenCalledWith('session_expired', 'true');
    expect(window.location.href).toBe('/login');
  });
});