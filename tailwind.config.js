/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
      './Views/**/*.cshtml',
      './Pages/**/*.cshtml'
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
      },
    },
  },
  plugins: [],
}