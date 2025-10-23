import React, { useState, useEffect } from 'react';
import { MapPin, Clock, Instagram, Facebook, Star } from 'lucide-react';
import type { PublicLandingPage } from '@/types/landing-page.types';
import { WhatsAppButton } from '@/components/WhatsAppButton';
import { useServiceSelection } from '@/hooks/useServiceSelection';
import { useNavigate } from 'react-router-dom';

interface Template5PremiumProps {
  data: PublicLandingPage;
}

export const Template5Premium: React.FC<Template5PremiumProps> = ({ data }) => {
  const navigate = useNavigate();
  const { barbershop, landingPage } = data;
  const {
    selectedIds,
    totalPrice,
    hasSelection,
    toggleService,
  } = useServiceSelection(landingPage.services);

  const [headerSolid, setHeaderSolid] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      setHeaderSolid(window.scrollY > 100);
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const handleSchedule = () => {
    const serviceIds = Array.from(selectedIds).join(',');
    const url = hasSelection
      ? `/barbearia/${barbershop.code}/agendar?servicos=${serviceIds}`
      : `/barbearia/${barbershop.code}/agendar`;
    navigate(url);
  };

  return (
    <div className="min-h-screen bg-premium-black text-white font-premium-sans">
      {/* Header */}
      <header
        className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
          headerSolid
            ? 'bg-premium-black/95 backdrop-blur-sm shadow-lg'
            : 'bg-transparent'
        }`}
      >
        <div className="container mx-auto px-6 py-4 flex justify-between items-center">
          <div className="flex items-center gap-4">
            {landingPage.logoUrl && (
              <img
                src={landingPage.logoUrl}
                alt={barbershop.name}
                className="w-12 h-12 rounded-full border-2 border-premium-gold"
              />
            )}
            <h1 className="text-2xl font-bold font-premium-serif text-premium-gold">
              {barbershop.name}
            </h1>
          </div>
          <nav className="hidden md:flex gap-8">
            <a
              href="#servicos"
              className="text-premium-gold hover:text-white transition-colors duration-300"
            >
              Serviços
            </a>
            <a
              href="#sobre"
              className="text-premium-gold hover:text-white transition-colors duration-300"
            >
              Sobre
            </a>
            <a
              href="#contato"
              className="text-premium-gold hover:text-white transition-colors duration-300"
            >
              Contato
            </a>
          </nav>
          <button
            onClick={handleSchedule}
            className="bg-premium-gold text-premium-black px-6 py-2 rounded-lg font-semibold hover:bg-premium-gold/90 transition-all duration-300 hover:scale-105"
          >
            Agendar Agora
          </button>
        </div>
      </header>

      {/* Hero Section with Parallax */}
      <section
        className="relative h-screen flex items-center justify-center text-white overflow-hidden"
        style={{
          background: 'linear-gradient(135deg, #1C1C1C 0%, #2E2E2E 100%)',
          backgroundAttachment: 'fixed',
          backgroundSize: 'cover',
        }}
      >
        <div className="absolute inset-0 bg-premium-black/30"></div>
        <div className="text-center z-10 animate-fade-in">
          <h2 className="text-6xl md:text-7xl font-bold font-premium-serif text-premium-gold mb-6">
            {barbershop.name}
          </h2>
          <p className="text-xl md:text-2xl mb-12 text-gray-300 max-w-2xl mx-auto">
            Luxo e Sofisticação em Cada Detalhe
          </p>
          <button
            onClick={handleSchedule}
            className="bg-premium-gold text-premium-black px-10 py-4 rounded-lg text-xl font-semibold hover:bg-premium-gold/90 transition-all duration-300 hover:scale-105 shadow-2xl"
          >
            Agendar Experiência Premium
          </button>
        </div>
        {/* Parallax effect hint - decorative elements */}
        <div className="absolute bottom-10 left-1/2 transform -translate-x-1/2 animate-bounce">
          <div className="w-6 h-10 border-2 border-premium-gold rounded-full flex justify-center">
            <div className="w-1 h-3 bg-premium-gold rounded-full mt-2 animate-pulse"></div>
          </div>
        </div>
      </section>

      {/* Services Section - List Format */}
      <section id="servicos" className="py-24 px-6 bg-premium-gray">
        <div className="container mx-auto max-w-4xl">
          <h2 className="text-5xl font-bold text-center mb-16 font-premium-serif text-premium-gold">
            Nossos Serviços Premium
          </h2>
          <div className="space-y-6">
            {landingPage.services.map((service, index) => (
              <div
                key={service.id}
                className={`flex items-center justify-between p-8 bg-premium-black rounded-lg shadow-lg hover:shadow-xl transition-all duration-300 animate-fade-in-up`}
                style={{ animationDelay: `${index * 0.1}s` }}
              >
                <div className="flex-1">
                  <div className="flex items-center gap-4 mb-2">
                    <h3 className="text-2xl font-semibold font-premium-serif text-premium-gold">
                      {service.name}
                    </h3>
                    <input
                      type="checkbox"
                      checked={selectedIds.has(service.id)}
                      onChange={() => toggleService(service.id)}
                      className="w-6 h-6 accent-premium-gold cursor-pointer"
                    />
                  </div>
                  {service.description && (
                    <p className="text-gray-300 leading-relaxed">
                      {service.description}
                    </p>
                  )}
                </div>
                <div className="text-right ml-8">
                  <div className="text-2xl font-bold text-premium-gold mb-1">
                    R$ {service.price.toFixed(2)}
                  </div>
                  <div className="text-sm text-gray-400">
                    {service.duration} minutos
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* About Section */}
      {landingPage.aboutText && (
        <section id="sobre" className="py-24 px-6 bg-premium-black">
          <div className="container mx-auto max-w-4xl text-center">
            <h2 className="text-5xl font-bold mb-12 font-premium-serif text-premium-gold">
              Sobre Nós
            </h2>
            <p className="text-xl text-gray-300 leading-relaxed whitespace-pre-line animate-fade-in">
              {landingPage.aboutText}
            </p>
          </div>
        </section>
      )}

      {/* Testimonials Section (Placeholder - if data available in future) */}
      <section className="py-24 px-6 bg-premium-gray">
        <div className="container mx-auto max-w-4xl text-center">
          <h2 className="text-5xl font-bold mb-16 font-premium-serif text-premium-gold">
            Depoimentos
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {/* Placeholder testimonials */}
            <div className="bg-premium-black p-8 rounded-lg shadow-lg animate-fade-in-up">
              <div className="flex justify-center mb-4">
                {[...Array(5)].map((_, i) => (
                  <Star key={i} className="w-5 h-5 text-premium-gold fill-current" />
                ))}
              </div>
              <p className="text-gray-300 italic mb-4">
                "Experiência excepcional. Serviço impecável e atendimento premium."
              </p>
              <p className="text-premium-gold font-semibold">- Cliente Satisfeito</p>
            </div>
            <div className="bg-premium-black p-8 rounded-lg shadow-lg animate-fade-in-up" style={{ animationDelay: '0.2s' }}>
              <div className="flex justify-center mb-4">
                {[...Array(5)].map((_, i) => (
                  <Star key={i} className="w-5 h-5 text-premium-gold fill-current" />
                ))}
              </div>
              <p className="text-gray-300 italic mb-4">
                "Ambiente sofisticado e profissionais de alto nível."
              </p>
              <p className="text-premium-gold font-semibold">- Cliente Premium</p>
            </div>
          </div>
        </div>
      </section>

      {/* Contact Section */}
      <section id="contato" className="py-24 px-6 bg-premium-black">
        <div className="container mx-auto max-w-4xl">
          <h2 className="text-5xl font-bold text-center mb-16 font-premium-serif text-premium-gold">
            Entre em Contato
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
            <div className="flex items-start gap-4 animate-fade-in-left">
              <MapPin className="text-premium-gold flex-shrink-0 mt-1" size={28} />
              <div>
                <h3 className="text-2xl font-semibold mb-3 font-premium-serif text-premium-gold">
                  Localização
                </h3>
                <p className="text-gray-300 leading-relaxed">{barbershop.address}</p>
              </div>
            </div>
            {landingPage.openingHours && (
              <div className="flex items-start gap-4 animate-fade-in-right">
                <Clock className="text-premium-gold flex-shrink-0 mt-1" size={28} />
                <div>
                  <h3 className="text-2xl font-semibold mb-3 font-premium-serif text-premium-gold">
                    Horário de Funcionamento
                  </h3>
                  <p className="text-gray-300 leading-relaxed whitespace-pre-line">
                    {landingPage.openingHours}
                  </p>
                </div>
              </div>
            )}
          </div>

          {/* Social Media */}
          {(landingPage.instagramUrl || landingPage.facebookUrl) && (
            <div className="mt-16 text-center animate-fade-in">
              <h3 className="text-3xl font-semibold mb-8 font-premium-serif text-premium-gold">
                Nos Acompanhe
              </h3>
              <div className="flex justify-center gap-8">
                {landingPage.instagramUrl && (
                  <a
                    href={landingPage.instagramUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-premium-gold hover:text-white transition-all duration-300 hover:scale-110"
                  >
                    <Instagram size={40} />
                  </a>
                )}
                {landingPage.facebookUrl && (
                  <a
                    href={landingPage.facebookUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-premium-gold hover:text-white transition-all duration-300 hover:scale-110"
                  >
                    <Facebook size={40} />
                  </a>
                )}
              </div>
            </div>
          )}
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-premium-gray text-white py-12 px-6">
        <div className="container mx-auto text-center">
          <p className="text-lg mb-4">© 2025 {barbershop.name} - Luxo e Tradição</p>
          <a
            href="/admin/login"
            className="text-premium-gold hover:text-white transition-colors duration-300 text-sm"
          >
            Área Administrativa
          </a>
        </div>
      </footer>

      {/* Floating WhatsApp Button */}
      <WhatsAppButton
        phoneNumber={landingPage.whatsappNumber}
        floating
        className="bg-premium-gold hover:bg-premium-gold/90"
      />

      {/* Floating Schedule Button */}
      {hasSelection && (
        <button
          onClick={handleSchedule}
          className="fixed bottom-24 right-6 bg-premium-gold text-premium-black px-8 py-4 rounded-full shadow-2xl hover:scale-105 transition-all duration-300 z-50 font-semibold"
        >
          Agendar {selectedIds.size} serviço{selectedIds.size > 1 ? 's' : ''} • R$ {totalPrice.toFixed(2)}
        </button>
      )}
    </div>
  );
};