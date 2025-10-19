import { useState } from 'react';
import { LoginForm } from '@/components/auth/LoginForm';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { HelpCircle } from 'lucide-react';

/**
 * Página de login para barbeiros
 * 
 * Página principal de autenticação que apresenta o formulário de login
 * em um layout mobile-first com card centralizado. Inclui modal de ajuda
 * para primeiro acesso.
 * 
 * Features:
 * - Layout responsivo e mobile-first
 * - Card centralizado com formulário de login
 * - Modal de ajuda para primeiro acesso
 * - Design consistente com Shadcn UI
 * 
 * @example
 * ```tsx
 * <Route path="/login" element={<LoginPage />} />
 * ```
 */
export function LoginPage() {
  const [showHelp, setShowHelp] = useState(false);
  
  return (
    <div className="min-h-screen flex items-center justify-center p-4 bg-gray-50" data-testid="login-page">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle className="text-2xl">Login Barbeiro</CardTitle>
          <CardDescription>
            Entre com seu e-mail e senha para acessar sua agenda
          </CardDescription>
        </CardHeader>
        
        <CardContent className="space-y-6">
          <LoginForm />
          
          {/* Link de Ajuda */}
          <div className="text-center">
            <p className="text-sm text-gray-600">
              Primeiro acesso?{' '}
              <button
                type="button"
                className="text-blue-600 hover:underline focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 rounded"
                onClick={() => setShowHelp(true)}
                data-testid="help-button"
              >
                Precisa de ajuda?
              </button>
            </p>
          </div>
        </CardContent>
      </Card>
      
      {/* Modal de Ajuda */}
      <Dialog open={showHelp} onOpenChange={setShowHelp}>
        <DialogContent data-testid="help-modal">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <HelpCircle className="h-5 w-5 text-blue-600" />
              Como fazer login
            </DialogTitle>
            <DialogDescription className="space-y-3 pt-4 text-left">
              <div className="space-y-2">
                <p className="text-sm text-gray-700">
                  <strong>📧 E-mail:</strong> Use o e-mail que foi cadastrado pelo administrador da sua barbearia.
                </p>
                <p className="text-sm text-gray-700">
                  <strong>🔒 Senha:</strong> Use a senha fornecida pelo administrador. Você pode alterá-la depois do primeiro acesso.
                </p>
                <p className="text-sm text-gray-700">
                  <strong>❓ Não tem acesso?</strong> Entre em contato com o administrador da sua barbearia.
                </p>
              </div>
            </DialogDescription>
          </DialogHeader>
          
          <div className="pt-4">
            <Button 
              onClick={() => setShowHelp(false)} 
              className="w-full"
              data-testid="help-close-button"
            >
              Entendi
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
