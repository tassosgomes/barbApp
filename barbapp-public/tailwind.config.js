/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: '#D4AF37',
        gold: '#D4AF37',
        'gold-dark': '#B8941E',
        // Template 2 - Modern colors
        'modern-dark': '#2C3E50',
        'modern-blue': '#3498DB',
        'modern-light': '#ECF0F1',
        // Template 3 - Vintage colors
        'vintage-brown': '#5D4037',
        'vintage-cream': '#F5E6D3',
        'vintage-red': '#B71C1C',
        // Template 4 - Urban colors
        'urban-black': '#000000',
        'urban-red': '#E74C3C',
        'urban-gray': '#95A5A6',
      },
      fontFamily: {
        serif: ['Playfair Display', 'serif'],
        sans: ['Inter', 'sans-serif'],
        // Template 2 - Modern fonts
        'modern-sans': ['Montserrat', 'Poppins', 'Inter', 'sans-serif'],
        // Template 3 - Vintage fonts
        'vintage-display': ['Lobster', 'Bebas Neue', 'serif'],
        'vintage-sans': ['Inter', 'sans-serif'],
        // Template 4 - Urban fonts
        'urban-display': ['Bebas Neue', 'Oswald', 'sans-serif'],
        'urban-sans': ['Oswald', 'sans-serif'],
      },
    },
  },
  plugins: [],
};
