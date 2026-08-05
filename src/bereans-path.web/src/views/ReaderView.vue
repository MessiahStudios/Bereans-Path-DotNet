<script setup>
import { computed, onMounted, ref } from 'vue'
import { createBookmark, fetchPassage } from '../api'
import { BIBLE_BOOKS, dailyVerseReference } from '../bible'

const books = Object.keys(BIBLE_BOOKS)
const book = ref('John')
const chapter = ref(3)
const status = ref('')
const error = ref('')
const passageText = ref('')
const activeReference = ref('')
const saving = ref(false)

const chapters = computed(() => {
  const count = BIBLE_BOOKS[book.value] ?? 1
  return Array.from({ length: count }, (_, i) => i + 1)
})

async function loadPassage(reference) {
  error.value = ''
  status.value = 'Loading passage…'
  activeReference.value = reference
  try {
    const data = await fetchPassage(reference)
    passageText.value = Array.isArray(data?.passages) ? data.passages.join('\n\n') : ''
    if (!passageText.value) {
      throw new Error('No passage text returned. Check ESV_API_KEY on the API.')
    }
    status.value = ''
  } catch (err) {
    passageText.value = ''
    error.value = err.message
    status.value = ''
  }
}

async function onSearch() {
  await loadPassage(`${book.value} ${chapter.value}`)
}

async function onDaily() {
  book.value = 'John'
  await loadPassage(dailyVerseReference())
}

async function onBookmark() {
  if (!activeReference.value || !passageText.value) return
  saving.value = true
  error.value = ''
  try {
    await createBookmark({
      reference: activeReference.value,
      passageText: passageText.value.slice(0, 4000),
      note: '',
    })
    status.value = 'Saved to bookmarks.'
  } catch (err) {
    error.value = err.message
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  onDaily()
})
</script>

<template>
  <section class="panel">
    <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
      <div>
        <h1 class="h4 mb-1">Scripture reader</h1>
        <p class="muted mb-0">ESV text is proxied by the ASP.NET API so your key stays server-side.</p>
      </div>
      <button class="btn btn-outline-secondary btn-sm" type="button" @click="onDaily">
        Daily verse
      </button>
    </div>

    <form class="row g-2 align-items-end mb-3" @submit.prevent="onSearch">
      <div class="col-md-5">
        <label class="form-label">Book</label>
        <select v-model="book" class="form-select">
          <option v-for="name in books" :key="name" :value="name">{{ name }}</option>
        </select>
      </div>
      <div class="col-md-3">
        <label class="form-label">Chapter</label>
        <select v-model.number="chapter" class="form-select">
          <option v-for="n in chapters" :key="n" :value="n">{{ n }}</option>
        </select>
      </div>
      <div class="col-md-4 d-flex gap-2">
        <button class="btn btn-primary flex-fill" type="submit">Read</button>
        <button
          class="btn btn-outline-primary"
          type="button"
          :disabled="saving || !passageText"
          @click="onBookmark"
        >
          Bookmark
        </button>
      </div>
    </form>

    <p v-if="status" class="text-secondary mb-2">{{ status }}</p>
    <div v-if="error" class="alert alert-warning py-2">{{ error }}</div>

    <article v-if="passageText">
      <h2 class="h5">{{ activeReference }}</h2>
      <div class="passage-text">{{ passageText }}</div>
    </article>
  </section>
</template>
