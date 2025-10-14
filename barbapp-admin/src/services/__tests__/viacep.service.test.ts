import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { fetchAddressByCep, ViaCepError } from '../viacep.service';

describe('ViaCEP Service', () => {
  const mockFetch = vi.fn();

  beforeEach(() => {
    globalThis.fetch = mockFetch;
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('fetchAddressByCep', () => {
    it('should fetch address successfully with valid CEP', async () => {
      const mockResponse = {
        cep: '01310-100',
        logradouro: 'Avenida Paulista',
        complemento: '',
        bairro: 'Bela Vista',
        localidade: 'São Paulo',
        uf: 'SP',
        ibge: '3550308',
        gia: '1004',
        ddd: '11',
        siafi: '7107',
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      const result = await fetchAddressByCep('01310-100');

      expect(result).toEqual({
        street: 'Avenida Paulista',
        neighborhood: 'Bela Vista',
        city: 'São Paulo',
        state: 'SP',
        cep: '01310-100',
      });

      expect(mockFetch).toHaveBeenCalledWith('https://viacep.com.br/ws/01310100/json/');
    });

    it('should handle CEP with mask', async () => {
      const mockResponse = {
        cep: '01310-100',
        logradouro: 'Avenida Paulista',
        complemento: '',
        bairro: 'Bela Vista',
        localidade: 'São Paulo',
        uf: 'SP',
        ibge: '3550308',
        gia: '1004',
        ddd: '11',
        siafi: '7107',
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      await fetchAddressByCep('01310-100');

      expect(mockFetch).toHaveBeenCalledWith('https://viacep.com.br/ws/01310100/json/');
    });

    it('should throw error for invalid CEP format', async () => {
      await expect(fetchAddressByCep('123')).rejects.toThrow(ViaCepError);
      await expect(fetchAddressByCep('123')).rejects.toThrow('CEP inválido. Deve conter 8 dígitos.');
    });

    it('should throw error for CEP with letters', async () => {
      await expect(fetchAddressByCep('abcd-efgh')).rejects.toThrow(ViaCepError);
    });

    it('should throw error when CEP not found', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ erro: true }),
      });

      await expect(fetchAddressByCep('99999-999')).rejects.toThrow(ViaCepError);
      await expect(fetchAddressByCep('99999-999')).rejects.toThrow('CEP não encontrado.');
    });

    it('should throw error on network failure', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
      });

      await expect(fetchAddressByCep('01310-100')).rejects.toThrow(ViaCepError);
      await expect(fetchAddressByCep('01310-100')).rejects.toThrow('Erro ao buscar CEP. Tente novamente.');
    });

    it('should throw error on fetch exception', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network error'));

      await expect(fetchAddressByCep('01310-100')).rejects.toThrow(ViaCepError);
      await expect(fetchAddressByCep('01310-100')).rejects.toThrow('Erro ao buscar CEP. Verifique sua conexão.');
    });

    it('should handle empty CEP', async () => {
      await expect(fetchAddressByCep('')).rejects.toThrow(ViaCepError);
    });

    it('should handle CEP with only spaces', async () => {
      await expect(fetchAddressByCep('     ')).rejects.toThrow(ViaCepError);
    });
  });
});
