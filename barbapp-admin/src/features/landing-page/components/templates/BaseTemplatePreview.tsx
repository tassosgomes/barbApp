/**
 * BaseTemplatePreview Component
 * 
 * Componente base para preview de templates.
 * Este é um componente placeholder que renderiza uma visualização simplificada
 * da landing page com as informações principais.
 * 
 * Será substituído pela implementação completa dos templates quando disponíveis.
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import React from 'react';
import { MapPin, Clock, Phone, Instagram, Facebook, Scissors } from 'lucide-react';
import { LandingPageConfig } from '../../types/landing-page.types';

interface BaseTemplatePreviewProps {
  config: LandingPageConfig;
  templateName: string;
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
}

export const BaseTemplatePreview: React.FC<BaseTemplatePreviewProps> = ({
  config,
  templateName,
  primaryColor,
  secondaryColor,
  accentColor,
}) => {
  const visibleServices = config.services.filter((service) => service.isVisible);

  return (
    <div className="min-h-screen bg-white font-sans">
      {/* Header */}
      <header
        className="py-4 px-6 shadow-sm"
        style={{ backgroundColor: primaryColor }}
      >
        <div className="container mx-auto flex items-center justify-between">
          <div className="flex items-center gap-3">
            {config.logoUrl ? (
              <img
                src={config.logoUrl}
                alt="Logo"
                className="w-10 h-10 rounded-full object-cover"
              />
            ) : (
              <div
                className="w-10 h-10 rounded-full flex items-center justify-center"
                style={{ backgroundColor: secondaryColor }}
              >
                <Scissors size={20} style={{ color: accentColor }} />
              </div>
            )}
            <h1 className="text-xl font-bold" style={{ color: accentColor }}>
              Barbearia
            </h1>
          </div>
          <button
            className="px-4 py-2 rounded font-semibold text-sm"
            style={{
              backgroundColor: secondaryColor,
              color: primaryColor,
            }}
          >
            Agendar
          </button>
        </div>
      </header>

      {/* Hero Section */}
      <section
        className="py-16 px-6 text-center"
        style={{
          background: `linear-gradient(135deg, ${primaryColor} 0%, ${secondaryColor} 100%)`,
        }}
      >
        <div className="container mx-auto">
          <h2 className="text-4xl font-bold mb-4" style={{ color: accentColor }}>
            Barbearia
          </h2>
          <p className="text-lg mb-8" style={{ color: accentColor, opacity: 0.9 }}>
            Template {templateName}
          </p>
          <button
            className="px-8 py-3 rounded-lg text-lg font-semibold"
            style={{
              backgroundColor: accentColor,
              color: primaryColor,
            }}
          >
            Agendar Serviço
          </button>
        </div>
      </section>

      {/* Services Section */}
      {visibleServices.length > 0 && (
        <section className="py-12 px-6 bg-gray-50">
          <div className="container mx-auto">
            <h2
              className="text-3xl font-bold text-center mb-8"
              style={{ color: primaryColor }}
            >
              Nossos Serviços
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {visibleServices.slice(0, 6).map((service) => (
                <div
                  key={service.serviceId}
                  className="p-4 rounded-lg border-2 bg-white"
                  style={{ borderColor: accentColor }}
                >
                  <h3 className="font-semibold mb-2" style={{ color: primaryColor }}>
                    {service.serviceName}
                  </h3>
                  {service.description && (
                    <p className="text-sm text-gray-600 mb-2">{service.description}</p>
                  )}
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-gray-500">{service.duration}min</span>
                    <span
                      className="font-bold"
                      style={{ color: secondaryColor }}
                    >
                      R$ {service.price.toFixed(2)}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>
      )}

      {/* About Section */}
      {config.aboutText && (
        <section className="py-12 px-6">
          <div className="container mx-auto max-w-3xl">
            <h2
              className="text-3xl font-bold text-center mb-6"
              style={{ color: primaryColor }}
            >
              Sobre Nós
            </h2>
            <p className="text-gray-700 leading-relaxed text-center whitespace-pre-line">
              {config.aboutText}
            </p>
          </div>
        </section>
      )}

      {/* Contact Section */}
      <section className="py-12 px-6 bg-gray-50">
        <div className="container mx-auto max-w-4xl">
          <h2
            className="text-3xl font-bold text-center mb-8"
            style={{ color: primaryColor }}
          >
            Contato & Localização
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="flex items-start gap-3">
              <MapPin style={{ color: secondaryColor }} size={24} />
              <div>
                <h3 className="font-semibold mb-1">Endereço</h3>
                <p className="text-gray-600 text-sm">
                  Rua Exemplo, 123 - Centro
                </p>
              </div>
            </div>

            {config.openingHours && (
              <div className="flex items-start gap-3">
                <Clock style={{ color: secondaryColor }} size={24} />
                <div>
                  <h3 className="font-semibold mb-1">Horário</h3>
                  <p className="text-gray-600 text-sm whitespace-pre-line">
                    {config.openingHours}
                  </p>
                </div>
              </div>
            )}

            <div className="flex items-start gap-3">
              <Phone style={{ color: secondaryColor }} size={24} />
              <div>
                <h3 className="font-semibold mb-1">WhatsApp</h3>
                <p className="text-gray-600 text-sm">{config.whatsappNumber}</p>
              </div>
            </div>

            {(config.instagramUrl || config.facebookUrl) && (
              <div className="flex items-start gap-3">
                <div className="flex gap-2">
                  {config.instagramUrl && (
                    <Instagram style={{ color: secondaryColor }} size={24} />
                  )}
                  {config.facebookUrl && (
                    <Facebook style={{ color: secondaryColor }} size={24} />
                  )}
                </div>
                <div>
                  <h3 className="font-semibold mb-1">Redes Sociais</h3>
                  <p className="text-gray-600 text-sm">Siga-nos!</p>
                </div>
              </div>
            )}
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer
        className="py-6 px-6 text-center"
        style={{ backgroundColor: primaryColor }}
      >
        <p className="text-sm" style={{ color: accentColor, opacity: 0.8 }}>
          © 2025 Barbearia - Todos os direitos reservados
        </p>
        <a
          href="#"
          className="text-xs mt-2 inline-block"
          style={{ color: accentColor, opacity: 0.6 }}
        >
          Área Admin
        </a>
      </footer>
    </div>
  );
};

BaseTemplatePreview.displayName = 'BaseTemplatePreview';

export default BaseTemplatePreview;
