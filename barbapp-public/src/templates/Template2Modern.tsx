import React from 'react';
import { MapPin, Clock, Instagram, Facebook } from 'lucide-react';
import type { PublicLandingPage } from '@/types/landing-page.types';
import { ServiceCard } from '@/components/ServiceCard';
import { WhatsAppButton } from '@/components/WhatsAppButton';
import { useServiceSelection } from '@/hooks/useServiceSelection';
import { useNavigate } from 'react-router-dom';

interface Template2ModernProps {
  data: PublicLandingPage;
}

export const Template2Modern: React.FC<Template2ModernProps> = ({ data }) => {
  const navigate = useNavigate();
  const { barbershop, landingPage } = data;
  const {
    selectedIds,
    totalPrice,
    hasSelection,
    toggleService,
  } = useServiceSelection(landingPage.services);

  const handleSchedule = () => {
    const serviceIds = Array.from(selectedIds).join(',');
    const url = hasSelection
      ? `/barbearia/${barbershop.code}/agendar?servicos=${serviceIds}`
      : `/barbearia/${barbershop.code}/agendar`;
    navigate(url);
  };

  return (
    <div className="min-h-screen bg-modern-light font-modern-sans">
      {/* Header - Fixed */}
      <header className="bg-modern-dark text-white py-4 px-6 fixed top-0 left-0 right-0 z-50 shadow-lg">
        <div className="container mx-auto flex justify-between items-center">
          <div className="flex items-center gap-4">
            {landingPage.logoUrl && (
              <img
                src={landingPage.logoUrl}
                alt={barbershop.name}
                className="w-12 h-12 rounded-full border-2 border-blue-500"
              />
            )}
            <h1 className="text-2xl font-bold">{barbershop.name}</h1>
          </div>
          <nav className="hidden md:flex gap-6">
            <a href="#servicos" className="hover:text-modern-blue transition-colors">
              Serviços
            </a>
            <a href="#sobre" className="hover:text-modern-blue transition-colors">
              Sobre
            </a>
            <a href="#contato" className="hover:text-modern-blue transition-colors">
              Contato
            </a>
          </nav>
          <button
            onClick={handleSchedule}
            className="bg-modern-blue text-white px-6 py-2 rounded-lg font-semibold hover:bg-blue-600 transition-colors"
          >
            Agendar Agora
          </button>
        </div>
      </header>

      {/* Hero Section */}
      <section className="pt-20 pb-16 px-6 bg-gradient-to-br from-modern-dark to-gray-700 flex items-center justify-center text-white min-h-[70vh]">
        <div className="container mx-auto text-center">
          <h2 className="text-6xl font-bold mb-6 leading-tight">{barbershop.name}</h2>
          <p className="text-xl mb-12 max-w-2xl mx-auto opacity-90">
            Experiência moderna e profissional para o seu cuidado pessoal
          </p>
          <button
            onClick={handleSchedule}
            className="bg-modern-blue text-white px-10 py-4 rounded-lg text-xl font-semibold hover:bg-blue-600 transition-all transform hover:scale-105 shadow-lg"
          >
            Agendar Serviço
          </button>
        </div>
      </section>

      {/* Services Section */}
      <section id="servicos" className="py-20 px-6 bg-white">
        <div className="container mx-auto">
          <h2 className="text-4xl font-bold text-center mb-16 text-modern-dark">
            Nossos Serviços
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {landingPage.Services.map((service) => (
              <div
                key={service.id}
                className="transform transition-all duration-300 hover:shadow-xl hover:-translate-y-2"
              >
                <ServiceCard
                  service={service}
                  isSelected={selectedIds.has(service.id)}
                  onToggle={() => toggleService(service.id)}
                />
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* About Section - More elaborate */}
      {landingPage.aboutText && (
        <section id="sobre" className="py-20 px-6 bg-modern-light">
          <div className="container mx-auto">
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
              <div>
                <h2 className="text-4xl font-bold mb-8 text-modern-dark">
                  Sobre Nós
                </h2>
                <div className="w-16 h-1 bg-modern-blue mb-8"></div>
                <p className="text-lg text-gray-700 leading-relaxed whitespace-pre-line">
                  {landingPage.aboutText}
                </p>
              </div>
              <div className="relative">
                <div className="aspect-square bg-gradient-to-br from-modern-blue to-blue-600 rounded-2xl shadow-2xl flex items-center justify-center">
                  <div className="text-white text-center">
                    <div className="text-6xl mb-4">✂️</div>
                    <p className="text-xl font-semibold">Profissionalismo</p>
                    <p className="text-sm opacity-90">e Qualidade</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>
      )}

      {/* Contact Section */}
      <section id="contato" className="py-20 px-6 bg-white">
        <div className="container mx-auto max-w-6xl">
          <h2 className="text-4xl font-bold text-center mb-16 text-modern-dark">
            Onde Estamos
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
            <div className="flex items-start gap-4">
              <div className="bg-modern-blue p-3 rounded-full">
                <MapPin className="text-white" size={24} />
              </div>
              <div>
                <h3 className="text-xl font-semibold mb-2 text-modern-dark">Endereço</h3>
                <p className="text-gray-600">{barbershop.address}</p>
              </div>
            </div>
            {landingPage.openingHours && (
              <div className="flex items-start gap-4">
                <div className="bg-modern-blue p-3 rounded-full">
                  <Clock className="text-white" size={24} />
                </div>
                <div>
                  <h3 className="text-xl font-semibold mb-2 text-modern-dark">Horário</h3>
                  <p className="text-gray-600 whitespace-pre-line">
                    {landingPage.openingHours}
                  </p>
                </div>
              </div>
            )}
          </div>

          {/* Social Media */}
          {(landingPage.instagramUrl || landingPage.facebookUrl) && (
            <div className="mt-16 text-center">
              <h3 className="text-2xl font-semibold mb-8 text-modern-dark">Nos Acompanhe</h3>
              <div className="flex justify-center gap-8">
                {landingPage.instagramUrl && (
                  <a
                    href={landingPage.instagramUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="bg-modern-dark text-white p-4 rounded-full hover:bg-modern-blue transition-colors transform hover:scale-110"
                  >
                    <Instagram size={24} />
                  </a>
                )}
                {landingPage.facebookUrl && (
                  <a
                    href={landingPage.facebookUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="bg-modern-dark text-white p-4 rounded-full hover:bg-modern-blue transition-colors transform hover:scale-110"
                  >
                    <Facebook size={24} />
                  </a>
                )}
              </div>
            </div>
          )}
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-modern-dark text-white py-12 px-6">
        <div className="container mx-auto text-center">
          <p className="text-lg mb-4">© 2025 {barbershop.name} - Todos os direitos reservados</p>
          <a
            href="/admin/login"
            className="text-sm text-gray-400 hover:text-modern-blue transition-colors"
          >
            Área Admin
          </a>
        </div>
      </footer>

      {/* Floating WhatsApp Button */}
      <WhatsAppButton
        phoneNumber={landingPage.whatsappNumber}
        floating
      />

      {/* Floating Schedule Button (quando tem seleção) */}
      {hasSelection && (
        <button
          onClick={handleSchedule}
          className="fixed bottom-24 right-6 bg-modern-blue text-white px-6 py-3 rounded-full shadow-lg hover:scale-105 transition-all z-50 font-semibold"
        >
          Agendar {selectedIds.size} serviço{selectedIds.size > 1 ? 's' : ''} •
          R$ {totalPrice.toFixed(2)}
        </button>
      )}
    </div>
  );
};