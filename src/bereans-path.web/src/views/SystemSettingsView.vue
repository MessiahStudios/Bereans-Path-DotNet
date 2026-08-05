<script setup>
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import ThemeSwitcher from '../components/ThemeSwitcher.vue'
import { THEMES, applyTheme, getSavedTheme } from '../themes'
import { clearLogs, fetchHealth, fetchLogs } from '../api'
import { MAIN_SITE_LABEL, MAIN_SITE_URL } from '../data/pathCopy'

const LOGS_VISIBLE_KEY = 'bereans-show-logs'
const AUTO_REFRESH_KEY = 'bereans-logs-auto-refresh'

const health = ref(null)
const lines = ref([])
const logFile = ref('')
const error = ref('')
const status = ref('')
const currentTheme = ref(getSavedTheme())
const showLogs = ref(localStorage.getItem(LOGS_VISIBLE_KEY) === 'true')
const autoRefresh = ref(localStorage.getItem(AUTO_REFRESH_KEY) !== 'false')
const logBox = ref(null)
let timer

const themeName = computed(() => THEMES[currentTheme.value]?.name || currentTheme.value)

async function refreshHealth() {
  error.value = ''
  try {
    health.value = await fetchHealth()
  } catch (err) {
    error.value = err.message
  }
}

async function refreshLogs() {
  error.value = ''
  try {
    const logData = await fetchLogs(250)
    lines.value = logData.lines ?? []
    logFile.value = logData.logFile ?? ''
    await nextTick()
    if (logBox.value) {
      logBox.value.scrollTop = logBox.value.scrollHeight
    }
  } catch (err) {
    error.value = err.message
  }
}

async function refreshAll() {
  await refreshHealth()
  if (showLogs.value) await refreshLogs()
}

async function onClear() {
  try {
    await clearLogs()
    status.value = 'Log buffer cleared.'
    await refreshLogs()
  } catch (err) {
    error.value = err.message
  }
}

function onThemeChanged() {
  currentTheme.value = getSavedTheme()
}

function resetTheme() {
  currentTheme.value = applyTheme('faith')
  window.dispatchEvent(new CustomEvent('bereans-theme-changed', { detail: 'faith' }))
  status.value = 'Theme reset to Faith & Scripture.'
}

watch(showLogs, async (visible) => {
  localStorage.setItem(LOGS_VISIBLE_KEY, String(visible))
  if (visible) await refreshLogs()
})

watch(autoRefresh, (value) => {
  localStorage.setItem(AUTO_REFRESH_KEY, String(value))
})

onMounted(async () => {
  window.addEventListener('bereans-theme-changed', onThemeChanged)
  await refreshAll()
  timer = setInterval(() => {
    if (showLogs.value && autoRefresh.value) refreshLogs()
  }, 3000)
})

onUnmounted(() => {
  window.removeEventListener('bereans-theme-changed', onThemeChanged)
  if (timer) clearInterval(timer)
})
</script>

<template>
  <div class="d-grid gap-3">
    <section class="panel">
      <h2 class="h4 mb-1">System &amp; Settings</h2>
      <p class="muted mb-0">Appearance, connection status, and optional diagnostics.</p>
    </section>

    <section class="panel">
      <h3 class="h5 mb-2">Appearance</h3>
      <p class="muted mb-3">
        Current theme: <strong>{{ themeName }}</strong>
      </p>
      <div class="d-flex flex-wrap gap-2 align-items-center">
        <ThemeSwitcher />
        <button class="btn btn-sm btn-outline-secondary" type="button" @click="resetTheme">
          Reset to Faith &amp; Scripture
        </button>
      </div>
    </section>

    <section class="panel">
      <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
        <div>
          <h3 class="h5 mb-1">Connection</h3>
          <p class="muted mb-0">Whether the API and ESV key are ready.</p>
        </div>
        <button class="btn btn-outline-secondary btn-sm" type="button" @click="refreshHealth">Refresh</button>
      </div>

      <div v-if="error" class="alert alert-warning py-2">{{ error }}</div>
      <p v-if="status" class="text-secondary">{{ status }}</p>

      <div v-if="health" class="row g-2">
        <div class="col-sm-6 col-lg-3">
          <div class="stat-card">
            <div class="small muted">Environment</div>
            <strong>{{ health.environment }}</strong>
          </div>
        </div>
        <div class="col-sm-6 col-lg-3">
          <div class="stat-card">
            <div class="small muted">ESV API key</div>
            <strong :class="health.esvApiKeyConfigured ? 'text-success' : 'text-danger'">
              {{ health.esvApiKeyConfigured ? 'Configured' : 'Missing' }}
            </strong>
          </div>
        </div>
        <div class="col-sm-6 col-lg-3">
          <div class="stat-card">
            <div class="small muted">Database</div>
            <strong>{{ health.databaseProvider }}</strong>
          </div>
        </div>
        <div class="col-sm-6 col-lg-3">
          <div class="stat-card">
            <div class="small muted">Status</div>
            <strong>{{ health.status }}</strong>
          </div>
        </div>
      </div>
    </section>

    <section class="panel">
      <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
        <div>
          <h3 class="h5 mb-1">Diagnostics</h3>
          <p class="muted mb-0">Live API logs for debugging. Off by default.</p>
        </div>
        <div class="form-check form-switch m-0">
          <input id="showLogs" v-model="showLogs" class="form-check-input" type="checkbox" />
          <label class="form-check-label" for="showLogs">Show logs</label>
        </div>
      </div>

      <div v-if="showLogs">
        <div class="d-flex flex-wrap gap-2 mb-3">
          <div class="form-check form-switch align-self-center me-2">
            <input id="autoRefresh" v-model="autoRefresh" class="form-check-input" type="checkbox" />
            <label class="form-check-label" for="autoRefresh">Auto-refresh</label>
          </div>
          <button class="btn btn-outline-secondary btn-sm" type="button" @click="refreshLogs">Refresh</button>
          <button class="btn btn-outline-danger btn-sm" type="button" @click="onClear">Clear buffer</button>
        </div>

        <p v-if="logFile" class="small muted mb-2">File: <code>{{ logFile }}</code></p>
        <pre ref="logBox" class="log-console mb-0">{{ lines.join('\n') || 'No log lines yet. Use the app, then refresh.' }}</pre>
      </div>
    </section>

    <section class="panel">
      <h3 class="h5 mb-2">About</h3>
      <p class="mb-1">Bereans Path — stay rooted in Scripture and walk with a church that does the same.</p>
      <p class="muted mb-2">Built by Messiah Studios · Vue 3 + ASP.NET Core</p>
      <a class="btn btn-outline-secondary btn-sm" :href="MAIN_SITE_URL">
        ← Back to {{ MAIN_SITE_LABEL }}
      </a>
    </section>
  </div>
</template>

<style scoped>
.stat-card {
  background: var(--bp-elevated);
  border: 1px solid var(--bp-border);
  border-radius: 0.75rem;
  padding: 0.75rem;
  height: 100%;
}

.log-console {
  max-height: 420px;
  overflow: auto;
  margin: 0;
  padding: 1rem;
  border-radius: 0.75rem;
  background: #0c0b0a;
  color: #f3ece4;
  border: 1px solid var(--bp-border);
  font-size: 0.8rem;
  line-height: 1.45;
  white-space: pre-wrap;
  word-break: break-word;
}
</style>
