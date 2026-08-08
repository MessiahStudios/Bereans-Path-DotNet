<script setup>
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import L from 'leaflet'
import markerIcon2x from 'leaflet/dist/images/marker-icon-2x.png'
import markerIcon from 'leaflet/dist/images/marker-icon.png'
import markerShadow from 'leaflet/dist/images/marker-shadow.png'
import { deleteSavedChurch, findNearbyChurches, listSavedChurches, saveChurch, suggestChurch } from '../api'
import { CHURCH_FIT_BLURB, CHURCH_FIT_BLURB_DETAIL, CHURCH_DATA_DISCLAIMER, isGenericChurchName } from '../data/churchFit'
import { buildChurchDetails } from '../data/churchDetails'
import { NEW_BELIEVER_AUDIO } from '../data/resources'
import MissionStrip from '../components/MissionStrip.vue'

delete L.Icon.Default.prototype._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: markerIcon2x,
  iconUrl: markerIcon,
  shadowUrl: markerShadow,
})

/** Shorter radius = less map area for Overpass = usually faster results. */
const RADIUS_OPTIONS = [
  { miles: 5, meters: 8000, zoom: 12, label: '5 mi', hint: 'Usually fastest' },
  { miles: 10, meters: 16000, zoom: 11, label: '10 mi', hint: 'Good balance' },
  { miles: 15, meters: 24000, zoom: 10, label: '15 mi', hint: '' },
  { miles: 20, meters: 32000, zoom: 10, label: '20 mi', hint: 'Wider, often slower' },
]

const mapEl = ref(null)
const status = ref('')
const error = ref('')
const churches = ref([])
const saved = ref([])
const searching = ref(false)
const mapReady = ref(false)
const selected = ref(null)
const showSuggest = ref(false)
const showFitDetail = ref(false)
const suggestStatus = ref('')
const suggestError = ref('')
const suggesting = ref(false)
const radiusMeters = ref(16000)
const lastSearch = reactive({ lat: null, lon: null, miles: null })

const waitAudio = NEW_BELIEVER_AUDIO[0]
const waitAudioHost = ref(null)
const showWaitAudio = ref(false)
/** Imperative player — kept out of Vue's patch cycle so re-renders can't reset it. */
let waitPlayer = null
let waitPlayerBlobUrl = null
let userPausedWaitAudio = false

const selectedRadius = computed(
  () => RADIUS_OPTIONS.find((o) => o.meters === radiusMeters.value) ?? RADIUS_OPTIONS[1],
)

const suggestion = reactive({
  name: '',
  city: '',
  website: '',
  denomination: '',
  reason: '',
  contactEmail: '',
})

const details = computed(() => buildChurchDetails(selected.value))

let map
let layerGroup
let markerByOsmId = new Map()

async function refreshSaved() {
  saved.value = await listSavedChurches()
}

function ensureMap(lat, lon, zoom = 12) {
  if (!map) {
    map = L.map(mapEl.value).setView([lat, lon], zoom)
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
    }).addTo(map)
    layerGroup = L.layerGroup().addTo(map)
    mapReady.value = true
  } else {
    map.setView([lat, lon], zoom)
  }
  setTimeout(() => map?.invalidateSize(), 50)
}

function selectChurch(church) {
  selected.value = church
  if (!map || !church) return
  map.setView([church.latitude, church.longitude], 15)
  const marker = markerByOsmId.get(church.osmId)
  marker?.openPopup()
}

function setRadius(meters) {
  if (searching.value) return
  radiusMeters.value = meters
}

function mountWaitPlayer() {
  const host = waitAudioHost.value
  if (!host || !waitPlayer) return
  if (waitPlayer.parentElement !== host) {
    host.replaceChildren(waitPlayer)
  }
}

