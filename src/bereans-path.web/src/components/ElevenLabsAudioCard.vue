<script setup>
import { computed } from 'vue'

const props = defineProps({
  title: { type: String, required: true },
  description: { type: String, default: '' },
  publicUserId: { type: String, required: true },
  projectId: { type: String, required: true },
})

// Build the same iframe URL ElevenLabs' helper would create.
// (Their script only auto-runs once on load, which breaks Vue open/close mounts.)
const playerSrc = computed(() => {
  const params = new URLSearchParams({
    publicUserId: props.publicUserId,
    projectId: props.projectId,
    small: 'True',
    textColor: 'rgba(243, 236, 228, 1.0)',
    backgroundColor: 'rgba(28, 24, 20, 1.0)',
  })
  return `https://elevenlabs.io/player/index.html?${params.toString()}`
})
</script>

<template>
  <div class="audio-card card-item">
    <div class="audio-heading mb-2">
      <strong>{{ title }}</strong>
      <span v-if="description" class="d-block small muted">{{ description }}</span>
    </div>
    <iframe
      class="audio-frame"
      :src="playerSrc"
      :title="title"
      loading="lazy"
      allow="autoplay"
    />
  </div>
</template>

<style scoped>
.audio-heading {
  color: var(--bp-ink);
}

.audio-frame {
  display: block;
  width: 100%;
  height: 90px;
  max-height: 90px;
  border: 1px solid var(--bp-border);
  border-radius: 0.65rem;
  background: var(--bp-elevated);
}
</style>
