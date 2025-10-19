import axios from 'axios';

/**
 * Axios instance configured with base URL, interceptors for authentication and error handling
 */
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Request interceptor: Add JWT token to all requests and log requests
 */
api.interceptors.request.use(
  (config) => {
    // Determine which token to use based on the current route
    const path = window.location.pathname;
    const isAdminBarbearia = path.match(/^\/[A-Z0-9]+\//); // Pattern: /{CODIGO}/...
    
    // Select appropriate token key
    const tokenKey = isAdminBarbearia ? 'admin_barbearia_token' : 'auth_token';
    const token = localStorage.getItem(tokenKey);
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Log request
    console.log(`API Request: ${config.method?.toUpperCase()} ${config.url}`);
    console.log(`Full URL: ${config.baseURL}${config.url}`);

    return config;
  },
  (error) => {
    console.error('API Request Error:', error);
    return Promise.reject(error);
  }
);

/**
 * Response interceptor: Handle global errors and log responses
 * - 401: Clear token, set session expiry flag, and redirect to login
 * - Other errors: Pass through for component-level handling
 */
api.interceptors.response.use(
  (response) => {
    // Log successful response
    console.log(`API Response: ${response.status} ${response.config.method?.toUpperCase()} ${response.config.url}`);
    return response;
  },
  (error) => {
    // Log error response
    if (error.response) {
      console.error(`API Error Response: ${error.response.status} ${error.config?.method?.toUpperCase()} ${error.config?.url}`, error.response.data);
    } else if (error.request) {
      console.error('API Network Error:', error.message);
    } else {
      console.error('API Error:', error.message);
    }

    if (error.response?.status === 401) {
      // Determine which token to clear and where to redirect based on current route
      const path = window.location.pathname;
      const isAdminBarbearia = path.match(/^\/([A-Z0-9]+)\//);
      
      if (isAdminBarbearia) {
        // Admin Barbearia: clear specific token and redirect to barbearia login
        const codigo = isAdminBarbearia[1];
        localStorage.removeItem('admin_barbearia_token');
        sessionStorage.setItem('session_expired', 'true');
        window.location.href = `/${codigo}/login`;
      } else {
        // Admin Central: clear central token and redirect to central login
        localStorage.removeItem('auth_token');
        sessionStorage.setItem('session_expired', 'true');
        window.location.href = '/login';
      }
    }

    // TODO: Future - Implement token refresh logic for 401 errors when token is expired but refresh token is available

    return Promise.reject(error);
  }
);

export default api;