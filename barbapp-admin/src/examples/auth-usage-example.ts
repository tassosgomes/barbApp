/**
 * Exemplo de uso dos tipos, schemas e utilitários de autenticação
 * Este arquivo demonstra como os componentes criados na Task 1.0 serão utilizados
 */

import type { LoginInput, AuthResponse, AuthContextType } from '@/types/auth.types';
import { barberLoginSchema, type BarberLoginFormData } from '@/schemas/login.schema';
import { applyPhoneMask, formatPhoneToAPI } from '@/lib/phone-utils';

// Exemplo 1: Validação de dados de login com Zod
function validateLoginData(data: unknown): BarberLoginFormData | null {
  try {
    const validated = barberLoginSchema.parse(data);
    console.log('✅ Dados válidos:', validated);
    return validated;
  } catch (error) {
    console.error('❌ Erro de validação:', error);
    return null;
  }
}

// Exemplo 2: Uso das funções de máscara de telefone
function demonstratePhoneMask() {
  const rawInput = '11999999999';
  const masked = applyPhoneMask(rawInput);
  const apiFormat = formatPhoneToAPI(masked);
  
  console.log('📱 Conversão de telefone:');
  console.log('  Input do usuário:', rawInput);
  console.log('  Com máscara (UI):', masked);
  console.log('  Formato API:', apiFormat);
}

// Exemplo 3: Tipagem de função de login
async function exampleLoginFunction(data: LoginInput): Promise<AuthResponse> {
  // Esta função seria implementada no auth.service.ts
  // Aqui apenas demonstramos a tipagem
  const response = await fetch('/api/auth/barbeiro/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      barbershopCode: data.barbershopCode,
      phone: formatPhoneToAPI(data.phone),
    }),
  });
  
  return response.json() as Promise<AuthResponse>;
}

// Exemplo 4: Tipagem de contexto de autenticação
const mockAuthContext: AuthContextType = {
  user: null,
  isAuthenticated: false,
  isLoading: false,
  login: async (data: LoginInput) => {
    console.log('🔐 Fazendo login com:', data);
  },
  logout: () => {
    console.log('👋 Saindo...');
  },
  validateSession: async () => {
    console.log('🔍 Validando sessão...');
    return false;
  },
};

// Executar exemplos
console.log('\n=== DEMONSTRAÇÃO DE USO - TASK 1.0 ===\n');

console.log('1️⃣ Validação de dados válidos:');
validateLoginData({
  barbershopCode: 'barb001',
  phone: '(11) 99999-9999',
});

console.log('\n2️⃣ Validação de dados inválidos:');
validateLoginData({
  barbershopCode: 'abc', // muito curto
  phone: '11999999999', // sem máscara
});

console.log('\n3️⃣ Demonstração de máscara de telefone:');
demonstratePhoneMask();

console.log('\n4️⃣ Context de autenticação:');
console.log('Context criado:', mockAuthContext.isAuthenticated ? 'Autenticado' : 'Não autenticado');

export { validateLoginData, demonstratePhoneMask, exampleLoginFunction, mockAuthContext };
