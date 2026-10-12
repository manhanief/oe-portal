import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ConfirmationService from 'primevue/confirmationservice'
import ToastService from 'primevue/toastservice'
import { createApp } from 'vue'

import '@fontsource-variable/bricolage-grotesque'
import '@fontsource-variable/figtree'
import 'primeicons/primeicons.css'
import './styles/main.css'

import App from './App.vue'
import { setUnauthorizedHandler } from './api/http'
import { router } from './router'
import { useAuthStore } from './stores/auth'
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
app.use(ConfirmationService)

// If a staff session expires, go back to the login page (and return afterwards)
setUnauthorizedHandler(() => {
  useAuthStore().forget()
  const current = router.currentRoute.value
  if (current.meta.staff && !current.meta.public) {
    router.push({ name: 'staff-login', query: { redirect: current.fullPath } })
  }
})

app.mount('#app')
