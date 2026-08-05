<script setup>
import { nextTick, onMounted, onUnmounted, ref } from 'vue'
import { clearLogs, fetchHealth, fetchLogs } from '../api'

const health = ref(null)
const lines = ref([])
const logFile = ref('')
const error = ref('')
const status = ref('')
const autoRefresh = ref(true)
const logBox = ref(null)
let timer

async function refresh() {
  error.value = ''
  try {
    const [healthData, logData] = await Promise.all([
      fetchHealth(),
      fetchLogs(250),
    ])
    health.value = healthData
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

async function onClear() {
  try {
    await clearLogs()
    status.value = 'Buffer cleared.'
    await refresh()
  } catch (err) {
    error.value = err.message
  }
}

onMounted(async () => {
  await refresh()
  timer = setInterval(() => {
    if (autoRefresh.value) refresh()
  }, 3000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>

<template>
  <section class="panel">
    <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
      <div>
        <h1 class="h4 mb-1">Diagnostics / logs</h1>
        <p class="muted mb-0">Live API activity so we can see requests, ESV, and errors.</p>
      </div>
      <div class="d-flex flex-wrap gap-2">
        <div class="form-check form-switch align-self-center me-2">
          <input id="autoRefresh" v-model="autoRefresh" class="form-check-input" type="checkbox" />
          <label class="form-check-label" for="autoRefresh">Auto-refresh</label>
        </div>
        <button class="btn btn-outline-secondary btn-sm" type="button" @click="refresh">Refresh</button>
        <button class="btn btn-outline-danger btn-sm" type="button" @click="onClear">Clear buffer</button>
      </div>
    </div>

    <div v-if="error" class="alert alert-warning py-2">{{ error }}</div>
    <p v-if="status" class="text-secondary">{{ status }}</p>

    <div v-if="health" class="row g-2 mb-3">
      <div class="col-md-3">
        <div class="border rounded-3 p-2 bg-white h-100">
          <div class="small muted">Environment</div>
          <strong>{{ health.environment }}</strong>
        </div>
      </div>
      <div class="col-md-3">
        <div class="border rounded-3 p-2 bg-white h-100">
          <div class="small muted">ESV API key</div>
          <strong :class="health.esvApiKeyConfigured ? 'text-success' : 'text-danger'">
            {{ health.esvApiKeyConfigured ? 'Configured' : 'Missing' }}
          </strong>
        </div>
      </div>
      <div class="col-md-3">
        <div class="border rounded-3 p-2 bg-white h-100">
          <div class="small muted">Database</div>
          <strong>{{ health.databaseProvider }}</strong>
        </div>
      </div>
      <div class="col-md-3">
        <div class="border rounded-3 p-2 bg-white h-100">
          <div class="small muted">Log lines</div>
          <strong>{{ lines.length }}</strong>
        </div>
      </div>
    </div>

    <p v-if="logFile" class="small muted mb-2">File: <code>{{ logFile }}</code></p>

    <pre ref="logBox" class="log-console mb-0">{{ lines.join('\n') || 'No log lines yet. Use the app, then refresh.' }}</pre>
  </section>
</template>

<style scoped>
.log-console {
  max-height: 520px;
  overflow: auto;
  margin: 0;
  padding: 1rem;
  border-radius: 0.75rem;
  background: #15202b;
  color: #d7e2ec;
  font-size: 0.8rem;
  line-height: 1.45;
  white-space: pre-wrap;
  word-break: break-word;
}
</style>
