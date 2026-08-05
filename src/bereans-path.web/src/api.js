import { filterAndRankChurches } from './data/churchFit.js'

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? ''

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers ?? {}),
    },
    ...options,
  })

  if (response.status === 204) return null

  const text = await response.text()
  let data = null
  if (text) {
    try {
      data = JSON.parse(text)
    } catch {
      data = text
    }
  }

  if (!response.ok) {
    const message = data?.error || data?.title || response.statusText
    throw new Error(message || `Request failed (${response.status})`)
  }

  return data
}

export function fetchPassage(reference) {
  const params = new URLSearchParams({
    q: reference,
    'include-footnotes': 'false',
    'include-headings': 'false',
    'include-passage-references': 'false',
  })
  return request(`/api/esv?${params.toString()}`)
}

export function listBookmarks() {
  return request('/api/bookmarks')
}

export function createBookmark(payload) {
  return request('/api/bookmarks', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export function updateBookmark(id, payload) {
  return request(`/api/bookmarks/${id}`, {
    method: 'PATCH',
    body: JSON.stringify(payload),
  })
}

export function deleteBookmark(id) {
  return request(`/api/bookmarks/${id}`, { method: 'DELETE' })
}

/** Default ~10 miles — shorter radius usually returns sooner from Overpass. */
export const DEFAULT_CHURCH_RADIUS_METERS = 16000

export async function findNearbyChurches(lat, lon, radiusMeters = DEFAULT_CHURCH_RADIUS_METERS) {
  // Proxied through our API — Overpass often returns 406 to direct browser calls.
  const clamped = Math.min(50000, Math.max(1000, radiusMeters))
  const params = new URLSearchParams({
    lat: String(lat),
    lon: String(lon),
    radius: String(clamped),
  })
  const path = `/api/churches/nearby?${params.toString()}`

  let lastError
  for (let attempt = 1; attempt <= 2; attempt++) {
    const controller = new AbortController()
    const timer = setTimeout(() => controller.abort(), 80000)
    try {
      const results = await request(path, { signal: controller.signal })
      return filterAndRankChurches(Array.isArray(results) ? results : [], { lat, lon })
    } catch (err) {
      lastError = err?.name === 'AbortError'
        ? new Error('Church search timed out. Tap Find near me again in a moment.')
        : err
      if (attempt < 2) {
        await new Promise((resolve) => setTimeout(resolve, 1200))
      }
    } finally {
      clearTimeout(timer)
    }
  }
  throw lastError
}

export function listSavedChurches() {
  return request('/api/churches/saved')
}

export function saveChurch(payload) {
  return request('/api/churches/saved', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export function deleteSavedChurch(id) {
  return request(`/api/churches/saved/${id}`, { method: 'DELETE' })
}

export function suggestChurch(payload) {
  return request('/api/churches/suggestions', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export function listChurchSuggestions() {
  return request('/api/churches/suggestions')
}

export function fetchHealth() {
  return request('/api/diagnostics/health')
}

export function fetchLogs(take = 200) {
  return request(`/api/diagnostics/logs?take=${take}`)
}

export function clearLogs() {
  return request('/api/diagnostics/logs', { method: 'DELETE' })
}
