import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Login } from '@/pages/Login';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/" element={
          <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-primary/5 via-background to-background px-4 py-10">
            <div className="w-full max-w-2xl rounded-xl border bg-background/90 p-8 shadow-sm backdrop-blur">
              <span className="inline-flex items-center gap-2 rounded-full bg-secondary px-3 py-1 text-sm font-medium text-secondary-foreground">
                <span className="h-2 w-2 rounded-full bg-success"></span>
                Ambiente inicial pronto
              </span>

              <h1 className="mt-6 text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
                BarbApp Admin
              </h1>

              <p className="mt-2 text-base text-muted-foreground">
                Sistema de gestão de barbearias implementado com sucesso.
              </p>

              <div className="mt-8 grid gap-4 sm:grid-cols-2">
                <article className="rounded-lg border border-dashed bg-muted/40 p-4">
                  <h2 className="text-sm font-semibold uppercase text-muted-foreground">
                    Funcionalidades
                  </h2>
                  <ul className="mt-3 space-y-1 text-sm text-muted-foreground">
                    <li>✅ Login implementado</li>
                    <li>✅ Validação de formulários</li>
                    <li>✅ Integração com API</li>
                    <li>⏳ Roteamento completo</li>
                  </ul>
                </article>

                <article className="rounded-lg border bg-background p-4">
                  <h2 className="text-sm font-semibold uppercase text-muted-foreground">
                    Ações
                  </h2>
                  <div className="mt-3 space-y-2">
                    <a
                      href="/login"
                      className="inline-flex w-full items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
                    >
                      Ver Tela de Login
                    </a>
                    <p className="text-xs text-muted-foreground">
                      Teste a página de login implementada
                    </p>
                  </div>
                </article>
              </div>
            </div>
          </div>
        } />
      </Routes>
    </Router>
  );
}

export default App;
