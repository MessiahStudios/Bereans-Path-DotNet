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

export async function findNearbyChurches(lat, lon, radiusMeters = 50000) {
  // Public Overpass API — no secret key (matches original Flask Bereans Path).
  const clamped = Math.min(50000, Math.max(1000, radiusMeters))
  const query = `
    [out:json][timeout:25];
    (
      node["amenity"="place_of_worship"]["religion"="christian"](around:${clamped},${lat},${lon});
      way["amenity"="place_of_worship"]["religion"="christian"](around:${clamped},${lat},${lon});
      relation["amenity"="place_of_worship"]["religion"="christian"](around:${clamped},${lat},${lon});
    );
    out center;
  `

  const response = await fetch(
    `https://overpass-api.de/api/interpreter?data=${encodeURIComponent(query)}`,
  )

  if (!response.ok) {
    throw new Error('Church search provider failed. Try again in a moment.')
  }

  const data = await response.json()
  const results = []

  for (const element of data.elements ?? []) {
    const latitude = element.lat ?? element.center?.lat
    const longitude = element.lon ?? element.center?.lon
    if (latitude == null || longitude == null) continue

    results.push({
      name: element.tags?.name || 'Church',
      latitude,
      longitude,
      osmId: `${element.type}/${element.id}`,
    })
  }

  return results
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

export function fetchHealth() {
  return request('/api/diagnostics/health')
}

export function fetchLogs(take = 200) {
  return request(`/api/diagnostics/logs?take=${take}`)
}

export function clearLogs() {
  return request('/api/diagnostics/logs', { method: 'DELETE' })
}
