import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import { createApp } from 'vue'

import '@fontsource-variable/bricolage-grotesque'
import '@fontsource-variable/figtree'
import 'primeicons/primeicons.css'
import './styles/main.css'

import App from './App.vue'
import { router } from './router'
import { RestoTheme } from './theme'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: RestoTheme,
    // Light theme only: this class is never added, so dark mode stays off
    options: { darkModeSelector: '.app-dark' },
  },
})
app.use(ToastService)

app.mount('#app')
