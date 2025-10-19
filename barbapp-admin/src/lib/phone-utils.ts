/**
 * Utilitários para formatação e conversão de números de telefone brasileiros
 */

/**
 * Aplica máscara de telefone brasileiro durante a digitação
 * Formato: (XX) XXXXX-XXXX
 * 
 * @param value - Valor digitado pelo usuário (pode conter caracteres não numéricos)
 * @returns String formatada com máscara aplicada
 * 
 * @example
 * applyPhoneMask('11999999999') // retorna '(11) 99999-9999'
 * applyPhoneMask('119999') // retorna '(11) 9999'
 */
export function applyPhoneMask(value: string): string {
  // Remove todos os caracteres não numéricos e limita a 11 dígitos
  const digits = value.replace(/\D/g, '').slice(0, 11);
  
  // Aplica a máscara progressivamente conforme o usuário digita
  if (digits.length <= 2) {
    return digits;
  }
  
  if (digits.length <= 7) {
    return `(${digits.slice(0, 2)}) ${digits.slice(2)}`;
  }
  
  return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`;
}

/**
 * Converte telefone com máscara para formato aceito pela API
 * Formato API: +55XXXXXXXXXXX (código do país + DDD + número)
 * 
 * @param phone - Telefone formatado com máscara (XX) XXXXX-XXXX
 * @returns String no formato internacional +55XXXXXXXXXXX
 * 
 * @example
 * formatPhoneToAPI('(11) 99999-9999') // retorna '+5511999999999'
 */
export function formatPhoneToAPI(phone: string): string {
  // Remove todos os caracteres não numéricos
  const digitsOnly = phone.replace(/\D/g, '');
  
  // Adiciona código do país (+55 para Brasil)
  return `+55${digitsOnly}`;
}
