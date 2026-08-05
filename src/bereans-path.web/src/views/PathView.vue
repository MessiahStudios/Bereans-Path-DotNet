<script setup>
import { RouterLink } from 'vue-router'
import {
  PATH_ACTS,
  PATH_CHURCH_NOTE,
  PATH_PILLARS,
  PATH_PROBLEM,
  PATH_SOLUTION,
  PATH_TAGLINE,
  MAIN_SITE_LABEL,
  MAIN_SITE_URL,
} from '../data/pathCopy'
</script>

<template>
  <div class="d-grid gap-3">
    <section class="panel path-hero">
      <p class="path-kicker mb-2">What &amp; why</p>
      <h2 class="h3 mb-2">The Bereans Path</h2>
      <p class="path-lead mb-3">{{ PATH_TAGLINE }}</p>
      <blockquote class="path-verse mb-0">{{ PATH_ACTS }}</blockquote>
    </section>

    <section class="panel">
      <h3 class="h5 mb-2">The problem</h3>
      <p class="mb-3">{{ PATH_PROBLEM }}</p>
      <h3 class="h5 mb-2">What this app solves</h3>
      <p class="mb-0">{{ PATH_SOLUTION }}</p>
    </section>

    <section class="panel">
      <h3 class="h5 mb-3">How you use it</h3>
      <div class="path-pillars">
        <article v-for="pillar in PATH_PILLARS" :key="pillar.title" class="path-pillar">
          <h4 class="h6 mb-1">{{ pillar.title }}</h4>
          <p class="muted mb-0">{{ pillar.body }}</p>
        </article>
      </div>
    </section>

    <section class="panel">
      <h3 class="h5 mb-2">Churches on this path</h3>
      <p class="mb-3">{{ PATH_CHURCH_NOTE }}</p>
      <div class="d-flex flex-wrap gap-2">
        <RouterLink class="btn btn-primary" to="/churches">Find churches</RouterLink>
        <RouterLink class="btn btn-outline-primary" to="/resources">Study resources</RouterLink>
        <RouterLink class="btn btn-outline-secondary" to="/">Open the reader</RouterLink>
        <a class="btn btn-outline-secondary" :href="MAIN_SITE_URL">
          ← {{ MAIN_SITE_LABEL }}
        </a>
      </div>
    </section>
  </div>
</template>

<style scoped>
.path-kicker {
  font-size: 0.8rem;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--bp-accent);
  font-weight: 600;
}

.path-lead {
  font-family: var(--bp-font-display);
  font-size: 1.45rem;
  line-height: 1.35;
  color: var(--bp-ink);
  max-width: 36rem;
}

.path-verse {
  margin: 0;
  padding: 0.85rem 1rem;
  border-left: 2px solid var(--bp-accent);
  color: var(--bp-muted);
  font-style: italic;
  line-height: 1.55;
  max-width: 40rem;
}

.path-pillars {
  display: grid;
  gap: 0.85rem;
}

@media (min-width: 768px) {
  .path-pillars {
    grid-template-columns: 1fr 1fr;
  }
}

.path-pillar {
  padding: 0.9rem 1rem;
  border: 1px solid var(--bp-border);
  border-radius: 0.75rem;
  background: var(--bp-elevated);
}

.path-pillar h4 {
  font-family: var(--bp-font-display);
  color: var(--bp-accent);
}
</style>
