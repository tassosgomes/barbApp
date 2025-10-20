import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TokenManager, UserType } from '../tokenManager';

describe('TokenManager', () => {
  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear();
    // Clear console mocks
    vi.clearAllMocks();
  });

  describe('setToken', () => {
    it('should store token for barbeiro', () => {
      const token = 'barbeiro-token-123';
      TokenManager.setToken(UserType.BARBEIRO, token);
      
      expect(localStorage.getItem('barbapp-barber-token')).toBe(token);
    });

    it('should store token for admin barbearia', () => {
      const token = 'admin-barbearia-token-123';
      TokenManager.setToken(UserType.ADMIN_BARBEARIA, token);
      
      expect(localStorage.getItem('admin_barbearia_token')).toBe(token);
    });

    it('should store token for admin central', () => {
      const token = 'admin-central-token-123';
      TokenManager.setToken(UserType.ADMIN_CENTRAL, token);
      
      expect(localStorage.getItem('auth_token')).toBe(token);
    });

    it('should clear all tokens before setting new one', () => {
      // Set multiple tokens
      localStorage.setItem('barbapp-barber-token', 'old-barber-token');
      localStorage.setItem('admin_barbearia_token', 'old-admin-token');
      localStorage.setItem('auth_token', 'old-central-token');
      
      // Set new token
      TokenManager.setToken(UserType.BARBEIRO, 'new-token');
      
      // Only barbeiro token should exist
      expect(localStorage.getItem('barbapp-barber-token')).toBe('new-token');
      expect(localStorage.getItem('admin_barbearia_token')).toBeNull();
      expect(localStorage.getItem('auth_token')).toBeNull();
    });
  });

  describe('getToken', () => {
    it('should return token for barbeiro', () => {
      const token = 'barbeiro-token-123';
      localStorage.setItem('barbapp-barber-token', token);
      
      expect(TokenManager.getToken(UserType.BARBEIRO)).toBe(token);
    });

    it('should return null when token does not exist', () => {
      expect(TokenManager.getToken(UserType.BARBEIRO)).toBeNull();
    });
  });

  describe('removeToken', () => {
    it('should remove token for barbeiro', () => {
      localStorage.setItem('barbapp-barber-token', 'token-123');
      TokenManager.removeToken(UserType.BARBEIRO);
      
      expect(localStorage.getItem('barbapp-barber-token')).toBeNull();
    });
  });

  describe('setContext', () => {
    it('should store context for admin barbearia', () => {
      const context = {
        id: 'barbearia-123',
        nome: 'Barbearia Teste',
        codigo: 'ABC123',
        isActive: true,
      };
      
      TokenManager.setContext(UserType.ADMIN_BARBEARIA, context);
      
      const stored = localStorage.getItem('admin_barbearia_context');
      expect(stored).not.toBeNull();
      expect(JSON.parse(stored!)).toEqual(context);
    });
  });

  describe('getContext', () => {
    it('should return context for admin barbearia', () => {
      const context = {
        id: 'barbearia-123',
        nome: 'Barbearia Teste',
      };
      
      localStorage.setItem('admin_barbearia_context', JSON.stringify(context));
      
      const retrieved = TokenManager.getContext(UserType.ADMIN_BARBEARIA);
      expect(retrieved).toEqual(context);
    });

    it('should return null when context does not exist', () => {
      expect(TokenManager.getContext(UserType.ADMIN_BARBEARIA)).toBeNull();
    });

    it('should return null when context is invalid JSON', () => {
      localStorage.setItem('admin_barbearia_context', 'invalid-json{');
      
      const retrieved = TokenManager.getContext(UserType.ADMIN_BARBEARIA);
      expect(retrieved).toBeNull();
    });
  });

  describe('removeContext', () => {
    it('should remove context for admin barbearia', () => {
      localStorage.setItem('admin_barbearia_context', '{"id":"123"}');
      TokenManager.removeContext(UserType.ADMIN_BARBEARIA);
      
      expect(localStorage.getItem('admin_barbearia_context')).toBeNull();
    });
  });

  describe('clearAllTokens', () => {
    it('should remove all authentication tokens and contexts', () => {
      // Set all possible tokens
      localStorage.setItem('barbapp-barber-token', 'token1');
      localStorage.setItem('admin_barbearia_token', 'token2');
      localStorage.setItem('auth_token', 'token3');
      localStorage.setItem('admin_barbearia_context', '{"id":"123"}');
      localStorage.setItem('authToken', 'legacy-token');
      localStorage.setItem('other-key', 'should-remain');
      
      TokenManager.clearAllTokens();
      
      // Auth tokens should be removed
      expect(localStorage.getItem('barbapp-barber-token')).toBeNull();
      expect(localStorage.getItem('admin_barbearia_token')).toBeNull();
      expect(localStorage.getItem('auth_token')).toBeNull();
      expect(localStorage.getItem('admin_barbearia_context')).toBeNull();
      expect(localStorage.getItem('authToken')).toBeNull();
      
      // Other keys should remain
      expect(localStorage.getItem('other-key')).toBe('should-remain');
    });
  });

  describe('logout', () => {
    it('should remove token for barbeiro', () => {
      localStorage.setItem('barbapp-barber-token', 'token-123');
      TokenManager.logout(UserType.BARBEIRO);
      
      expect(localStorage.getItem('barbapp-barber-token')).toBeNull();
    });

    it('should remove token and context for admin barbearia', () => {
      localStorage.setItem('admin_barbearia_token', 'token-123');
      localStorage.setItem('admin_barbearia_context', '{"id":"123"}');
      
      TokenManager.logout(UserType.ADMIN_BARBEARIA);
      
      expect(localStorage.getItem('admin_barbearia_token')).toBeNull();
      expect(localStorage.getItem('admin_barbearia_context')).toBeNull();
    });

    it('should only remove token for admin central (no context)', () => {
      localStorage.setItem('auth_token', 'token-123');
      TokenManager.logout(UserType.ADMIN_CENTRAL);
      
      expect(localStorage.getItem('auth_token')).toBeNull();
    });
  });

  describe('hasConflictingTokens', () => {
    it('should return false when no tokens exist', () => {
      expect(TokenManager.hasConflictingTokens()).toBe(false);
    });

    it('should return false when only one token exists', () => {
      localStorage.setItem('barbapp-barber-token', 'token-123');
      expect(TokenManager.hasConflictingTokens()).toBe(false);
    });

    it('should return true when multiple tokens exist', () => {
      const consoleWarnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
      
      localStorage.setItem('barbapp-barber-token', 'token-1');
      localStorage.setItem('auth_token', 'token-2');
      
      expect(TokenManager.hasConflictingTokens()).toBe(true);
      expect(consoleWarnSpy).toHaveBeenCalledWith(
        expect.stringContaining('CONFLITO DETECTADO!')
      );
      
      consoleWarnSpy.mockRestore();
    });
  });

  describe('getCurrentUserType', () => {
    it('should return null when no token exists', () => {
      expect(TokenManager.getCurrentUserType()).toBeNull();
    });

    it('should return BARBEIRO when barber token exists', () => {
      localStorage.setItem('barbapp-barber-token', 'token-123');
      expect(TokenManager.getCurrentUserType()).toBe(UserType.BARBEIRO);
    });

    it('should return ADMIN_BARBEARIA when admin barbearia token exists', () => {
      localStorage.setItem('admin_barbearia_token', 'token-123');
      expect(TokenManager.getCurrentUserType()).toBe(UserType.ADMIN_BARBEARIA);
    });

    it('should return ADMIN_CENTRAL when central token exists', () => {
      localStorage.setItem('auth_token', 'token-123');
      expect(TokenManager.getCurrentUserType()).toBe(UserType.ADMIN_CENTRAL);
    });

    it('should clear all tokens and return null when conflict detected', () => {
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      localStorage.setItem('barbapp-barber-token', 'token-1');
      localStorage.setItem('auth_token', 'token-2');
      
      const result = TokenManager.getCurrentUserType();
      
      expect(result).toBeNull();
      expect(localStorage.getItem('barbapp-barber-token')).toBeNull();
      expect(localStorage.getItem('auth_token')).toBeNull();
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        expect.stringContaining('Múltiplos tokens detectados!')
      );
      
      consoleErrorSpy.mockRestore();
    });
  });

  describe('validateAuthState', () => {
    it('should clear conflicting tokens', () => {
      const consoleWarnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
      
      localStorage.setItem('barbapp-barber-token', 'token-1');
      localStorage.setItem('auth_token', 'token-2');
      
      TokenManager.validateAuthState();
      
      expect(localStorage.getItem('barbapp-barber-token')).toBeNull();
      expect(localStorage.getItem('auth_token')).toBeNull();
      expect(consoleWarnSpy).toHaveBeenCalled();
      
      consoleWarnSpy.mockRestore();
    });

    it('should remove orphaned context without token', () => {
      const consoleWarnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
      
      localStorage.setItem('admin_barbearia_context', '{"id":"123"}');
      
      TokenManager.validateAuthState();
      
      expect(localStorage.getItem('admin_barbearia_context')).toBeNull();
      expect(consoleWarnSpy).toHaveBeenCalledWith(
        expect.stringContaining('Contexto órfão detectado!')
      );
      
      consoleWarnSpy.mockRestore();
    });

    it('should keep valid context with token', () => {
      localStorage.setItem('admin_barbearia_token', 'token-123');
      localStorage.setItem('admin_barbearia_context', '{"id":"123"}');
      
      TokenManager.validateAuthState();
      
      expect(localStorage.getItem('admin_barbearia_token')).toBe('token-123');
      expect(localStorage.getItem('admin_barbearia_context')).toBe('{"id":"123"}');
    });
  });
});
