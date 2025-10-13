const appName = import.meta.env.VITE_APP_NAME ?? 'BarbApp Admin';

function App() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-primary/5 via-background to-background px-4 py-10">
      <div className="w-full max-w-2xl rounded-xl border bg-background/90 p-8 shadow-sm backdrop-blur">
        <span className="inline-flex items-center gap-2 rounded-full bg-secondary px-3 py-1 text-sm font-medium text-secondary-foreground">
          <span className="h-2 w-2 rounded-full bg-success"></span>
          Ambiente inicial pronto
        </span>

        <h1 className="mt-6 text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">
          {appName}
        </h1>

        <p className="mt-2 text-base text-muted-foreground">
          Configuração base do projeto React + Vite + TailwindCSS criada com sucesso. A partir
          daqui seguiremos com os módulos da Admin Central do barbApp.
        </p>

        <div className="mt-8 grid gap-4 sm:grid-cols-2">
          <article className="rounded-lg border border-dashed bg-muted/40 p-4">
            <h2 className="text-sm font-semibold uppercase text-muted-foreground">
              Frontend Stack
            </h2>
            <ul className="mt-3 space-y-1 text-sm text-muted-foreground">
              <li>React 18 + TypeScript</li>
              <li>Vite 5 com alias @/</li>
              <li>TailwindCSS 3 + Radix UI</li>
            </ul>
          </article>

          <article className="rounded-lg border bg-background p-4">
            <h2 className="text-sm font-semibold uppercase text-muted-foreground">
              Próximos passos
            </h2>
            <ul className="mt-3 space-y-1 text-sm text-muted-foreground">
              <li>Definir estrutura de pastas</li>
              <li>Implementar roteamento</li>
              <li>Adicionar componentes compartilhados</li>
            </ul>
          </article>
        </div>
      </div>
    </div>
  );
}

export default App;
