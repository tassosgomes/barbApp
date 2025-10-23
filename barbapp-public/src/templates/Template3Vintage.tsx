import React from 'react';
import { MapPin, Clock, Instagram, Facebook, Scissors, Star } from 'lucide-react';
import type { PublicLandingPage } from '@/types/landing-page.types';
import { WhatsAppButton } from '@/components/WhatsAppButton';
import { useServiceSelection } from '@/hooks/useServiceSelection';
import { useNavigate } from 'react-router-dom';

interface Template3VintageProps {
  data: PublicLandingPage;
}

export const Template3Vintage: React.FC<Template3VintageProps> = ({ data }) => {
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
    <div className="min-h-screen bg-vintage-cream font-vintage-sans relative">
      {/* Subtle texture background */}
      <div className="absolute inset-0 opacity-5 bg-[url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPGcgZmlsbD0ibm9uZSIgZmlsbC1ydWxlPSJldmVub2RkIj4KPGcgZmlsbD0iIzAwMCIgZmlsbC1vcGFjaXR5PSIwLjA0Ij4KPHBhdGggZD0iTTM2IDE0YzAtMS4xLS45LTItMi0ycy0uOS0xLTItMWgtOHYySDR2Mmg4YzEuMSAwIDItLjkgMi0yczAuOS0yIDItMmg4di0ySDR2MmgtOHoiLz4KPC9nPgo8L2c+')] repeat"></div>

      {/* Header with Banner */}
      <header className="relative bg-vintage-brown text-vintage-cream py-6 px-6 shadow-lg">
        <div className="absolute inset-0 bg-gradient-to-r from-vintage-brown to-vintage-brown/80"></div>
        <div className="relative container mx-auto">
          <div className="flex justify-between items-center">
            <div className="flex items-center gap-4">
              {landingPage.logoUrl && (
                <div className="relative">
                  <img
                    src={landingPage.logoUrl}
                    alt={barbershop.name}
                    className="w-16 h-16 rounded-full border-4 border-vintage-cream shadow-lg"
                  />
                  <div className="absolute -top-1 -right-1 w-6 h-6 bg-vintage-red rounded-full flex items-center justify-center">
                    <Star size={12} className="text-vintage-cream" />
                  </div>
                </div>
              )}
              <div>
                <h1 className="text-3xl font-vintage-display font-bold tracking-wider">
                  {barbershop.name}
                </h1>
                <p className="text-vintage-cream/80 text-sm">Estilo Clássico</p>
              </div>
            </div>
            <nav className="hidden md:flex gap-8">
              <a href="#servicos" className="hover:text-vintage-red transition-colors font-semibold">
                Serviços
              </a>
              <a href="#sobre" className="hover:text-vintage-red transition-colors font-semibold">
                Sobre
              </a>
              <a href="#contato" className="hover:text-vintage-red transition-colors font-semibold">
                Contato
              </a>
            </nav>
            <button
              onClick={handleSchedule}
              className="bg-vintage-red text-vintage-cream px-8 py-3 rounded-lg font-bold hover:bg-vintage-red/90 transition-all shadow-lg hover:shadow-xl transform hover:-translate-y-0.5"
            >
              Agendar Agora
            </button>
          </div>
        </div>
        {/* Ornamental border */}
        <div className="absolute bottom-0 left-0 right-0 h-2 bg-gradient-to-r from-vintage-brown via-vintage-red to-vintage-brown"></div>
      </header>

      {/* Hero Section */}
      <section className="relative py-20 px-6 bg-gradient-to-b from-vintage-brown/10 to-vintage-cream">
        <div className="container mx-auto text-center">
          <div className="max-w-4xl mx-auto">
            <h2 className="text-6xl md:text-8xl font-vintage-display font-bold text-vintage-brown mb-6 tracking-wider">
              {barbershop.name}
            </h2>
            <div className="flex items-center justify-center gap-4 mb-8">
              <div className="h-px bg-vintage-brown flex-1 max-w-20"></div>
              <Scissors className="text-vintage-red" size={32} />
              <div className="h-px bg-vintage-brown flex-1 max-w-20"></div>
            </div>
            <p className="text-xl text-vintage-brown/80 mb-12 font-semibold">
              Experiência autêntica dos anos 50 • Cortes clássicos • Ambiente acolhedor
            </p>
            <button
              onClick={handleSchedule}
              className="bg-vintage-red text-vintage-cream px-12 py-4 rounded-lg text-xl font-bold hover:bg-vintage-red/90 transition-all shadow-lg hover:shadow-xl transform hover:-translate-y-1"
            >
              Agendar Seu Corte
            </button>
          </div>
        </div>
      </section>

      {/* Services Section - Vintage List Style */}
      <section id="servicos" className="py-20 px-6 bg-vintage-cream">
        <div className="container mx-auto max-w-4xl">
          <div className="text-center mb-16">
            <h2 className="text-5xl font-vintage-display font-bold text-vintage-brown mb-4">
              Nossos Serviços
            </h2>
            <div className="w-32 h-1 bg-vintage-red mx-auto mb-6"></div>
            <p className="text-vintage-brown/70 text-lg">
              Cortes e serviços com o charme dos anos dourados
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {landingPage.services.map((service) => (
              <div
                key={service.id}
                className="relative bg-white rounded-lg shadow-lg p-6 border-2 border-vintage-brown/20 hover:border-vintage-red/50 transition-all hover:shadow-xl"
              >
                {/* Ornamental corner */}
                <div className="absolute top-2 right-2 w-4 h-4 border-t-2 border-r-2 border-vintage-red"></div>

                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-xl font-bold text-vintage-brown">{service.name}</h3>
                  <input
                    type="checkbox"
                    checked={selectedIds.has(service.id)}
                    onChange={() => toggleService(service.id)}
                    className="w-6 h-6 text-vintage-red bg-vintage-cream border-vintage-brown rounded focus:ring-vintage-red"
                  />
                </div>

                {service.description && (
                  <p className="text-vintage-brown/70 mb-4 text-sm">{service.description}</p>
                )}

                {/* Vintage dotted line separator */}
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2 text-vintage-brown/60">
                    <Clock size={16} />
                    <span className="text-sm">{service.duration}min</span>
                  </div>
                  <div className="flex-1 mx-4 border-t-2 border-dotted border-vintage-brown/30"></div>
                  <div className="text-xl font-vintage-display font-bold text-vintage-red">
                    R$ {service.price.toFixed(2)}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* About Section */}
      {landingPage.aboutText && (
        <section id="sobre" className="py-20 px-6 bg-vintage-brown text-vintage-cream">
          <div className="container mx-auto max-w-4xl text-center">
            <h2 className="text-5xl font-vintage-display font-bold mb-8">
              Sobre a {barbershop.name}
            </h2>
            <div className="w-32 h-1 bg-vintage-red mx-auto mb-12"></div>
            <div className="bg-vintage-cream/10 backdrop-blur-sm rounded-lg p-8 border border-vintage-cream/20">
              <p className="text-lg leading-relaxed whitespace-pre-line text-vintage-cream/90">
                {landingPage.aboutText}
              </p>
            </div>
          </div>
        </section>
      )}

      {/* Contact Section */}
      <section id="contato" className="py-20 px-6 bg-vintage-cream">
        <div className="container mx-auto max-w-4xl">
          <div className="text-center mb-16">
            <h2 className="text-5xl font-vintage-display font-bold text-vintage-brown mb-4">
              Onde Estamos
            </h2>
            <div className="w-32 h-1 bg-vintage-red mx-auto"></div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
            <div className="bg-white rounded-lg shadow-lg p-8 border-2 border-vintage-brown/20">
              <div className="flex items-start gap-4">
                <div className="bg-vintage-red text-vintage-cream p-3 rounded-full">
                  <MapPin size={24} />
                </div>
                <div>
                  <h3 className="text-xl font-bold text-vintage-brown mb-2">Endereço</h3>
                  <p className="text-vintage-brown/70 leading-relaxed">{barbershop.address}</p>
                </div>
              </div>
            </div>

            {landingPage.openingHours && (
              <div className="bg-white rounded-lg shadow-lg p-8 border-2 border-vintage-brown/20">
                <div className="flex items-start gap-4">
                  <div className="bg-vintage-red text-vintage-cream p-3 rounded-full">
                    <Clock size={24} />
                  </div>
                  <div>
                    <h3 className="text-xl font-bold text-vintage-brown mb-2">Horário</h3>
                    <p className="text-vintage-brown/70 leading-relaxed whitespace-pre-line">
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
              <h3 className="text-2xl font-bold text-vintage-brown mb-8">Nos Acompanhe</h3>
              <div className="flex justify-center gap-8">
                {landingPage.instagramUrl && (
                  <a
                    href={landingPage.instagramUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="bg-vintage-brown text-vintage-cream p-4 rounded-full hover:bg-vintage-red transition-all shadow-lg hover:shadow-xl transform hover:-translate-y-1"
                  >
                    <Instagram size={32} />
                  </a>
                )}
                {landingPage.facebookUrl && (
                  <a
                    href={landingPage.facebookUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="bg-vintage-brown text-vintage-cream p-4 rounded-full hover:bg-vintage-red transition-all shadow-lg hover:shadow-xl transform hover:-translate-y-1"
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
      <footer className="bg-vintage-brown text-vintage-cream py-12 px-6">
        <div className="container mx-auto text-center">
          <div className="flex items-center justify-center gap-4 mb-6">
            <div className="h-px bg-vintage-red flex-1 max-w-20"></div>
            <Scissors className="text-vintage-red" size={24} />
            <div className="h-px bg-vintage-red flex-1 max-w-20"></div>
          </div>
          <p className="text-lg font-semibold mb-4">© 2025 {barbershop.name}</p>
          <p className="text-vintage-cream/70 mb-6">Tradição e estilo desde sempre</p>
          <a
            href="/admin/login"
            className="text-vintage-cream/60 hover:text-vintage-red transition-colors text-sm"
          >
            Área Administrativa
          </a>
        </div>
      </footer>

      {/* Floating WhatsApp Button */}
      <WhatsAppButton
        phoneNumber={landingPage.whatsappNumber}
        floating
        className="bg-vintage-red hover:bg-vintage-red/90"
      />

      {/* Floating Schedule Button */}
      {hasSelection && (
        <button
          onClick={handleSchedule}
          className="fixed bottom-24 right-6 bg-vintage-red text-vintage-cream px-8 py-4 rounded-full shadow-lg hover:bg-vintage-red/90 transition-all hover:scale-105 z-50 font-bold"
        >
          Agendar {selectedIds.size} serviço{selectedIds.size > 1 ? 's' : ''} • R$ {totalPrice.toFixed(2)}
        </button>
      )}
    </div>
  );
};