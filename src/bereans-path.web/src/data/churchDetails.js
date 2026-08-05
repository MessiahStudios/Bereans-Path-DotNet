import { getEnrichment, SEMINARY_GUIDANCE } from './churchEnrichment'

function tag(church, ...keys) {
  const tags = church?.tags || {}
  for (const key of keys) {
    const value = tags[key]
    if (value && String(value).trim()) return String(value).trim()
  }
  return ''
}

function ensureUrl(raw) {
  if (!raw) return ''
  const value = raw.trim()
  if (!value) return ''
  if (/^https?:\/\//i.test(value)) return value
  return `https://${value}`
}

export function buildChurchDetails(church) {
  if (!church) return null

  const enrichment = getEnrichment(church) || {}
  const website = ensureUrl(
    enrichment.website ||
      tag(church, 'website', 'contact:website', 'url') ||
      church.website,
  )
  const phone = tag(church, 'phone', 'contact:phone') || enrichment.phone || ''
  const address =
    enrichment.address ||
    [
      tag(church, 'addr:housenumber'),
      tag(church, 'addr:street'),
      tag(church, 'addr:city'),
      tag(church, 'addr:state'),
      tag(church, 'addr:postcode'),
    ]
      .filter(Boolean)
      .join(' ')
      .replace(/\s+/g, ' ')
      .trim() ||
    tag(church, 'addr:full')

  const lat = church.latitude
  const lon = church.longitude
  const query = encodeURIComponent(
    address || `${church.name}${lat != null ? ` @ ${lat},${lon}` : ''}`,
  )

  const directions = {
    google: `https://www.google.com/maps/dir/?api=1&destination=${lat},${lon}`,
    apple: `https://maps.apple.com/?daddr=${lat},${lon}&q=${encodeURIComponent(church.name || 'Church')}`,
    geo: `geo:${lat},${lon}?q=${lat},${lon}(${encodeURIComponent(church.name || 'Church')})`,
    search: `https://www.google.com/maps/search/?api=1&query=${query}`,
  }

  return {
    name: church.name,
    denomination: church.denomination || tag(church, 'denomination') || '',
    distanceKm: church.distanceKm,
    website,
    phone,
    address,
    seminaryNote: enrichment.seminaryNote || '',
    seminaryGuidance: SEMINARY_GUIDANCE,
    directions,
    osmId: church.osmId,
    latitude: lat,
    longitude: lon,
    tags: church.tags || {},
  }
}