async function ensureWaitPlayer() {
  if (waitPlayer) {
    mountWaitPlayer()
    return waitPlayer
  }

  // Fetch once into a blob so Overpass network traffic can't stall mid-clip.
  const response = await fetch(waitAudio.audioUrl)
  if (!response.ok) throw new Error('Unable to load wait audio.')
  const blob = await response.blob()
  waitPlayerBlobUrl = URL.createObjectURL(blob)

  waitPlayer = new Audio(waitPlayerBlobUrl)
  waitPlayer.controls = true
  waitPlayer.preload = 'auto'
  waitPlayer.playsInline = true
  waitPlayer.className = 'wait-audio-player'

  waitPlayer.addEventListener('play', () => {
    userPausedWaitAudio = false
  })

  waitPlayer.addEventListener('pointerdown', () => {
    // Treat control clicks as intentional pause/play so we don't fight the user.
    userPausedWaitAudio = true
  })

  const kickIfStalled = () => {
    if (!searching.value || userPausedWaitAudio || !waitPlayer) return
    if (waitPlayer.paused && !waitPlayer.ended) {
      waitPlayer.play().catch(() => {})
    }
  }
  waitPlayer.addEventListener('stalled', kickIfStalled)
  waitPlayer.addEventListener('waiting', kickIfStalled)

  mountWaitPlayer()
  return waitPlayer
}

async function startWaitAudio() {
  showWaitAudio.value = true
  userPausedWaitAudio = false
  await nextTick()
  try {
    const el = await ensureWaitPlayer()
    if (el.ended) el.currentTime = 0
    if (el.paused) await el.play()
  } catch {
    // Controls still work if autoplay is blocked.
  }
}

function pauseWaitAudio({ fromUser = false } = {}) {
  if (fromUser) userPausedWaitAudio = true
  if (!waitPlayer || waitPlayer.paused) return
  waitPlayer.pause()
}

async function locateAndSearch() {
  error.value = ''
  status.value = 'Getting your location…'
  searching.value = true
  selected.value = null
  // Same user-gesture as the button click — resume or start without seeking to 0.
  void startWaitAudio()

  if (!navigator.geolocation) {
    error.value = 'Geolocation is not supported in this browser.'
    status.value = ''
    searching.value = false
    return
  }

  navigator.geolocation.getCurrentPosition(async (position) => {
    const { latitude, longitude } = position.coords
    await runSearchAt(latitude, longitude)
  }, (geoError) => {
    const denied = geoError.code === geoError.PERMISSION_DENIED
    error.value = denied
      ? 'Location permission is blocked. Allow location for this site, then try again.'
      : (geoError.message || 'Unable to get location.')
    status.value = ''
    searching.value = false
  }, {
    enableHighAccuracy: false,
    timeout: 15000,
  })
}

async function runSearchAt(latitude, longitude) {
  const option = selectedRadius.value
  await nextTick()
  ensureMap(latitude, longitude, option.zoom)
  layerGroup.clearLayers()
  markerByOsmId = new Map()
  L.marker([latitude, longitude]).addTo(layerGroup).bindPopup('You are here').openPopup()

  status.value = `Searching within about ${option.miles} miles… Usually under 20 seconds when map servers are healthy.`
  searching.value = true
  error.value = ''
  selected.value = null

  try {
    churches.value = await findNearbyChurches(latitude, longitude, option.meters)
    lastSearch.lat = latitude
    lastSearch.lon = longitude
    lastSearch.miles = option.miles
    // Defer markers so a big result set doesn't hitch the audio thread.
    const found = churches.value
    requestAnimationFrame(() => {
      found.forEach((church) => {
        const marker = L.marker([church.latitude, church.longitude])
          .addTo(layerGroup)
          .bindPopup(church.name)
        marker.on('click', () => selectChurch(church))
        markerByOsmId.set(church.osmId, marker)
      })
    })
    status.value = found.length
      ? `Found ${found.length} churches within about ${option.miles} miles — nearest first.`
      : `No matching churches within about ${option.miles} miles. Try a wider radius.`
  } catch (err) {
    error.value = err.message
    status.value = ''
  } finally {
    searching.value = false
  }
}

