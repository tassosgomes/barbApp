/**
 * Cliente HTTP configurado para usar variáveis de ambiente
 * 
 * Este arquivo demonstra como usar as configurações de ambiente
 * que serão substituídas em tempo de execução no Docker.
 */

import axios, { AxiosInstance, AxiosResponse, AxiosError } from 'axios';
import { config } from '@/config/env';

/**
 * Instância principal do cliente HTTP
 */
export const api: AxiosInstance = axios.create({
  baseURL: config.api.baseURL,
  timeout: config.api.timeout,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Interceptor de requisição
 */
api.interceptors.request.use(
  (config) => {
    // Adicionar token de autenticação se disponível
    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Log em desenvolvimento
    if (import.meta.env.DEV) {
      console.log(`🚀 API Request: ${config.method?.toUpperCase()} ${config.url}`);
    }

    return config;
  },
  (error) => {
    console.error('❌ Request error:', error);
    return Promise.reject(error);
  }
);

/**
 * Interceptor de resposta
 */
api.interceptors.response.use(
  (response: AxiosResponse) => {
    // Log em desenvolvimento
    if (import.meta.env.DEV) {
      console.log(`✅ API Response: ${response.status} ${response.config.url}`);
    }

    return response;
  },
  (error: AxiosError) => {
    // Log do erro
    console.error('❌ API Error:', {
      status: error.response?.status,
      message: error.message,
      url: error.config?.url,
    });

    // Tratar erros específicos
    if (error.response?.status === 401) {
      // Token expirado ou inválido
      localStorage.removeItem('auth_token');
      window.location.href = '/login';
    }

    return Promise.reject(error);
  }
);

/**
 * Utilitários para diferentes endpoints
 */
export const apiEndpoints = {
  auth: {
    login: () => `${config.api.endpoints.auth}/login`,
    logout: () => `${config.api.endpoints.auth}/logout`,
    me: () => `${config.api.endpoints.auth}/me`,
  },
  
  barbershops: {
    list: () => `${config.api.endpoints.barbershops}`,
    get: (id: string) => `${config.api.endpoints.barbershops}/${id}`,
    create: () => `${config.api.endpoints.barbershops}`,
    update: (id: string) => `${config.api.endpoints.barbershops}/${id}`,
    delete: (id: string) => `${config.api.endpoints.barbershops}/${id}`,
  },
  
  landingPages: {
    list: () => `${config.api.endpoints.landingPages}`,
    get: (id: string) => `${config.api.endpoints.landingPages}/${id}`,
    create: () => `${config.api.endpoints.landingPages}`,
    update: (id: string) => `${config.api.endpoints.landingPages}/${id}`,
    uploadLogo: (id: string) => `${config.api.endpoints.landingPages}/${id}/logo`,
    deleteLogo: (id: string) => `${config.api.endpoints.landingPages}/${id}/logo`,
  },
  
  uploads: {
    logo: () => `${config.api.endpoints.uploads}/logo`,
    image: () => `${config.api.endpoints.uploads}/image`,
  },
};

/**
 * Função helper para verificar se a API está disponível
 */
export async function checkApiHealth(): Promise<boolean> {
  try {
    const response = await api.get('/health');
    return response.status === 200;
  } catch {
    return false;
  }
}

/**
 * Log da configuração da API (apenas em desenvolvimento)
 */
if (import.meta.env.DEV) {
  console.log('🔧 API Configuration:', {
    baseURL: config.api.baseURL,
    timeout: config.api.timeout,
    endpoints: config.api.endpoints,
  });
}