<script setup>
import { RouterLink, RouterView, useRoute } from 'vue-router'

const route = useRoute()

const links = [
  { to: '/', label: 'Reader' },
  { to: '/bookmarks', label: 'Bookmarks' },
  { to: '/churches', label: 'Churches' },
  { to: '/logs', label: 'Logs' },
]
</script>

<template>
  <div class="app-shell">
    <header class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
      <div>
        <div class="brand-mark h3 mb-1">Bereans <span>Path</span></div>
        <p class="muted mb-0">Scripture, bookmarks, and nearby churches</p>
      </div>
      <ul class="nav nav-pills">
        <li v-for="link in links" :key="link.to" class="nav-item">
          <RouterLink
            class="nav-link"
            :class="{ active: route.path === link.to }"
            :to="link.to"
          >
            {{ link.label }}
          </RouterLink>
        </li>
      </ul>
    </header>

    <RouterView />
  </div>
</template>
