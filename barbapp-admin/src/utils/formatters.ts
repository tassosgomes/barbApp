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
 * Aplicar máscara de CEP 99999-999
 */
export function applyZipCodeMask(value: string): string {
  const cleaned = value.replace(/\D/g, '');

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