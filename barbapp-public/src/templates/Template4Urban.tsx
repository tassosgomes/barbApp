import React, { useState } from 'react';
import { Menu, X, MapPin, Clock, Instagram, Facebook, Zap } from 'lucide-react';
import type { PublicLandingPage } from '@/types/landing-page.types';
import { WhatsAppButton } from '@/components/WhatsAppButton';
import { useServiceSelection } from '@/hooks/useServiceSelection';
import { useNavigate } from 'react-router-dom';

interface Template4UrbanProps {
  data: PublicLandingPage;
}

export const Template4Urban: React.FC<Template4UrbanProps> = ({ data }) => {
  const navigate = useNavigate();
    const { barbershop, landingPage } = data;
  const {
    selectedIds,
    totalPrice,
    hasSelection,
    toggleService,
  } = useServiceSelection(landingPage.services);

  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const handleSchedule = () => {
    const serviceIds = Array.from(selectedIds).join(',');
    const url = hasSelection
      ? `/barbearia/${barbershop.code}/agendar?servicos=${serviceIds}`
      : `/barbearia/${barbershop.code}/agendar`;
    navigate(url);
  };

  const toggleMenu = () => setIsMenuOpen(!isMenuOpen);

  return (
    <div className="min-h-screen bg-urban-black text-white font-urban-sans relative overflow-x-hidden">
      {/* Diagonal background elements */}
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute -top-20 -right-20 w-96 h-96 bg-urban-red/10 transform rotate-45"></div>
        <div className="absolute -bottom-20 -left-20 w-96 h-96 bg-urban-red/5 transform -rotate-45"></div>
      </div>

      {/* Side Menu Overlay */}
      {isMenuOpen && (
        <div className="fixed inset-0 bg-urban-black/90 z-50 flex">
          <div className="w-80 bg-urban-black border-r-4 border-urban-red p-8">
            <button
              onClick={toggleMenu}
              className="mb-8 text-urban-red hover:text-white transition-colors"
            >
              <X size={32} />
            </button>
            <nav className="space-y-6">
              <a
                href="#servicos"
                onClick={toggleMenu}
                className="block text-2xl font-urban-display uppercase tracking-wider hover:text-urban-red transition-colors"
              >
                Serviços
              </a>
              <a
                href="#sobre"
                onClick={toggleMenu}
                className="block text-2xl font-urban-display uppercase tracking-wider hover:text-urban-red transition-colors"
              >
                Sobre
              </a>
              <a
                href="#contato"
                onClick={toggleMenu}
                className="block text-2xl font-urban-display uppercase tracking-wider hover:text-urban-red transition-colors"
              >
                Contato
              </a>
            </nav>
          </div>
          <div className="flex-1" onClick={toggleMenu}></div>
        </div>
      )}

      {/* Header */}
      <header className="relative z-10 flex justify-between items-center p-6 bg-urban-black/95 backdrop-blur-sm border-b border-urban-gray/20">
        <div className="flex items-center gap-4">
          {landingPage.logoUrl && (
            <img
              src={landingPage.logoUrl}
              alt={barbershop.name}
              className="w-12 h-12 rounded-full border-2 border-urban-red"
            />
          )}
          <h1 className="text-2xl font-urban-display uppercase tracking-wider">
            {barbershop.name}
          </h1>
        </div>
        <button
          onClick={toggleMenu}
          className="text-urban-red hover:text-white transition-colors"
        >
          <Menu size={32} />
        </button>
      </header>

      {/* Hero Section - Full Screen */}
      <section className="relative h-screen flex items-center justify-center bg-gradient-to-br from-urban-black via-urban-black/95 to-urban-red/20">
        {/* Background graphics */}
        <div className="absolute inset-0">
          <div className="absolute top-20 left-20 w-32 h-32 border-2 border-urban-red/30 transform rotate-45"></div>
          <div className="absolute bottom-20 right-20 w-24 h-24 bg-urban-red/10 transform -rotate-12"></div>
          <div className="absolute top-1/2 left-1/4 w-16 h-16 border border-urban-gray/20 transform rotate-30"></div>
        </div>

        <div className="relative z-10 text-center px-6 max-w-4xl mx-auto">
          <div className="mb-8">
            <Zap className="inline-block text-urban-red mb-4" size={64} />
          </div>
          <h2 className="text-6xl md:text-8xl font-urban-display uppercase tracking-widest mb-6 text-white">
            {barbershop.name}
          </h2>
          <p className="text-xl md:text-2xl text-urban-gray mb-12 font-urban-sans">
            Estilo Urbano • Energia Inigualável • Cortes de Impacto
          </p>
          <button
            onClick={handleSchedule}
            className="bg-urban-red text-white px-12 py-4 text-xl font-urban-display uppercase tracking-wider hover:bg-urban-red/80 transition-all transform hover:scale-105 shadow-lg"
          >
            Agendar Agora
          </button>
        </div>

        {/* Diagonal accent lines */}
        <div className="absolute bottom-0 left-0 w-full h-32 bg-gradient-to-t from-urban-red/20 to-transparent transform -skew-y-3"></div>
      </section>

      {/* Services Section - 3 Column Grid */}
      <section id="servicos" className="py-20 px-6 bg-urban-black relative">
        {/* Diagonal separator */}
        <div className="absolute top-0 left-0 w-full h-16 bg-urban-red transform -skew-y-2"></div>

        <div className="relative z-10 container mx-auto max-w-6xl">
          <div className="text-center mb-16">
            <h2 className="text-5xl font-urban-display uppercase tracking-widest text-white mb-4">
              Nossos Serviços
            </h2>
            <div className="w-32 h-1 bg-urban-red mx-auto mb-6"></div>
            <p className="text-urban-gray text-lg">
              Cortes que definem seu estilo • Qualidade urbana
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {landingPage.services.map((service) => (
              <div
                key={service.id}
                className="relative bg-urban-gray/10 backdrop-blur-sm border border-urban-gray/20 rounded-lg p-6 hover:border-urban-red/50 transition-all hover:transform hover:scale-105 group"
              >
                {/* Diagonal accent */}
                <div className="absolute top-0 right-0 w-16 h-16 bg-urban-red/20 transform rotate-45 -translate-y-8 translate-x-8 group-hover:bg-urban-red/30 transition-colors"></div>

                <div className="relative z-10">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-xl font-urban-display uppercase tracking-wider text-white">
                      {service.name}
                    </h3>
                    <input
                      type="checkbox"
                      checked={selectedIds.has(service.id)}
                      onChange={() => toggleService(service.id)}
                      className="w-6 h-6 text-urban-red bg-urban-black border-urban-gray rounded focus:ring-urban-red"
                    />
                  </div>

                  {service.description && (
                    <p className="text-urban-gray/80 mb-4 text-sm">{service.description}</p>
                  )}

                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2 text-urban-gray">
                      <Clock size={16} />
                      <span className="text-sm">{service.duration}min</span>
                    </div>
                    <div className="text-2xl font-urban-display font-bold text-urban-red">
                      R$ {service.price.toFixed(2)}
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* About Section */}
      {landingPage.aboutText && (
        <section id="sobre" className="py-20 px-6 bg-gradient-to-r from-urban-black to-urban-gray/10 relative">
          {/* Diagonal graphics */}
          <div className="absolute inset-0 overflow-hidden">
            <div className="absolute top-10 right-10 w-20 h-20 border-2 border-urban-red/20 transform rotate-45"></div>
            <div className="absolute bottom-10 left-10 w-16 h-16 bg-urban-red/10 transform -rotate-30"></div>
          </div>

          <div className="relative z-10 container mx-auto max-w-4xl text-center">
            <h2 className="text-5xl font-urban-display uppercase tracking-widest text-white mb-8">
              Sobre a {barbershop.name}
            </h2>
            <div className="w-32 h-1 bg-urban-red mx-auto mb-12"></div>
            <div className="bg-urban-black/50 backdrop-blur-sm rounded-lg p-8 border border-urban-gray/20">
              <p className="text-lg leading-relaxed whitespace-pre-line text-urban-gray">
                {landingPage.aboutText}
              </p>
            </div>
          </div>
        </section>
      )}

      {/* Contact Section */}
      <section id="contato" className="py-20 px-6 bg-urban-black">
        <div className="container mx-auto max-w-4xl">
          <div className="text-center mb-16">
            <h2 className="text-5xl font-urban-display uppercase tracking-widest text-white mb-4">
              Onde Estamos
            </h2>
            <div className="w-32 h-1 bg-urban-red mx-auto"></div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
            <div className="bg-urban-gray/10 backdrop-blur-sm rounded-lg p-8 border border-urban-gray/20">
              <div className="flex items-start gap-4">
                <div className="bg-urban-red text-white p-3 rounded-full">
                  <MapPin size={24} />
                </div>
                <div>
                  <h3 className="text-xl font-urban-display uppercase tracking-wider text-white mb-2">
                    Endereço
                  </h3>
                  <p className="text-urban-gray leading-relaxed">{barbershop.address}</p>
                </div>
              </div>
            </div>

            {landingPage.openingHours && (
              <div className="bg-urban-gray/10 backdrop-blur-sm rounded-lg p-8 border border-urban-gray/20">
                <div className="flex items-start gap-4">
                  <div className="bg-urban-red text-white p-3 rounded-full">
                    <Clock size={24} />
                  </div>
                  <div>
                    <h3 className="text-xl font-urban-display uppercase tracking-wider text-white mb-2">
                      Horário
                    </h3>
                    <p className="text-urban-gray leading-relaxed whitespace-pre-line">
                      {landingPage.openingHours}
                    </p>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Social Media */}
          {(landingPage.instagramUrl || landingPage.facebookUrl) && (
            <div className="mt-16 text-center">
              <h3 className="text-2xl font-urban-display uppercase tracking-wider text-white mb-8">
                Nos Acompanhe
              </h3>
              <div className="flex justify-center gap-8">
                {landingPage.instagramUrl && (
                  <a
                    href={landingPage.instagramUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="bg-urban-black border-2 border-urban-red text-urban-red p-4 rounded-full hover:bg-urban-red hover:text-white transition-all transform hover:scale-110"
                  >
                    <Instagram size={32} />
                  </a>
                )}
                {landingPage.facebookUrl && (
                  <a
                    href={landingPage.facebookUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="bg-urban-black border-2 border-urban-red text-urban-red p-4 rounded-full hover:bg-urban-red hover:text-white transition-all transform hover:scale-110"
                  >
                    <Facebook size={32} />
                  </a>
                )}
              </div>
            </div>
          )}
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-urban-black border-t border-urban-gray/20 py-12 px-6">
        <div className="container mx-auto text-center">
          <div className="flex items-center justify-center gap-4 mb-6">
            <div className="h-px bg-urban-red flex-1 max-w-20"></div>
            <Zap className="text-urban-red" size={24} />
            <div className="h-px bg-urban-red flex-1 max-w-20"></div>
          </div>
          <p className="text-lg font-urban-display uppercase tracking-wider text-white mb-4">
            © 2025 {barbershop.name}
          </p>
          <p className="text-urban-gray mb-6">Estilo que marca • Energia urbana</p>
          <a
            href="/admin/login"
            className="text-urban-gray hover:text-urban-red transition-colors text-sm"
          >
            Área Administrativa
          </a>
        </div>
      </footer>

      {/* Floating WhatsApp Button */}
      <WhatsAppButton
        phoneNumber={landingPage.whatsappNumber}
        floating
        className="bg-urban-red hover:bg-urban-red/80"
      />

      {/* Floating Schedule Button */}
      {hasSelection && (
        <button
          onClick={handleSchedule}
          className="fixed bottom-24 right-6 bg-urban-red text-white px-8 py-4 rounded-full shadow-lg hover:bg-urban-red/80 transition-all hover:scale-105 z-50 font-urban-display uppercase tracking-wider"
        >
          Agendar {selectedIds.size} serviço{selectedIds.size > 1 ? 's' : ''} • R$ {totalPrice.toFixed(2)}
        </button>
      )}
    </div>
  );
};