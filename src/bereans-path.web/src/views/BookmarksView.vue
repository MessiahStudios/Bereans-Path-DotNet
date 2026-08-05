<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { deleteBookmark, listBookmarks, updateBookmark } from '../api'

const bookmarks = ref([])
const error = ref('')
const loading = ref(true)
const editingId = ref(null)
const draftNote = ref('')
const savingId = ref(null)
const expandedPassages = ref(new Set())
const expandedMemoirs = ref(new Set())

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

/** Whole chapters / long text stay collapsed so the note stays front and center. */
function isLongPassage(item) {
  const text = item.passageText || ''
  const looksLikeChapterOnly = /^[\w\s]+\s+\d+$/i.test((item.reference || '').trim())
  return looksLikeChapterOnly || text.length > 420
}

function isPassageOpen(id) {
  return expandedPassages.value.has(id)
}

function togglePassage(id) {
  const next = new Set(expandedPassages.value)
  if (next.has(id)) next.delete(id)
  else next.add(id)
  expandedPassages.value = next
}

function isMemoirOpen(id) {
  return expandedMemoirs.value.has(id)
}

function toggleMemoir(id) {
  const next = new Set(expandedMemoirs.value)
  if (next.has(id)) next.delete(id)
  else next.add(id)
  expandedMemoirs.value = next
}

function startEdit(item) {
  editingId.value = item.id
  draftNote.value = item.note || ''
}

function cancelEdit() {
  editingId.value = null
  draftNote.value = ''
}

async function saveNote(id) {
  savingId.value = id
  error.value = ''
  try {
    const updated = await updateBookmark(id, { note: draftNote.value })
    bookmarks.value = bookmarks.value.map((b) => (b.id === id ? updated : b))
    cancelEdit()
  } catch (err) {
    error.value = err.message
  } finally {
    savingId.value = null
  }
}

async function remove(id) {
  error.value = ''
  try {
    await deleteBookmark(id)
    bookmarks.value = bookmarks.value.filter((b) => b.id !== id)
    if (editingId.value === id) cancelEdit()
  } catch (err) {
    error.value = err.message
  }
}

const sortedBookmarks = computed(() => bookmarks.value)

onMounted(refresh)
</script>

<template>
  <section class="panel">
    <div class="d-flex justify-content-between align-items-center mb-3 gap-2">
      <div>
        <h2 class="h4 mb-1">Bookmarks</h2>
        <p class="muted mb-0">Notes first — long passages stay tucked away. Updated notes keep an archived memoir.</p>
      </div>
      <button class="btn btn-outline-secondary btn-sm" type="button" @click="refresh">Refresh</button>
    </div>

    <div v-if="error" class="alert alert-warning py-2">{{ error }}</div>
    <p v-if="loading" class="text-secondary">Loading…</p>

    <div v-else-if="!sortedBookmarks.length" class="empty-state">
      <p class="mb-2">No bookmarks yet.</p>
      <RouterLink class="btn btn-primary btn-sm" to="/">Open Reader</RouterLink>
    </div>

    <div v-else class="d-grid gap-3">
      <article v-for="item in sortedBookmarks" :key="item.id" class="card-item bookmark-card">
        <div class="d-flex justify-content-between gap-3">
          <div class="min-w-0 flex-grow-1">
            <div class="bookmark-header mb-2">
              <h3 class="h6 mb-0">{{ item.reference }}</h3>
              <p class="small muted mb-0">{{ new Date(item.createdAt).toLocaleString() }}</p>
            </div>

            <!-- Notes are the primary surface -->
            <div class="note-panel mb-3">
              <div class="note-label">Current note</div>

              <div v-if="editingId === item.id" class="note-editor">
                <textarea
                  :id="`note-${item.id}`"
                  v-model="draftNote"
                  class="form-control mb-2"
                  rows="4"
                  maxlength="2000"
                  placeholder="A newer perspective, prayer, or reflection…"
                />
                <p class="small muted mb-2">
                  Saving a different note archives the previous one as a memoir.
                </p>
                <div class="d-flex gap-2">
                  <button
                    class="btn btn-sm btn-primary"
                    type="button"
                    :disabled="savingId === item.id"
                    @click="saveNote(item.id)"
                  >
                    {{ savingId === item.id ? 'Saving…' : 'Save note' }}
                  </button>
                  <button class="btn btn-sm btn-outline-secondary" type="button" @click="cancelEdit">
                    Cancel
                  </button>
                </div>
              </div>

              <div v-else>
                <p v-if="item.note" class="note-body mb-2">{{ item.note }}</p>
                <p v-else class="muted mb-2">No note yet — add one to capture why this passage matters.</p>
                <button class="btn btn-sm btn-outline-primary" type="button" @click="startEdit(item)">
                  {{ item.note ? 'Update note' : 'Add note' }}
                </button>
              </div>
            </div>

            <!-- Memoir archive -->
            <div v-if="item.memoirs?.length" class="memoir-block mb-3">
              <button
                class="memoir-toggle"
                type="button"
                @click="toggleMemoir(item.id)"
              >
                {{ isMemoirOpen(item.id) ? 'Hide' : 'Show' }} archived memoir
                ({{ item.memoirs.length }})
              </button>
              <div v-if="isMemoirOpen(item.id)" class="memoir-list">
                <article
                  v-for="memoir in item.memoirs"
                  :key="memoir.id"
                  class="memoir-item"
                >
                  <p class="small muted mb-1">
                    Archived {{ new Date(memoir.archivedAt).toLocaleString() }}
                  </p>
                  <p class="mb-0">{{ memoir.noteText }}</p>
                </article>
              </div>
            </div>

            <!-- Collapsed passage for chapters / long text -->
            <div class="passage-block">
              <button
                v-if="isLongPassage(item)"
                class="passage-toggle"
                type="button"
                @click="togglePassage(item.id)"
              >
                {{ isPassageOpen(item.id) ? 'Hide passage' : 'Show passage' }}
              </button>
              <div
                v-if="!isLongPassage(item) || isPassageOpen(item.id)"
                class="passage-text mt-2"
              >
                {{ item.passageText }}
              </div>
              <p
                v-else
                class="small muted mt-2 mb-0"
              >
                Passage tucked away so your note stays in focus.
              </p>
            </div>
          </div>

          <button class="btn btn-sm btn-outline-danger align-self-start shrink-0" type="button" @click="remove(item.id)">
            Delete
          </button>
        </div>
      </article>
    </div>
  </section>
</template>
