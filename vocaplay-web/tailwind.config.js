/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        mint: {
          50: '#F4FBF6',
          100: '#E3F6E8',
          200: '#CDEFD6',
          300: '#AEE3BD',
          400: '#86D29F',
          500: '#5FBE82',
          600: '#469E68',
        },
        lavender: {
          50: '#F8F6FD',
          100: '#EFE9FB',
          200: '#DFD3F5',
          300: '#C9B6EC',
          400: '#AF94DF',
          500: '#9575CD',
          600: '#7C5BB3',
        },
        cream: {
          50: '#FFFDF9',
          100: '#FDF6EC',
          200: '#FAEDD9',
          300: '#F3DFBC',
        },
        coral: {
          50: '#FFF1EE',
          100: '#FFE1DA',
          300: '#FFA391',
          400: '#FF8169',
          500: '#FF6B52',
          600: '#F0502F',
          700: '#D43F22',
        },
        ink: {
          400: '#8B8696',
          500: '#6B6678',
          600: '#4F4B5C',
          700: '#363347',
        },
      },
      fontFamily: {
        display: ['Quicksand', 'system-ui', 'sans-serif'],
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      borderRadius: {
        bento: '16px',
      },
      boxShadow: {
        soft: '0 2px 8px rgba(80, 70, 110, 0.06), 0 1px 2px rgba(80, 70, 110, 0.04)',
        'soft-lg': '0 8px 24px rgba(80, 70, 110, 0.10), 0 2px 6px rgba(80, 70, 110, 0.06)',
        'soft-hover': '0 12px 32px rgba(80, 70, 110, 0.14), 0 4px 10px rgba(80, 70, 110, 0.08)',
      },
      keyframes: {
        'pop-in': {
          '0%': { opacity: 0, transform: 'scale(0.96) translateY(4px)' },
          '100%': { opacity: 1, transform: 'scale(1) translateY(0)' },
        },
      },
      animation: {
        'pop-in': 'pop-in 0.25s ease-out',
      },
    },
  },
  plugins: [],
}
