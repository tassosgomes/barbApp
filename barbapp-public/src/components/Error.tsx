import React from 'react';

interface ErrorStateProps {
  title?: string;
  message?: string;
  className?: string;
  onRetry?: () => void;
}

export const ErrorState: React.FC<ErrorStateProps> = ({
  title = 'Erro',
  message = 'Ocorreu um erro inesperado.',
  className = '',
  onRetry,
}) => {
  return (
    <div className={`min-h-screen flex items-center justify-center ${className}`}>
      <div className="text-center max-w-md">
        <h1 className="text-2xl font-bold text-red-500 mb-2">{title}</h1>
        <p className="text-gray-600 mb-4">{message}</p>
        {onRetry && (
          <button
            onClick={onRetry}
            className="bg-primary text-white px-4 py-2 rounded hover:bg-primary/90 transition-colors"
          >
            Tentar Novamente
          </button>
        )}
      </div>
    </div>
  );
};