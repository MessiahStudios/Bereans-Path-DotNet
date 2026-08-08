<script setup>
import { computed } from 'vue'
import { RouterLink, RouterView, useRoute } from 'vue-router'
import ThemeSwitcher from './components/ThemeSwitcher.vue'
import { MAIN_SITE_LABEL, MAIN_SITE_URL } from './data/pathCopy'
import { ESV_SITE_URL } from './data/esvCopy'

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
      <div class="brand-block">
        <a class="home-link" :href="MAIN_SITE_URL">
          ← {{ MAIN_SITE_LABEL }}
        </a>
        <RouterLink to="/" class="brand-link" aria-label="Bereans Path home">
          <img
            class="brand-logo"
            src="/bereans-path-logo.png"
            alt="Bereans Path — Searching the Scriptures daily to find the truth. Acts 17:11"
            width="220"
            height="220"
          />
        </RouterLink>
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

    <footer class="app-footer">
      <a :href="MAIN_SITE_URL">
        ← Back to {{ MAIN_SITE_LABEL }}
      </a>
      <span class="footer-sep" aria-hidden="true">·</span>
      <span>Bereans Path</span>
      <span class="footer-sep" aria-hidden="true">·</span>
      <a :href="ESV_SITE_URL" target="_blank" rel="noopener noreferrer">Scripture from ESV</a>
    </footer>

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
.brand-block {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 0.35rem;
}

.home-link {
  display: inline-block;
  color: var(--bp-muted);
  font-size: 0.85rem;
  font-weight: 600;
  text-decoration: none;
}

.home-link:hover {
  color: var(--bp-accent);
}

.brand-link {
  display: inline-block;
  text-decoration: none;
  line-height: 0;
}

.brand-logo {
  display: block;
  width: min(210px, 52vw);
  height: auto;
  border-radius: 0.85rem;
  background: #fff;
  box-shadow: 0 0 0 1px rgba(196, 165, 116, 0.28);
}

.settings-link:not(.active) {
  opacity: 0.72;
}

.app-footer {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.45rem;
  margin-top: 1.5rem;
  padding: 0.85rem 0 0.25rem;
  border-top: 1px solid var(--bp-border);
  color: var(--bp-muted);
  font-size: 0.9rem;
}

.app-footer a {
  color: var(--bp-accent);
  font-weight: 600;
  text-decoration: none;
}

.app-footer a:hover {
  text-decoration: underline;
}

.footer-sep {
  opacity: 0.5;
}

@media (max-width: 768px) {
  .brand-logo {
    width: min(150px, 46vw);
  }
}
</style>
