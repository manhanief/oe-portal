import { definePreset } from '@primeuix/themes'
import Aura from '@primeuix/themes/aura'

/**
 * PrimeVue's "Aura" theme, recoloured with our pandan green.
 * 500 is the main brand colour; the other steps are lighter/darker shades of it.
 */
export const RestoTheme = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#edf6f0',
      100: '#d2e8da',
      200: '#a6cfb7',
      300: '#6fae8b',
      400: '#3e8a63',
      500: '#1f6b47',
      600: '#1a5b3d',
      700: '#164c33',
      800: '#123d29',
      900: '#0e3121',
      950: '#082016',
    },
  },
})
