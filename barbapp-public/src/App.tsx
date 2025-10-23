import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LandingPage } from './pages/LandingPage';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/barbearia/:code" element={<LandingPage />} />
          {/* Outras rotas... */}
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
