<script setup>
import { onMounted, ref } from 'vue'
import { deleteBookmark, listBookmarks } from '../api'

const bookmarks = ref([])
const error = ref('')
const loading = ref(true)

async function refresh() {
  loading.value = true
  error.value = ''
  try {
    bookmarks.value = await listBookmarks()
  } catch (err) {
    error.value = err.message
  } finally {
    loading.value = false
  }
}

async function remove(id) {
  error.value = ''
  try {
    await deleteBookmark(id)
    bookmarks.value = bookmarks.value.filter((b) => b.id !== id)
  } catch (err) {
    error.value = err.message
  }
}

onMounted(refresh)
</script>

<template>
  <section class="panel">
    <div class="d-flex justify-content-between align-items-center mb-3">
      <div>
        <h1 class="h4 mb-1">Bookmarks</h1>
        <p class="muted mb-0">Stored with Entity Framework (SQLite locally; SQL Server ready).</p>
      </div>
      <button class="btn btn-outline-secondary btn-sm" type="button" @click="refresh">Refresh</button>
    </div>

    <div v-if="error" class="alert alert-warning py-2">{{ error }}</div>
    <p v-if="loading" class="text-secondary">Loading…</p>

    <div v-else-if="!bookmarks.length" class="muted">
      No bookmarks yet. Save one from the Reader.
    </div>

    <div v-else class="d-grid gap-3">
      <article v-for="item in bookmarks" :key="item.id" class="border rounded-3 p-3 bg-white">
        <div class="d-flex justify-content-between gap-3">
          <div>
            <h2 class="h6 mb-1">{{ item.reference }}</h2>
            <p class="small muted mb-2">{{ new Date(item.createdAt).toLocaleString() }}</p>
            <p class="mb-0 passage-text">{{ item.passageText }}</p>
            <p v-if="item.note" class="mt-2 mb-0"><strong>Note:</strong> {{ item.note }}</p>
          </div>
          <button class="btn btn-sm btn-outline-danger align-self-start" type="button" @click="remove(item.id)">
            Delete
          </button>
        </div>
      </article>
    </div>
  </section>
</template>
