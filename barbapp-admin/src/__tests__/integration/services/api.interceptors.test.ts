import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

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
    // The interceptors are configured when the module is imported
    // We can verify this by checking that the api instance exists and has interceptors
    expect(api).toBeDefined();
    expect(api.interceptors).toBeDefined();
    expect(api.interceptors.request).toBeDefined();
    expect(api.interceptors.response).toBeDefined();
  });

  it('should add authorization header when token exists in request interceptor', async () => {
    localStorageMock.getItem.mockReturnValue('test-token');

    // Mock axios request to capture the config
    const mockAxios = await import('axios');
    const originalCreate = mockAxios.default.create;
    const mockInstance = {
      interceptors: { request: { use: vi.fn() }, response: { use: vi.fn() } },
      get: vi.fn().mockResolvedValue({ data: 'success', status: 200, config: { method: 'get', url: '/test' } }),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn(),
    };

    // Mock the create function to return our mock instance
    mockAxios.default.create = vi.fn().mockReturnValue(mockInstance);

    // Re-import to get the new instance
    const apiModule = await import('@/services/api');
    const testApi = apiModule.default;

    await testApi.get('/test');

    // Restore original create
    mockAxios.default.create = originalCreate;

    // The interceptor should have called localStorage.getItem
    expect(localStorageMock.getItem).toHaveBeenCalledWith('auth_token');
  });

  it('should handle 401 response in response interceptor', async () => {
    localStorageMock.getItem.mockReturnValue('test-token');

    // Create a mock error for 401
    const error401 = {
      response: {
        status: 401,
        data: { message: 'Unauthorized' },
        config: { method: 'get', url: '/test' }
      },
      config: { method: 'get', url: '/test' }
    };

    // Mock axios to reject with 401 error
    const mockAxios = await import('axios');
    const originalCreate = mockAxios.default.create;
    const mockInstance = {
      interceptors: { request: { use: vi.fn() }, response: { use: vi.fn() } },
      get: vi.fn().mockRejectedValue(error401),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn(),
    };

    mockAxios.default.create = vi.fn().mockReturnValue(mockInstance);

    // Re-import to get the new instance
    const apiModule = await import('@/services/api');
    const testApi = apiModule.default;

    await expect(testApi.get('/test')).rejects.toEqual(error401);

    // Restore original create
    mockAxios.default.create = originalCreate;

    expect(localStorageMock.removeItem).toHaveBeenCalledWith('auth_token');
    expect(sessionStorageMock.setItem).toHaveBeenCalledWith('session_expired', 'true');
    expect(window.location.href).toBe('/login');
  });

  it('should log requests and responses', async () => {
    // Test request logging
    const mockAxios = await import('axios');
    const originalCreate = mockAxios.default.create;
    const mockInstance = {
      interceptors: { request: { use: vi.fn() }, response: { use: vi.fn() } },
      get: vi.fn().mockResolvedValue({
        data: 'success',
        status: 200,
        config: { method: 'get', url: '/test' }
      }),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn(),
    };

    mockAxios.default.create = vi.fn().mockReturnValue(mockInstance);

    // Re-import to get the new instance
    const apiModule = await import('@/services/api');
    const testApi = apiModule.default;

    await testApi.get('/test');

    // Restore original create
    mockAxios.default.create = originalCreate;

    // Check that console.log was called for request and response
    expect(consoleLogSpy).toHaveBeenCalledWith('API Request: GET /test');
    expect(consoleLogSpy).toHaveBeenCalledWith('API Response: 200 GET /test');
  });
});