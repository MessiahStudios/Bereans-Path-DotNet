<script setup>
import { RouterLink, RouterView, useRoute } from 'vue-router'
import ThemeSwitcher from './components/ThemeSwitcher.vue'

const route = useRoute()

const primaryLinks = [
  { to: '/', label: 'Reader' },
  { to: '/bookmarks', label: 'Bookmarks' },
  { to: '/churches', label: 'Churches' },
  { to: '/resources', label: 'Resources' },
  { to: '/settings', label: 'Settings', fullLabel: 'System & Settings' },
]
</script>

<template>
  <div class="app-shell">
    <header class="app-header">
      <div>
        <RouterLink to="/" class="brand-link">
          <h1 class="brand-mark h3">Bereans <span>Path</span></h1>
        </RouterLink>
        <p class="muted mb-0">Scripture, bookmarks, and nearby churches</p>
      </div>
      <div class="header-actions">
        <ThemeSwitcher />
        <ul class="nav nav-pills desktop-nav">
          <li v-for="link in primaryLinks" :key="link.to" class="nav-item">
            <RouterLink
              class="nav-link"
              :class="{ active: route.path === link.to }"
              :to="link.to"
              :title="link.fullLabel || link.label"
            >
              {{ link.fullLabel || link.label }}
            </RouterLink>
          </li>
        </ul>
      </div>
    </header>

    <RouterView />

    <nav class="mobile-nav" aria-label="Primary">
      <RouterLink
        v-for="link in primaryLinks"
        :key="link.to"
        :class="{ active: route.path === link.to }"
        :to="link.to"
      >
        {{ link.label }}
      </RouterLink>
    </nav>
  </div>
</template>
