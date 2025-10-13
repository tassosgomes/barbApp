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
 * Aplicar máscara de documento (CPF/CNPJ)
 * CPF: 999.999.999-99
 * CNPJ: 99.999.999/9999-99
 */
export function applyDocumentMask(value: string): string {
  const cleaned = value.replace(/\D/g, '');

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