async function searchAgainWithRadius(meters) {
  radiusMeters.value = meters
  if (lastSearch.lat != null && lastSearch.lon != null) {
    searching.value = true
    void startWaitAudio()
    await runSearchAt(lastSearch.lat, lastSearch.lon)
    return
  }
  await locateAndSearch()
}

function searchFarther() {
  const idx = RADIUS_OPTIONS.findIndex((o) => o.meters === radiusMeters.value)
  const next = RADIUS_OPTIONS[Math.min(idx + 1, RADIUS_OPTIONS.length - 1)]
  if (!next || next.meters === radiusMeters.value) return
  return searchAgainWithRadius(next.meters)
}

const canSearchFarther = computed(
  () => !searching.value && lastSearch.miles != null && radiusMeters.value < 32000,
)

function formatDistance(km) {
  if (km == null || Number.isNaN(km)) return ''
  const miles = km * 0.621371
  if (miles < 10) return `${miles.toFixed(1)} mi`
  return `${Math.round(miles)} mi`
}

async function onSave(church) {
  error.value = ''
  try {
    await saveChurch({
      name: church.name,
      latitude: church.latitude,
      longitude: church.longitude,
      osmId: church.osmId,
    })
    await refreshSaved()
    status.value = `Saved ${church.name}.`
  } catch (err) {
    error.value = err.message
  }
}

async function onRemoveSaved(id) {
  await deleteSavedChurch(id)
  await refreshSaved()
}

function resetSuggestion() {
  suggestion.name = ''
  suggestion.city = ''
  suggestion.website = ''
  suggestion.denomination = ''
  suggestion.reason = ''
  suggestion.contactEmail = ''
}

async function onSuggestSubmit() {
  suggestError.value = ''
  suggestStatus.value = ''
  suggesting.value = true
  try {
    await suggestChurch({
      name: suggestion.name,
      city: suggestion.city || null,
      website: suggestion.website || null,
      denomination: suggestion.denomination || null,
      reason: suggestion.reason,
      contactEmail: suggestion.contactEmail || null,
    })
    suggestStatus.value = 'Thank you — your church suggestion was received.'
    resetSuggestion()
    showSuggest.value = false
  } catch (err) {
    suggestError.value = err.message
  } finally {
    suggesting.value = false
  }
}

onMounted(async () => {
  try {
    await refreshSaved()
  } catch (err) {
    error.value = err.message
  }
  // Warm the blob in the background so Find near me can play from local memory.
  try {
    showWaitAudio.value = false
    await ensureWaitPlayer()
  } catch {
    // Non-fatal — first play will retry the fetch.
  }
})

onBeforeUnmount(() => {
  pauseWaitAudio({ fromUser: true })
  if (waitPlayer) {
    waitPlayer.removeAttribute('src')
    waitPlayer.load()
    waitPlayer = null
  }
  if (waitPlayerBlobUrl) {
    URL.revokeObjectURL(waitPlayerBlobUrl)
    waitPlayerBlobUrl = null
  }
  if (map) {
    map.remove()
    map = null
  }
})
</script>

