<script setup>
import { computed, ref, watch } from 'vue'
import { createBookmark, fetchPassage } from '../api'
import { BIBLE_BOOKS, dailyVerseReference } from '../bible'
import MissionStrip from '../components/MissionStrip.vue'
import { ESV_COPYRIGHT_NOTICE, ESV_PAGE_NOTE, ESV_SITE_URL } from '../data/esvCopy'

const books = Object.keys(BIBLE_BOOKS)
const book = ref('Genesis')
const chapter = ref(1)
const verseFrom = ref('')
const verseTo = ref('')
const status = ref('')
const error = ref('')
const passageText = ref('')
const loadedBook = ref('')
const loadedChapter = ref(null)
const loadedReference = ref('')
const loadedVerseFrom = ref(null)
const loadedVerseTo = ref(null)
const note = ref('')
const saving = ref(false)
const loading = ref(false)

const chapters = computed(() => {
  const count = BIBLE_BOOKS[book.value] ?? 1
  return Array.from({ length: count }, (_, i) => i + 1)
})

const selectionKey = computed(() =>
  [book.value, chapter.value, normalizeVerse(verseFrom.value), normalizeVerse(verseTo.value)].join('|'),
)

const loadedSelectionKey = computed(() =>
  [loadedBook.value, loadedChapter.value, loadedVerseFrom.value, loadedVerseTo.value].join('|'),
)

/** Passage is ready only after a successful Read for the current selection. */
const isReady = computed(
  () => Boolean(passageText.value) && selectionKey.value === loadedSelectionKey.value,
)

const selectionChanged = computed(
  () => Boolean(loadedReference.value) && selectionKey.value !== loadedSelectionKey.value,
)

const hasVerseSelection = computed(() => loadedVerseFrom.value != null)

const esvAudioSrc = computed(() => {
  if (!isReady.value || !loadedBook.value || !loadedChapter.value) return ''
  const formattedBook = loadedBook.value.replace(/\s+/g, '+')
  // ESV.org audio player is chapter-scoped.
  return `https://www.esv.org/audio-player/${formattedBook}+${loadedChapter.value}/`
})

watch(book, () => {
  const max = BIBLE_BOOKS[book.value] ?? 1
  if (chapter.value > max) chapter.value = 1
})

function normalizeVerse(value) {
  if (value === '' || value == null) return null
  const n = Number(value)
  if (!Number.isInteger(n) || n < 1) return null
  return n
}

function buildReference(bookName, chapterNum, fromRaw, toRaw) {
  const from = normalizeVerse(fromRaw)
  const to = normalizeVerse(toRaw)
  if (from == null) return `${bookName} ${chapterNum}`
  if (to != null && to !== from) {
    const start = Math.min(from, to)
    const end = Math.max(from, to)
    return `${bookName} ${chapterNum}:${start}-${end}`
  }
  return `${bookName} ${chapterNum}:${from}`
}

function clearLoaded() {
  passageText.value = ''
  loadedBook.value = ''
  loadedChapter.value = null
  loadedReference.value = ''
  loadedVerseFrom.value = null
  loadedVerseTo.value = null
  note.value = ''
}

async function loadPassage(reference, { bookName, chapterNum, fromVerse, toVerse } = {}) {
  error.value = ''
  status.value = 'Loading passage…'
  loading.value = true
  note.value = ''
  try {
    const data = await fetchPassage(reference)
    const text = Array.isArray(data?.passages) ? data.passages.join('\n\n') : ''
    if (!text) {
      throw new Error('No passage text returned. Check ESV_API_KEY on the API.')
    }

    passageText.value = text
    loadedReference.value = reference
    loadedBook.value = bookName ?? book.value
    loadedChapter.value = chapterNum ?? chapter.value
    loadedVerseFrom.value = fromVerse ?? null
    loadedVerseTo.value = toVerse ?? null
    book.value = loadedBook.value
    chapter.value = loadedChapter.value
    verseFrom.value = fromVerse != null ? String(fromVerse) : ''
    verseTo.value = toVerse != null ? String(toVerse) : ''
    status.value = ''
  } catch (err) {
    clearLoaded()
    error.value = err.message
    status.value = ''
  } finally {
    loading.value = false
  }
}

async function onRead() {
  let from = normalizeVerse(verseFrom.value)
  let to = normalizeVerse(verseTo.value)
  if (from == null) {
    to = null
  } else if (to != null && to < from) {
    ;[from, to] = [to, from]
  }

  verseFrom.value = from != null ? String(from) : ''
  verseTo.value = to != null ? String(to) : ''

  const reference = buildReference(book.value, chapter.value, verseFrom.value, verseTo.value)
  await loadPassage(reference, {
    bookName: book.value,
    chapterNum: chapter.value,
    fromVerse: from,
    toVerse: to,
  })
}

async function onDaily() {
  const reference = dailyVerseReference()
  const bookName = books.find((name) => reference.startsWith(name)) || 'John'
  const chapterMatch = reference.slice(bookName.length).trim().match(/^(\d+)/)
  const chapterNum = chapterMatch ? Number(chapterMatch[1]) : 1
  book.value = bookName
  chapter.value = chapterNum
  verseFrom.value = ''
  verseTo.value = ''
  await loadPassage(`${bookName} ${chapterNum}`, {
    bookName,
    chapterNum,
    fromVerse: null,
    toVerse: null,
  })
}

