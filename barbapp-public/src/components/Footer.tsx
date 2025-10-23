import React from 'react';

interface FooterProps {
  barbershopName: string;
  className?: string;
  children?: React.ReactNode;
}

export const Footer: React.FC<FooterProps> = ({
  barbershopName,
  className = '',
  children,
}) => {
  return (
    <footer className={`bg-black text-white py-8 px-6 text-center ${className}`}>
      <p>© 2025 {barbershopName} - Todos os direitos reservados</p>
      {children}
      <a
        href="/admin/login"
        className="text-sm text-gray-400 hover:text-white mt-2 inline-block"
      >
        Área Admin
      </a>
    </footer>
  );
};