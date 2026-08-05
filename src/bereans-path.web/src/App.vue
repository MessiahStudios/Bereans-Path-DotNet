<script setup>
import { computed } from 'vue'
import { RouterLink, RouterView, useRoute } from 'vue-router'
import ThemeSwitcher from './components/ThemeSwitcher.vue'

const route = useRoute()

const primaryLinks = [
  { to: '/', label: 'Reader' },
  { to: '/bookmarks', label: 'Bookmarks' },
  { to: '/churches', label: 'Churches' },
  { to: '/path', label: 'Path' },
  { to: '/resources', label: 'Resources' },
]

const settingsActive = computed(() => route.path === '/settings')
</script>

<template>
  <div class="app-shell">
    <header class="app-header">
      <div>
        <RouterLink to="/" class="brand-link">
          <h1 class="brand-mark h3">Bereans <span>Path</span></h1>
        </RouterLink>
        <p class="muted mb-0 brand-tagline">Stay rooted in Scripture. Walk with a church that does the same.</p>
      </div>
      <div class="header-actions">
        <ThemeSwitcher />
        <ul class="nav nav-pills desktop-nav">
          <li v-for="link in primaryLinks" :key="link.to" class="nav-item">
            <RouterLink
              class="nav-link"
              :class="{ active: route.path === link.to }"
              :to="link.to"
            >
              {{ link.label }}
            </RouterLink>
          </li>
          <li class="nav-item">
            <RouterLink
              class="nav-link settings-link"
              :class="{ active: settingsActive }"
              to="/settings"
              title="Settings"
            >
              Settings
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
      <RouterLink :class="{ active: settingsActive }" to="/settings" title="Settings">
        ···
      </RouterLink>
    </nav>
  </div>
</template>

<style scoped>
.brand-tagline {
  max-width: 22rem;
  line-height: 1.35;
}

.settings-link:not(.active) {
  opacity: 0.72;
}
</style>
