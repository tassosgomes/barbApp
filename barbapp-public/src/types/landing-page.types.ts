// Landing page types will be defined here
export interface PublicLandingPage {
  barbershop: {
    id: string;
    name: string;
    code: string;
    address: string;
  };
  landingPage: {
    templateId: number;
    logoUrl?: string;
    aboutText?: string;
    openingHours?: string;
    instagramUrl?: string;
    facebookUrl?: string;
    whatsappNumber: string;
    services: PublicService[];
  };
}

export interface PublicService {
  id: string;
  name: string;
  description?: string;
  duration: number;
  price: number;
}
