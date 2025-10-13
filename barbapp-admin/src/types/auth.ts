/**
 * Login credentials for admin authentication
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Authentication response containing token and user info
 */
export interface LoginResponse {
  token: string;
  user: User;
}

/**
 * Authenticated user information
 */
export interface User {
  id: string;
  email: string;
  name: string;
}
