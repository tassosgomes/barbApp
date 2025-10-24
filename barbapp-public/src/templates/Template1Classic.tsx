import React from 'react';
import { MapPin, Clock, Instagram, Facebook } from 'lucide-react';
import type { PublicLandingPage } from '@/types/landing-page.types';
import { ServiceCard } from '@/components/ServiceCard';
import { WhatsAppButton } from '@/components/WhatsAppButton';
import { useServiceSelection } from '@/hooks/useServiceSelection';
import { useNavigate } from 'react-router-dom';

interface Template1ClassicProps {
  data: PublicLandingPage;
}

export const Template1Classic: React.FC<Template1ClassicProps> = ({ data }) => {
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
    <div className="min-h-screen bg-white font-serif">
      {/* Header */}
      <header className="bg-black text-white py-4 px-6 sticky top-0 z-40">
        <div className="container mx-auto flex justify-between items-center">
          <div className="flex items-center gap-4">
            {landingPage.logoUrl && (
              <img
                src={landingPage.logoUrl}
                alt={barbershop.name}
                className="w-12 h-12 rounded-full"
              />
            )}
            <h1 className="text-2xl font-bold">{barbershop.name}</h1>
          </div>
          <nav className="hidden md:flex gap-6">
            <a href="#servicos" className="hover:text-gold transition">
              Serviços
            </a>
            <a href="#sobre" className="hover:text-gold transition">
              Sobre
            </a>
            <a href="#contato" className="hover:text-gold transition">
              Contato
            </a>
          </nav>
          <button
            onClick={handleSchedule}
            className="bg-gold text-black px-6 py-2 rounded font-semibold hover:bg-gold/90 transition"
          >
            Agendar Agora
          </button>
        </div>
      </header>

      {/* Hero Section */}
      <section className="relative h-[60vh] bg-gradient-to-r from-black to-gray-800 flex items-center justify-center text-white">
        <div className="text-center z-10">
          <h2 className="text-5xl font-bold mb-4">{barbershop.name}</h2>
          <p className="text-xl mb-8">Tradição e Elegância desde sempre</p>
          <button
            onClick={handleSchedule}
            className="bg-gold text-black px-8 py-3 rounded-lg text-lg font-semibold hover:bg-gold/90 transition"
          >
            Agendar Serviço
          </button>
        </div>
      </section>

      {/* Services Section */}
      <section id="servicos" className="py-16 px-6">
        <div className="container mx-auto">
          <h2 className="text-4xl font-bold text-center mb-12">
            Nossos Serviços
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {landingPage.services.map((service) => (
              <ServiceCard
                key={service.id}
                service={service}
                isSelected={selectedIds.has(service.id)}
                onToggle={() => toggleService(service.id)}
              />
            ))}
          </div>
        </div>
      </section>

      {/* About Section */}
      {landingPage.aboutText && (
        <section id="sobre" className="py-16 px-6 bg-gray-50">
          <div className="container mx-auto max-w-3xl text-center">
            <h2 className="text-4xl font-bold mb-8">Sobre Nós</h2>
            <p className="text-lg text-gray-700 leading-relaxed whitespace-pre-line">
              {landingPage.aboutText}
            </p>
          </div>
        </section>
      )}

      {/* Contact Section */}
      <section id="contato" className="py-16 px-6">
        <div className="container mx-auto max-w-4xl">
          <h2 className="text-4xl font-bold text-center mb-12">
            Onde Estamos
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div className="flex items-start gap-3">
              <MapPin className="text-gold flex-shrink-0" size={24} />
              <div>
                <h3 className="font-semibold mb-1">Endereço</h3>
                <p className="text-gray-600">{barbershop.address}</p>
              </div>
            </div>
            {landingPage.openingHours && (
              <div className="flex items-start gap-3">
                <Clock className="text-gold flex-shrink-0" size={24} />
                <div>
                  <h3 className="font-semibold mb-1">Horário</h3>
                  <p className="text-gray-600 whitespace-pre-line">
                    {landingPage.openingHours}
                  </p>
                </div>
              </div>
            )}
          </div>

          {/* Social Media */}
          {(landingPage.instagramUrl || landingPage.facebookUrl) && (
            <div className="mt-12 text-center">
              <h3 className="text-2xl font-semibold mb-6">Nos Acompanhe</h3>
              <div className="flex justify-center gap-6">
                {landingPage.instagramUrl && (
                  <a
                    href={landingPage.instagramUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-gold hover:text-gold/80 transition"
                  >
                    <Instagram size={32} />
                  </a>
                )}
                {landingPage.facebookUrl && (
                  <a
                    href={landingPage.facebookUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-gold hover:text-gold/80 transition"
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
      <footer className="bg-black text-white py-8 px-6 text-center">
        <p>© 2025 {barbershop.name} - Todos os direitos reservados</p>
        <a
          href="/admin/login"
          className="text-sm text-gray-400 hover:text-white mt-2 inline-block"
        >
          Área Admin
        </a>
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
          className="fixed bottom-24 right-6 bg-primary text-white px-6 py-3 rounded-full shadow-lg hover:scale-105 transition-transform z-50"
        >
          Agendar {selectedIds.size} serviço{selectedIds.size > 1 ? 's' : ''} •
          R$ {totalPrice.toFixed(2)}
        </button>
      )}
    </div>
  );
};