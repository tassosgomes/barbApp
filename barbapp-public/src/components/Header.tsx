import React from 'react';

interface HeaderProps {
  logoUrl?: string;
  barbershopName: string;
  className?: string;
  children?: React.ReactNode;
}

export const Header: React.FC<HeaderProps> = ({
  logoUrl,
  barbershopName,
  className = '',
  children,
}) => {
  return (
    <header className={`bg-black text-white py-4 px-6 sticky top-0 z-40 ${className}`}>
      <div className="container mx-auto flex justify-between items-center">
        <div className="flex items-center gap-4">
          {logoUrl && (
            <img
              src={logoUrl}
              alt={barbershopName}
              className="w-12 h-12 rounded-full"
            />
          )}
          <h1 className="text-2xl font-bold">{barbershopName}</h1>
        </div>
        {children}
      </div>
    </header>
  );
};