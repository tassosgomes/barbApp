import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import '@/index.css';
import { Toaster } from '@/components/ui/toaster';
import App from '@/App';
import { queryClient } from '@/lib/queryClient';
import { BarbeariaProvider } from '@/contexts/BarbeariaContext';

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
