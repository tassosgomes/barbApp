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
      },
      fontFamily: {
        serif: ['Playfair Display', 'serif'],
        sans: ['Inter', 'sans-serif'],
        // Template 2 - Modern fonts
        'modern-sans': ['Montserrat', 'Poppins', 'Inter', 'sans-serif'],
      },
    },
  },
  plugins: [],
};
