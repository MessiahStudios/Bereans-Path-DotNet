<script setup>
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import L from 'leaflet'
import markerIcon2x from 'leaflet/dist/images/marker-icon-2x.png'
import markerIcon from 'leaflet/dist/images/marker-icon.png'
import markerShadow from 'leaflet/dist/images/marker-shadow.png'
import { deleteSavedChurch, findNearbyChurches, listSavedChurches, saveChurch, suggestChurch } from '../api'
import { CHURCH_FIT_BLURB, CHURCH_FIT_BLURB_DETAIL } from '../data/churchFit'
import { buildChurchDetails } from '../data/churchDetails'
import { RouterLink } from 'vue-router'

delete L.Icon.Default.prototype._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: markerIcon2x,
  iconUrl: markerIcon,
  shadowUrl: markerShadow,
})

const mapEl = ref(null)
const status = ref('')
const error = ref('')
const churches = ref([])
const saved = ref([])
const searching = ref(false)
const mapReady = ref(false)
const selected = ref(null)
const showSuggest = ref(false)
const suggestStatus = ref('')
const suggestError = ref('')
const suggesting = ref(false)

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

function ensureMap(lat, lon) {
  if (!map) {
    map = L.map(mapEl.value).setView([lat, lon], 12)
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
    }).addTo(map)
    layerGroup = L.layerGroup().addTo(map)
    mapReady.value = true
  } else {
    map.setView([lat, lon], 12)
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

async function locateAndSearch() {
  error.value = ''
  status.value = 'Getting your location…'
  searching.value = true
  selected.value = null

  if (!navigator.geolocation) {
    error.value = 'Geolocation is not supported in this browser.'
    status.value = ''
    searching.value = false
    return
  }

  navigator.geolocation.getCurrentPosition(async (position) => {
    const { latitude, longitude } = position.coords
    await nextTick()
    ensureMap(latitude, longitude)
    layerGroup.clearLayers()
    markerByOsmId = new Map()
    L.marker([latitude, longitude]).addTo(layerGroup).bindPopup('You are here').openPopup()

    status.value = 'Searching nearby churches… (map servers can take up to a minute)'
    try {
      churches.value = await findNearbyChurches(latitude, longitude)
      churches.value.forEach((church) => {
        const marker = L.marker([church.latitude, church.longitude])
          .addTo(layerGroup)
          .bindPopup(church.name)
        marker.on('click', () => selectChurch(church))
        markerByOsmId.set(church.osmId, marker)
      })
      status.value = churches.value.length
        ? `Found ${churches.value.length} churches within about 19 miles — nearest first.`
        : 'No matching churches found in range. Try again from a closer location.'
    } catch (err) {
      error.value = err.message
      status.value = ''
    } finally {
      searching.value = false
    }
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
})

onBeforeUnmount(() => {
  if (map) {
    map.remove()
    map = null
  }
})
</script>

<template>
  <section class="panel">
    <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
      <div>
        <h2 class="h4 mb-1">Church finder</h2>
        <p class="muted mb-1">Find Protestant and non-denominational churches near you.</p>
        <p class="church-fit-blurb mb-1">{{ CHURCH_FIT_BLURB }}</p>
        <p class="small muted mb-0">
          {{ CHURCH_FIT_BLURB_DETAIL }}
          <RouterLink to="/path">Why this matters →</RouterLink>
        </p>
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

    <p v-if="status" class="text-secondary">{{ status }}</p>
    <p v-if="suggestStatus" class="text-success">{{ suggestStatus }}</p>
    <div v-if="error" class="alert alert-warning py-2">{{ error }}</div>

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
        Tap <strong>Find near me</strong> to load the map around your location.
      </div>
    </div>

    <div v-if="details" class="church-detail mb-3">
      <div class="d-flex justify-content-between align-items-start gap-2 mb-2">
        <div>
          <h3 class="h5 mb-1">{{ details.name }}</h3>
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
        <h3 class="h6">Nearby results</h3>
        <div v-if="!churches.length" class="muted">Run a search to see churches here.</div>
        <ul v-else class="item-list church-results">
          <li
            v-for="church in churches"
            :key="church.osmId"
            class="item-list-row"
            :class="{ active: selected?.osmId === church.osmId }"
          >
            <button class="item-list-main" type="button" @click="selectChurch(church)">
              <span class="d-block">{{ church.name }}</span>
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
