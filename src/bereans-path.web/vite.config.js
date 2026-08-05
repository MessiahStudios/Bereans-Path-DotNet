import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    vue(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.svg', 'icons.svg', 'bereans-path-logo.png'],
      manifest: {
        name: 'Bereans Path',
        short_name: 'Bereans',
        description: 'Searching the Scriptures daily to find the truth. Acts 17:11',
        theme_color: '#c4a574',
        background_color: '#0c0b0a',
        display: 'standalone',
        start_url: '/',
        icons: [
          {
            src: 'bereans-path-logo.png',
            sizes: '512x512',
            type: 'image/png',
            purpose: 'any',
          },
          {
            src: 'bereans-path-logo.png',
            sizes: '512x512',
            type: 'image/png',
            purpose: 'maskable',
          },
        ],
      },
      workbox: {
        navigateFallback: '/index.html',
        runtimeCaching: [
          {
            // Church search can take 15–60s while Overpass mirrors recover.
            urlPattern: ({ url }) => url.pathname.startsWith('/api/churches/nearby'),
            handler: 'NetworkOnly',
          },
          {
            urlPattern: ({ url }) => url.pathname.startsWith('/api/'),
            handler: 'NetworkFirst',
            options: {
              cacheName: 'bereans-api',
              networkTimeoutSeconds: 60,
            },
          },
        ],
      },
    }),
  ],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5068',
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: '../BereansPath.Api/wwwroot',
    emptyOutDir: true,
  },
})