<template>
  <div class="d-grid gap-3">
    <MissionStrip
      aside="Church search uses OpenStreetMap. Pick a shorter radius for a quicker first look."
    />

    <section class="panel">
      <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
        <div>
          <h2 class="h4 mb-1">Church finder</h2>
          <p class="muted mb-1">Find Protestant and non-denominational churches near you.</p>
          <p class="church-fit-blurb mb-1">{{ CHURCH_FIT_BLURB }}</p>
          <button
            class="btn btn-link btn-sm church-fit-toggle px-0"
            type="button"
            @click="showFitDetail = !showFitDetail"
          >
            {{ showFitDetail ? 'Hide who we set aside' : 'Who we set aside' }}
          </button>
          <p v-if="showFitDetail" class="small muted mb-0">{{ CHURCH_FIT_BLURB_DETAIL }}</p>
          <p class="small muted church-data-note mb-0 mt-2">{{ CHURCH_DATA_DISCLAIMER }}</p>
        </div>
        <div class="d-flex flex-wrap gap-2">
          <button class="btn btn-outline-primary" type="button" @click="showSuggest = !showSuggest">
            {{ showSuggest ? 'Close suggestion' : 'Suggest a church' }}
          </button>
          <button class="btn btn-primary" type="button" :disabled="searching" @click="locateAndSearch">
            {{ searching ? 'Searching…' : 'Find near me' }}
          </button>
        </div>
      </div>

      <div class="radius-row mb-3">
        <span class="radius-label">Search radius</span>
        <div class="radius-options" role="group" aria-label="Search radius">
          <button
            v-for="opt in RADIUS_OPTIONS"
            :key="opt.meters"
            type="button"
            class="btn btn-sm"
            :class="radiusMeters === opt.meters ? 'btn-primary' : 'btn-outline-secondary'"
            :disabled="searching"
            :title="opt.hint || undefined"
            @click="setRadius(opt.meters)"
          >
            {{ opt.label }}
          </button>
        </div>
        <p class="small muted mb-0 radius-hint">
          {{ selectedRadius.hint || 'Shorter distance usually loads faster.' }}
          Map servers can still take a moment when busy.
        </p>
      </div>

      <p v-if="status && !searching" class="text-secondary">{{ status }}</p>
      <p v-if="suggestStatus" class="text-success">{{ suggestStatus }}</p>
      <div v-if="error" class="alert alert-warning py-2">{{ error }}</div>

      <div v-show="showWaitAudio" class="search-wait mb-3">
        <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-2">
          <p class="mb-0">
            <template v-if="searching">
              <strong>Searching within about {{ selectedRadius.miles }} miles…</strong>
              Playing <em>{{ waitAudio.title }}</em> while the list fills in.
            </template>
            <template v-else>
              <strong>{{ waitAudio.title }}</strong>
              <span class="d-block small muted">{{ waitAudio.description }} Pause and resume anytime — progress is kept.</span>
            </template>
          </p>
          <button
            v-if="!searching"
            class="btn btn-sm btn-outline-secondary"
            type="button"
            @click="showWaitAudio = false; pauseWaitAudio({ fromUser: true })"
          >
            Hide
          </button>
        </div>
        <div ref="waitAudioHost" class="wait-audio"></div>
      </div>

      <div v-if="showSuggest" class="suggest-panel mb-3">
        <h3 class="h6 mb-1">Suggest a church</h3>
        <p class="small muted mb-3">
          Know a Protestant or non-denominational church that treats Scripture as the final authority?
          Send it our way — especially if OpenStreetMap is missing it.
        </p>
        <form class="row g-2" @submit.prevent="onSuggestSubmit">
          <div class="col-md-6">
            <label class="form-label" for="suggest-name">Church name</label>
            <input id="suggest-name" v-model="suggestion.name" class="form-control" required maxlength="300" />
          </div>
          <div class="col-md-6">
            <label class="form-label" for="suggest-city">City / area</label>
            <input id="suggest-city" v-model="suggestion.city" class="form-control" maxlength="200" placeholder="Phoenix, AZ" />
          </div>
          <div class="col-md-6">
            <label class="form-label" for="suggest-website">Website</label>
            <input id="suggest-website" v-model="suggestion.website" class="form-control" type="url" maxlength="500" placeholder="https://" />
          </div>
          <div class="col-md-6">
            <label class="form-label" for="suggest-denom">Denomination</label>
            <input id="suggest-denom" v-model="suggestion.denomination" class="form-control" maxlength="120" placeholder="Non-denominational" />
          </div>
          <div class="col-12">
            <label class="form-label" for="suggest-reason">Why it fits Bereans Path</label>
            <textarea
              id="suggest-reason"
              v-model="suggestion.reason"
              class="form-control"
              rows="3"
              required
              maxlength="2000"
              placeholder="Expository preaching, high view of Scripture, seminary-trained pastors…"
            />
          </div>
          <div class="col-md-6">
            <label class="form-label" for="suggest-email">Your email (optional)</label>
            <input id="suggest-email" v-model="suggestion.contactEmail" class="form-control" type="email" maxlength="200" />
          </div>
          <div class="col-12">
            <div v-if="suggestError" class="alert alert-warning py-2">{{ suggestError }}</div>
            <button class="btn btn-primary" type="submit" :disabled="suggesting">
              {{ suggesting ? 'Sending…' : 'Submit suggestion' }}
            </button>
          </div>
        </form>
      </div>

      <div class="map-wrap mb-3">
        <div ref="mapEl" class="map-frame"></div>
        <div v-if="!mapReady" class="map-placeholder">
          Choose a radius, then tap <strong>Find near me</strong>.
        </div>
      </div>

      <div v-if="details" class="church-detail mb-3">
        <div class="d-flex justify-content-between align-items-start gap-2 mb-2">
          <div>
            <h3 class="h5 mb-1">
              {{ details.name }}
              <span v-if="isGenericChurchName(details.name)" class="generic-name-badge">Name incomplete</span>
            </h3>
            <p v-if="isGenericChurchName(details.name)" class="small muted mb-1">
              OpenStreetMap has no public name for this place — only a generic “Church” label. Check the pin, website, or
              <button class="btn btn-link btn-sm p-0 align-baseline" type="button" @click="showSuggest = true">Suggest a church</button>
              with the correct name.
            </p>
            <p class="small muted mb-0">
              <template v-if="details.distanceKm != null">{{ formatDistance(details.distanceKm) }}</template>
              <template v-if="details.distanceKm != null && details.denomination"> · </template>
              <span v-if="details.denomination" class="text-capitalize">{{ details.denomination.replaceAll('_', ' ') }}</span>
            </p>
          </div>
          <button class="btn btn-sm btn-outline-secondary" type="button" @click="selected = null">Close</button>
        </div>

        <p v-if="details.address" class="mb-2">{{ details.address }}</p>
        <p v-if="details.phone" class="mb-2">
          <a :href="`tel:${details.phone}`">{{ details.phone }}</a>
        </p>

        <div class="detail-actions mb-3">
          <a
            v-if="details.website"
            class="btn btn-primary"
            :href="details.website"
            target="_blank"
            rel="noopener noreferrer"
          >
            Website
          </a>
          <a
            class="btn btn-outline-primary"
            :href="details.directions.google"
            target="_blank"
            rel="noopener noreferrer"
          >
            Directions
          </a>
          <a
            class="btn btn-outline-secondary"
            :href="details.directions.apple"
            target="_blank"
            rel="noopener noreferrer"
          >
            Apple Maps
          </a>
          <button class="btn btn-outline-primary" type="button" @click="onSave(selected)">Save</button>
        </div>

        <div class="seminary-block">
          <h4 class="h6 mb-1">{{ details.seminaryGuidance.title }}</h4>
          <p v-if="details.seminaryNote" class="mb-2">{{ details.seminaryNote }}</p>
          <p class="mb-2">{{ details.seminaryGuidance.body }}</p>
          <div class="d-flex flex-wrap gap-2">
            <a
              v-for="link in details.seminaryGuidance.links"
              :key="link.href"
              class="btn btn-sm btn-outline-secondary"
              :href="link.href"
              target="_blank"
              rel="noopener noreferrer"
            >
              {{ link.name }}
            </a>
          </div>
        </div>
      </div>

      <div class="row g-3">
        <div class="col-lg-7">
          <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
            <h3 class="h6 mb-0">Nearby results</h3>
            <button
              v-if="canSearchFarther"
              class="btn btn-sm btn-outline-secondary"
              type="button"
              @click="searchFarther"
            >
              Search farther
            </button>
          </div>
          <div v-if="!churches.length" class="muted">Run a search to see churches here.</div>
          <ul v-else class="item-list church-results">
            <li
              v-for="church in churches"
              :key="church.osmId"
              class="item-list-row"
              :class="{ active: selected?.osmId === church.osmId }"
            >
              <button class="item-list-main" type="button" @click="selectChurch(church)">
                <span class="d-block">
                  {{ church.name }}
                  <span v-if="isGenericChurchName(church.name)" class="generic-name-badge">Name incomplete</span>
                </span>
                <span class="small muted">
                  <template v-if="church.distanceKm != null">{{ formatDistance(church.distanceKm) }}</template>
                  <template v-if="church.distanceKm != null && church.denomination"> · </template>
                  <span v-if="church.denomination" class="text-capitalize">{{ church.denomination.replaceAll('_', ' ') }}</span>
                </span>
              </button>
              <button class="btn btn-sm btn-outline-primary" type="button" @click="onSave(church)">
                Save
              </button>
            </li>
          </ul>
        </div>
        <div class="col-lg-5">
          <h3 class="h6">Saved churches</h3>
          <div v-if="!saved.length" class="muted">None saved yet.</div>
          <ul v-else class="item-list">
            <li v-for="church in saved" :key="church.id" class="item-list-row">
              <span>{{ church.name }}</span>
              <button class="btn btn-sm btn-outline-danger" type="button" @click="onRemoveSaved(church.id)">
                Remove
              </button>
            </li>
          </ul>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.church-fit-blurb {
  max-width: 42rem;
  color: var(--bp-ink-muted, var(--bp-muted, #8a8178));
  font-size: 0.95rem;
  line-height: 1.55;
  border-left: 2px solid var(--bp-gold, #c4a574);
  padding-left: 0.75rem;
}

.church-fit-toggle {
  color: var(--bp-accent, var(--bp-gold, #c4a574));
  text-decoration: none;
}

.church-data-note {
  max-width: 42rem;
  line-height: 1.45;
}

.generic-name-badge {
  display: inline-block;
  margin-left: 0.35rem;
  padding: 0.1rem 0.4rem;
  border-radius: 0.35rem;
  font-size: 0.7rem;
  font-weight: 600;
  letter-spacing: 0.02em;
  text-transform: uppercase;
  color: var(--bp-ink-muted, #8a8178);
  border: 1px solid var(--bp-border, rgba(196, 165, 116, 0.4));
  vertical-align: middle;
}

.radius-row {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.radius-label {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--bp-ink, #f3ece4);
}

.radius-options {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
}

.radius-hint {
  max-width: 36rem;
}

.search-wait {
  padding: 1rem 1.1rem;
  border: 1px solid var(--bp-border, rgba(196, 165, 116, 0.35));
  border-radius: 0.75rem;
  background: var(--bp-elevated, rgba(28, 24, 20, 0.55));
}

.wait-audio-player {
  display: block;
  width: 100%;
  max-width: 28rem;
  height: 2.5rem;
}

.church-results {
  max-height: min(28rem, 55vh);
  overflow: auto;
}

.item-list-row.active {
  outline: 1px solid var(--bp-gold, #c4a574);
  border-radius: 0.5rem;
}

.church-detail,
.suggest-panel {
  padding: 1rem 1.1rem;
  border: 1px solid var(--bp-border, rgba(196, 165, 116, 0.35));
  border-radius: 0.75rem;
  background: var(--bp-elevated, rgba(28, 24, 20, 0.55));
}

.detail-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.seminary-block {
  padding-top: 0.75rem;
  border-top: 1px solid var(--bp-border, rgba(196, 165, 116, 0.25));
}

.seminary-block p {
  color: var(--bp-ink-muted, #8a8178);
  font-size: 0.95rem;
  line-height: 1.5;
}
</style>
