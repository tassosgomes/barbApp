/**
 * Aplicar máscara de telefone (99) 99999-9999
 */
export function applyPhoneMask(value: string): string {
  const cleaned = value.replace(/\D/g, '');

  if (cleaned.length === 0) return '';

  // Always start with area code
  let formatted = `(${cleaned.slice(0, 2)}`;

  if (cleaned.length > 2) {
    formatted += `) ${cleaned.slice(2, 7)}`;
  }

  if (cleaned.length > 7) {
    formatted += `-${cleaned.slice(7, 11)}`;
  }

  return formatted;
}

/**
 * Remover máscara de telefone - retorna apenas números
 * Exemplo: "(11) 98765-4321" -> "11987654321"
 */
export function removePhoneMask(value: string): string {
  return value.replace(/\D/g, '');
}

/**
 * Aplicar máscara de CEP 99999-999
 */
export function applyZipCodeMask(value: string): string {
  const cleaned = value.replace(/\D/g, '').slice(0, 8); // Limit to 8 digits

  if (cleaned.length >= 8) {
    const match = cleaned.match(/^(\d{5})(\d{3})$/);
    if (match) {
      return `${match[1]}-${match[2]}`;
    }
  } else if (cleaned.length >= 5) {
    const match = cleaned.match(/^(\d{5})(\d{0,3})$/);
    if (match) {
      return `${match[1]}${match[2] ? `-${match[2]}` : ''}`;
    }
  }

  return cleaned;
}

/**
 * Aplicar máscara de documento (CPF/CNPJ)
 * CPF: 999.999.999-99
 * CNPJ: 99.999.999/9999-99
 */
export function applyDocumentMask(value: string): string {
  const cleaned = value.replace(/\D/g, '').slice(0, 14); // Limit to 14 digits (max CNPJ)

  if (cleaned.length <= 11) {
    // CPF format
    if (cleaned.length >= 9) {
      const match = cleaned.match(/^(\d{3})(\d{3})(\d{3})(\d{0,2})$/);
      if (match) {
        return `${match[1]}.${match[2]}.${match[3]}${match[4] ? `-${match[4]}` : ''}`;
      }
    } else if (cleaned.length >= 6) {
      const match = cleaned.match(/^(\d{3})(\d{3})(\d{0,3})$/);
      if (match) {
        return `${match[1]}.${match[2]}${match[3] ? `.${match[3]}` : ''}`;
      }
    } else if (cleaned.length >= 3) {
      const match = cleaned.match(/^(\d{3})(\d{0,3})$/);
      if (match) {
        return `${match[1]}${match[2] ? `.${match[2]}` : ''}`;
      }
    }
  } else {
    // CNPJ format
    if (cleaned.length >= 12) {
      const match = cleaned.match(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{0,2})$/);
      if (match) {
        return `${match[1]}.${match[2]}.${match[3]}/${match[4]}${match[5] ? `-${match[5]}` : ''}`;
      }
    } else if (cleaned.length >= 8) {
      const match = cleaned.match(/^(\d{2})(\d{3})(\d{3})(\d{0,4})$/);
      if (match) {
        return `${match[1]}.${match[2]}.${match[3]}${match[4] ? `/${match[4]}` : ''}`;
      }
    } else if (cleaned.length >= 5) {
      const match = cleaned.match(/^(\d{2})(\d{3})(\d{0,3})$/);
      if (match) {
        return `${match[1]}.${match[2]}${match[3] ? `.${match[3]}` : ''}`;
      }
    } else if (cleaned.length >= 2) {
      const match = cleaned.match(/^(\d{2})(\d{0,3})$/);
      if (match) {
        return `${match[1]}${match[2] ? `.${match[2]}` : ''}`;
      }
    }
  }

  return cleaned;
}

/**
 * Formatar data ISO para pt-BR
 */
export function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

/**
 * Formatar valor monetário em Real Brasileiro (R$)
 * 
 * @param value - Valor numérico a ser formatado
 * @returns String formatada como moeda brasileira
 * 
 * @example
 * formatCurrency(35) // "R$ 35,00"
 * formatCurrency(50.5) // "R$ 50,50"
 * formatCurrency(1234.56) // "R$ 1.234,56"
 */
export function formatCurrency(value: number): string {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(value);
}

/**
 * Formatar duração em minutos para formato legível (horas e minutos)
 * 
 * @param minutes - Duração em minutos
 * @returns String formatada no estilo "1h 30min" ou "45min"
 * 
 * @example
 * formatDuration(30) // "30min"
 * formatDuration(60) // "1h"
 * formatDuration(90) // "1h 30min"
 * formatDuration(150) // "2h 30min"
 */
export function formatDuration(minutes: number): string {
  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;

  if (hours > 0 && mins > 0) {
    return `${hours}h ${mins}min`;
  }

  if (hours > 0) {
    return `${hours}h`;
  }

  return `${mins}min`;
}