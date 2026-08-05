<script setup>
import { onMounted, onUnmounted, ref } from 'vue'
import { THEMES, applyTheme, getSavedTheme } from '../themes'

const open = ref(false)
const current = ref(getSavedTheme())
const root = ref(null)

function select(id) {
  current.value = applyTheme(id)
  open.value = false
  window.dispatchEvent(new CustomEvent('bereans-theme-changed', { detail: id }))
}

function onDocClick(e) {
  if (root.value && !root.value.contains(e.target)) {
    open.value = false
  }
}

onMounted(() => {
  current.value = applyTheme(getSavedTheme())
  document.addEventListener('click', onDocClick)
})

onUnmounted(() => {
  document.removeEventListener('click', onDocClick)
})
</script>

<template>
  <div ref="root" class="theme-switcher">
    <button
      class="btn btn-sm btn-outline-secondary"
      type="button"
      :aria-expanded="open"
      @click="open = !open"
    >
      Theme
    </button>
    <div v-if="open" class="theme-picker panel">
      <button
        v-for="(theme, id) in THEMES"
        :key="id"
        class="theme-swatch"
        :class="{ active: current === id }"
        type="button"
        @click="select(id)"
      >
        <span class="swatch-colors">
          <span class="swatch-bg" :style="{ background: theme.swatchBg }" />
          <span class="swatch-accent" :style="{ background: theme.swatchAccent }" />
        </span>
        <span>{{ theme.name }}</span>
      </button>
    </div>
  </div>
</template>

<style scoped>
.theme-switcher {
  position: relative;
}

.theme-picker {
  position: absolute;
  right: 0;
  top: calc(100% + 0.4rem);
  z-index: 30;
  width: min(280px, 80vw);
  display: grid;
  gap: 0.4rem;
  padding: 0.75rem;
}

.theme-swatch {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  width: 100%;
  border: 1px solid var(--bp-border);
  background: transparent;
  color: var(--bp-ink);
  border-radius: 0.65rem;
  padding: 0.45rem 0.55rem;
  text-align: left;
}

.theme-swatch.active {
  border-color: var(--bp-accent);
  background: color-mix(in srgb, var(--bp-accent) 14%, transparent);
}

.swatch-colors {
  display: inline-flex;
  width: 1.6rem;
  height: 1.6rem;
  border-radius: 999px;
  overflow: hidden;
  border: 1px solid var(--bp-border);
}

.swatch-bg,
.swatch-accent {
  flex: 1;
}
</style>
