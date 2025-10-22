import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import '@/index.css';
import '@/styles/animations.css';
import { Toaster } from '@/components/ui/toaster';
import App from '@/App';
import { queryClient } from '@/lib/queryClient';
import { BarbeariaProvider } from '@/contexts/BarbeariaContext';
import { validateEnvironment, env } from '@/config/env';

// Validar variáveis de ambiente antes de inicializar a aplicação
try {
  validateEnvironment();
} catch (error) {
  console.error('❌ Erro na validação de ambiente:', error);
  // Em produção, você pode querer redirecionar para uma página de erro
}

// Definir título da página baseado na configuração
document.title = env.APP_NAME;

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <BarbeariaProvider>
        <App />
        <Toaster />
        {import.meta.env.DEV && <ReactQueryDevtools initialIsOpen={false} />}
      </BarbeariaProvider>
    </QueryClientProvider>
  </StrictMode>,
);
