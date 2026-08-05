import { createApp } from 'vue'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'leaflet/dist/leaflet.css'
import './assets/main.css'
import { applyTheme, getSavedTheme } from './themes'
import App from './App.vue'
import router from './router'

applyTheme(getSavedTheme())

createApp(App).use(router).mount('#app')

// Register service worker when the PWA plugin is available (dev + prod).
import('virtual:pwa-register')
  .then(({ registerSW }) => registerSW({ immediate: true }))
  .catch(() => {
    // Dev servers started without vite-plugin-pwa can ignore this.
  })