async function onBookmark() {
  if (!isReady.value) return
  saving.value = true
  error.value = ''
  try {
    await createBookmark({
      reference: loadedReference.value,
      passageText: passageText.value.slice(0, 4000),
      note: note.value.trim(),
    })
    status.value = `Saved ${loadedReference.value} to bookmarks.`
    note.value = ''
  } catch (err) {
    error.value = err.message
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="d-grid gap-3">
    <MissionStrip />

    <section class="panel">
      <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
        <div>
          <h2 class="h4 mb-1">Scripture reader</h2>
          <p class="muted mb-0">
            Choose a book and chapter. Leave verses blank to read the whole chapter, or set a verse / range (e.g. 16 or 16–18).
          </p>
        </div>
        <button class="btn btn-outline-secondary btn-sm" type="button" :disabled="loading" @click="onDaily">
          Daily chapter
        </button>
      </div>

      <form class="row g-2 align-items-end mb-3" @submit.prevent="onRead">
        <div class="col-sm-6 col-md-4">
          <label class="form-label" for="reader-book">Book</label>
          <select id="reader-book" v-model="book" class="form-select" :disabled="loading">
            <option v-for="name in books" :key="name" :value="name">{{ name }}</option>
          </select>
        </div>
        <div class="col-4 col-md-2">
          <label class="form-label" for="reader-chapter">Chapter</label>
          <select id="reader-chapter" v-model.number="chapter" class="form-select" :disabled="loading">
            <option v-for="n in chapters" :key="n" :value="n">{{ n }}</option>
          </select>
        </div>
        <div class="col-4 col-md-2">
          <label class="form-label" for="reader-verse-from">Verse</label>
          <input
            id="reader-verse-from"
            v-model="verseFrom"
            class="form-control"
            type="number"
            min="1"
            inputmode="numeric"
            placeholder="All"
            :disabled="loading"
          />
        </div>
        <div class="col-4 col-md-2">
          <label class="form-label" for="reader-verse-to">To</label>
          <input
            id="reader-verse-to"
            v-model="verseTo"
            class="form-control"
            type="number"
            min="1"
            inputmode="numeric"
            placeholder="—"
            :disabled="loading || !verseFrom"
          />
        </div>
        <div class="col-12 col-md-2 reader-actions">
          <button class="btn btn-primary w-100" type="submit" :disabled="loading">
            {{ loading ? 'Reading…' : 'Read' }}
          </button>
        </div>
      </form>

      <p class="small muted mb-3">
        Tip: <strong>John 3</strong> = whole chapter · <strong>verse 16</strong> = John 3:16 · <strong>16 to 18</strong> = John 3:16–18.
      </p>

      <p v-if="selectionChanged" class="reader-hint mb-3">
        Selection changed. Tap <strong>Read</strong> to open
        {{ buildReference(book, chapter, verseFrom, verseTo) }}.
      </p>

      <div v-if="!loadedReference && !loading && !error" class="reader-empty mb-0">
        Pick a book and chapter (and optional verses), then tap <strong>Read</strong> to listen, take notes, and bookmark.
      </div>

      <p v-if="status" class="text-secondary mb-2">{{ status }}</p>
      <div v-if="error" class="alert alert-warning py-2 mb-0">{{ error }}</div>

      <article v-if="isReady" class="reader-ready">
        <h3 class="h5 mb-3">{{ loadedReference }}</h3>

        <div class="esv-audio mb-3">
          <p class="small muted mb-2">
            Listen · {{ loadedBook }} {{ loadedChapter }} (ESV audio)
            <span v-if="hasVerseSelection"> — audio is the full chapter; text below matches your verse selection.</span>
          </p>
          <iframe
            :key="esvAudioSrc"
            :src="esvAudioSrc"
            :title="`ESV audio for ${loadedBook} ${loadedChapter}`"
            class="esv-audio-frame"
            loading="lazy"
          />
        </div>

        <div class="passage-text mb-3">{{ passageText }}</div>

        <p class="esv-attribution mb-3">
          {{ ESV_PAGE_NOTE }}
          <a :href="ESV_SITE_URL" target="_blank" rel="noopener noreferrer">www.esv.org</a>
        </p>

        <div class="reader-capture">
          <label class="form-label" for="reader-note">Note for this passage</label>
          <input
            id="reader-note"
            v-model="note"
            class="form-control mb-2"
            type="text"
            maxlength="500"
            placeholder="Why this passage matters to you…"
          />
          <button
            class="btn btn-outline-primary"
            type="button"
            :disabled="saving"
            @click="onBookmark"
          >
            {{ saving ? 'Saving…' : 'Bookmark passage' }}
          </button>
        </div>
      </article>

      <footer class="esv-copyright mt-3">
        <p class="small muted mb-0">{{ ESV_COPYRIGHT_NOTICE }}</p>
      </footer>
    </section>
  </div>
</template>

<style scoped>
.reader-empty,
.reader-hint {
  padding: 0.9rem 1rem;
  border-radius: 0.75rem;
  border: 1px dashed var(--bp-border);
  color: var(--bp-muted);
  line-height: 1.45;
}

.reader-hint {
  border-style: solid;
  border-color: color-mix(in srgb, var(--bp-accent) 45%, var(--bp-border));
  color: var(--bp-ink);
}

.reader-ready {
  padding-top: 0.25rem;
  border-top: 1px solid var(--bp-border);
}

.reader-capture {
  padding-top: 1rem;
  border-top: 1px solid var(--bp-border);
}

.esv-audio-frame {
  width: 100%;
  height: 110px;
  border: 1px solid var(--bp-border);
  border-radius: 0.75rem;
  background: var(--bp-elevated);
}

.esv-attribution {
  font-size: 0.9rem;
  color: var(--bp-muted);
}

.esv-attribution a {
  color: var(--bp-accent);
  font-weight: 600;
}

.esv-copyright {
  padding-top: 0.85rem;
  border-top: 1px solid var(--bp-border);
  line-height: 1.45;
}
</style>
