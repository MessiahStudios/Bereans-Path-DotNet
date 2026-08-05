<script setup>
import { ref } from 'vue'
import { RouterLink } from 'vue-router'
import ElevenLabsAudioCard from '../components/ElevenLabsAudioCard.vue'
import {
  EXTERNAL_RESOURCES,
  NEW_BELIEVER_AUDIO,
  STUDY_SUMMARY,
  STUDY_TOPICS,
} from '../data/resources'
import { PATH_TAGLINE } from '../data/pathCopy'

const openTopicId = ref(null)
const showMethod = ref(false)

function toggleTopic(id) {
  openTopicId.value = openTopicId.value === id ? null : id
}
</script>

<template>
  <div class="d-grid gap-3">
    <section class="panel">
      <h2 class="h4 mb-1">Resources</h2>
      <p class="muted mb-2">{{ PATH_TAGLINE }}</p>
      <p class="mb-0">
        Start with a clear word about Jesus. When you are ready, open the study approach behind Bereans Path —
        <RouterLink to="/path">or read Why this app exists</RouterLink>.
      </p>
    </section>

    <section class="panel">
      <h3 class="h5 mb-1">For new believers</h3>
      <p class="muted mb-3">Listen at your own pace. These clips introduce Jesus clearly and gently.</p>
      <div class="d-grid gap-2">
        <ElevenLabsAudioCard
          v-for="clip in NEW_BELIEVER_AUDIO"
          :key="clip.id"
          :title="clip.title"
          :description="clip.description"
          :public-user-id="clip.publicUserId"
          :project-id="clip.projectId"
        />
      </div>
    </section>

    <section class="panel">
      <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-2">
        <div>
          <h3 class="h5 mb-1">Bible study methodology</h3>
          <p class="muted mb-0">
            Six steps that keep us under Scripture — clear, careful, and ready to share.
          </p>
        </div>
        <button class="btn btn-outline-secondary btn-sm" type="button" @click="showMethod = !showMethod">
          {{ showMethod ? 'Hide details' : 'Show details' }}
        </button>
      </div>

      <ul class="method-summary mb-0">
        <li v-for="line in STUDY_SUMMARY" :key="line">{{ line }}</li>
      </ul>

      <div v-if="showMethod" class="method-details mt-3">
        <div class="d-grid gap-2">
          <article v-for="topic in STUDY_TOPICS" :key="topic.id" class="card-item topic-card">
            <button class="topic-toggle" type="button" @click="toggleTopic(topic.id)">
              <strong>{{ topic.title }}</strong>
              <span>{{ openTopicId === topic.id ? '−' : '+' }}</span>
            </button>
            <div v-if="openTopicId === topic.id" class="topic-body">
              <p><strong>What it is:</strong> {{ topic.what }}</p>
              <p><strong>Why we use it:</strong> {{ topic.why }}</p>
              <p class="mb-0"><strong>How it helps:</strong> {{ topic.help }}</p>
            </div>
          </article>
        </div>
      </div>
    </section>

    <section class="panel">
      <h3 class="h5 mb-3">Trusted external resources</h3>
      <div v-for="group in EXTERNAL_RESOURCES" :key="group.group" class="mb-3">
        <h4 class="h6 muted mb-2">{{ group.group }}</h4>
        <div class="d-grid gap-2">
          <a
            v-for="link in group.links"
            :key="link.href"
            class="resource-link"
            :href="link.href"
            target="_blank"
            rel="noopener noreferrer"
          >
            <span>{{ link.name }}</span>
            <span aria-hidden="true">→</span>
          </a>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.method-summary {
  margin: 0;
  padding-left: 1.15rem;
  color: var(--bp-muted);
  line-height: 1.55;
}

.topic-toggle {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  border: 0;
  background: transparent;
  color: var(--bp-ink);
  text-align: left;
  padding: 0;
}

.topic-body {
  margin-top: 0.85rem;
  color: var(--bp-ink);
}

.topic-body p {
  margin-bottom: 0.65rem;
}

.resource-link {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  padding: 0.85rem 1rem;
  border-radius: 0.75rem;
  border: 1px solid var(--bp-border);
  background: var(--bp-elevated);
  color: var(--bp-ink);
  text-decoration: none;
  font-weight: 600;
}

.resource-link:hover {
  border-color: var(--bp-accent);
  color: var(--bp-accent);
}
</style>
