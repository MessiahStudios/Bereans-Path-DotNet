<script setup>
import { RouterLink } from 'vue-router'
import { PATH_ONE_LINER, PATH_TAGLINE } from '../data/pathCopy'

defineProps({
  /** Optional extra line under the one-liner (e.g. church-page tip). */
  aside: { type: String, default: '' },
  showPathLink: { type: Boolean, default: true },
})
</script>

<template>
  <aside class="mission-strip" aria-label="What Bereans Path is">
    <p class="mission-tagline mb-1">{{ PATH_TAGLINE }}</p>
    <p class="mission-body mb-1">{{ PATH_ONE_LINER }}</p>
    <p v-if="aside" class="mission-aside mb-1">{{ aside }}</p>
    <RouterLink v-if="showPathLink" class="mission-link" to="/path">
      More on the Path →
    </RouterLink>
  </aside>
</template>

<style scoped>
.mission-strip {
  padding: 0.85rem 1rem;
  border-left: 2px solid var(--bp-accent, var(--bp-gold, #c4a574));
  color: var(--bp-muted, var(--bp-ink-muted, #8a8178));
  line-height: 1.5;
}

.mission-tagline {
  color: var(--bp-ink, #f3ece4);
  font-weight: 600;
  font-size: 0.98rem;
}

.mission-body,
.mission-aside {
  max-width: 46rem;
  font-size: 0.95rem;
  margin-bottom: 0;
}

.mission-aside {
  opacity: 0.92;
}

.mission-link {
  display: inline-block;
  margin-top: 0.35rem;
  color: var(--bp-accent, var(--bp-gold, #c4a574));
  font-weight: 600;
  text-decoration: none;
}

.mission-link:hover {
  color: var(--bp-accent-deep, #a88655);
  text-decoration: underline;
}
</style>
