<script setup>
import { nextTick, onBeforeUnmount, onMounted, ref } from 'vue'
import L from 'leaflet'
import markerIcon2x from 'leaflet/dist/images/marker-icon-2x.png'
import markerIcon from 'leaflet/dist/images/marker-icon.png'
import markerShadow from 'leaflet/dist/images/marker-shadow.png'
import { deleteSavedChurch, findNearbyChurches, listSavedChurches, saveChurch } from '../api'

// Fix default marker paths under Vite
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
let map
let layerGroup

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
  } else {
    map.setView([lat, lon], 12)
  }
}

async function locateAndSearch() {
  error.value = ''
  status.value = 'Getting your location…'

  if (!navigator.geolocation) {
    error.value = 'Geolocation is not supported in this browser.'
    status.value = ''
    return
  }

  navigator.geolocation.getCurrentPosition(async (position) => {
    const { latitude, longitude } = position.coords
    await nextTick()
    ensureMap(latitude, longitude)
    layerGroup.clearLayers()
    L.marker([latitude, longitude]).addTo(layerGroup).bindPopup('You are here').openPopup()

    status.value = 'Searching nearby churches…'
    try {
      churches.value = await findNearbyChurches(latitude, longitude)
      churches.value.forEach((church) => {
        L.marker([church.latitude, church.longitude])
          .addTo(layerGroup)
          .bindPopup(church.name)
      })
      status.value = `Found ${churches.value.length} churches nearby.`
    } catch (err) {
      error.value = err.message
      status.value = ''
    }
  }, (geoError) => {
    error.value = geoError.message || 'Unable to get location.'
    status.value = ''
  })
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
        <h1 class="h4 mb-1">Church finder</h1>
        <p class="muted mb-0">Uses your location and OpenStreetMap Overpass via the API.</p>
      </div>
      <button class="btn btn-primary" type="button" @click="locateAndSearch">
        Find near me
      </button>
    </div>

    <p v-if="status" class="text-secondary">{{ status }}</p>
    <div v-if="error" class="alert alert-warning py-2">{{ error }}</div>

    <div ref="mapEl" class="map-frame mb-3"></div>

    <div class="row g-3">
      <div class="col-lg-7">
        <h2 class="h6">Nearby results</h2>
        <div v-if="!churches.length" class="muted">Run a search to see churches here.</div>
        <ul v-else class="list-group">
          <li
            v-for="church in churches.slice(0, 25)"
            :key="church.osmId"
            class="list-group-item d-flex justify-content-between align-items-center"
          >
            <span>{{ church.name }}</span>
            <button class="btn btn-sm btn-outline-primary" type="button" @click="onSave(church)">
              Save
            </button>
          </li>
        </ul>
      </div>
      <div class="col-lg-5">
        <h2 class="h6">Saved churches</h2>
        <div v-if="!saved.length" class="muted">None saved yet.</div>
        <ul v-else class="list-group">
          <li
            v-for="church in saved"
            :key="church.id"
            class="list-group-item d-flex justify-content-between align-items-center"
          >
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
