import axios from 'axios';
import { TokenManager } from './tokenManager';

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
 * IMPORTANTE: Usa TokenManager para obter o token correto baseado no tipo de usuário autenticado
 */
api.interceptors.request.use(
  (config) => {
    // Detect current user type and get appropriate token
    const currentUserType = TokenManager.getCurrentUserType();
    
    if (currentUserType) {
      const token = TokenManager.getToken(currentUserType);
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
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
 * 
 * IMPORTANTE: Usa TokenManager para limpeza de tokens
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
      // Detect current user type and handle logout accordingly
      const currentUserType = TokenManager.getCurrentUserType();
      
      if (currentUserType) {
        // Clear tokens using TokenManager
        TokenManager.logout(currentUserType);
        sessionStorage.setItem('session_expired', 'true');
        
        // Redirect based on user type
        if (!window.location.pathname.includes('/login')) {
          // Extract codigo from path for Admin Barbearia
          const path = window.location.pathname;
          const codigoMatch = path.match(/^\/([A-Z0-9]+)\//);
          
          if (codigoMatch) {
            // Admin Barbearia with codigo
            window.location.href = `/${codigoMatch[1]}/login`;
          } else if (path.startsWith('/barber')) {
            // Barbeiro
            window.location.href = '/login';
          } else {
            // Admin Central
            window.location.href = '/admin/login';
          }
        }
      }
    }

    // TODO: Future - Implement token refresh logic for 401 errors when token is expired but refresh token is available

    return Promise.reject(error);
  }
);

export default api;