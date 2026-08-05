/**
 * Church fit for Bereans Path.
 * Prefer Protestant / non-denominational congregations whose teaching
 * can sit with expository, Scripture-first study (see Resources).
 */

/** OSM denomination=* values to exclude (and close variants). */
const EXCLUDED_DENOMINATIONS = new Set([
  'catholic',
  'roman_catholic',
  'roman catholic',
  'greek_catholic',
  'eastern_catholic',
  'mormon',
  'latter_day_saints',
  'latter-day_saints',
  'lds',
  'jehovahs_witness',
  'jehovahs_witnesses',
  'jehovah\'s_witness',
  'jehovah\'s_witnesses',
  // Eastern / Oriental Orthodox — tradition over sola Scriptura fit
  'orthodox',
  'eastern_orthodox',
  'greek_orthodox',
  'russian_orthodox',
  'serbian_orthodox',
  'romanian_orthodox',
  'ukrainian_orthodox',
  'antiochian_orthodox',
  'coptic',
  'coptic_orthodox',
  'oriental_orthodox',
  'armenian_apostolic',
  'syriac_orthodox',
  'ethiopian_orthodox',
  // Extra-biblical authority / not Protestant evangelical fit
  'unitarian',
  'unitarian_universalist',
  'christian_science',
  'church_of_christ_scientist',
  'new_apostolic',
  'scientology',
])

/** Name / tag phrases that mark an excluded congregation. */
const EXCLUDED_NAME_PATTERNS = [
  /\bcatholic\b/i,
  /\broman\s+catholic\b/i,
  /\bour\s+lady\b/i,
  /\bsacred\s+heart\b/i,
  /\bimaculada\b/i,
  /\bimmaculate\b/i,
  /\bholy\s+see\b/i,
  /\bbasilica\b/i,
  /\bmormon\b/i,
  /\blatter[- ]day\s+saints?\b/i,
  /\bchurch\s+of\s+jesus\s+christ\s+of\s+latter/i,
  /\blds\b/i,
  /\bjehovah/i,
  /\bkingdom\s+hall\b/i,
  /\bjehovah.?s?\s+witness/i,
  /\borthodox\b/i,
  /\bcoptic\b/i,
  /\barmenian\s+apostolic\b/i,
  /\bunitarian\b/i,
  /\bchristian\s+science\b/i,
]

/**
 * Preferred OSM denominations — non-denom first, then common Protestant.
 * Unknown denominations are kept but ranked lower.
 */
const PREFERRED_DENOMINATIONS = new Set([
  'nondenominational',
  'non-denominational',
  'non_denominational',
  'independent',
  'evangelical',
  'bible',
  'bible_church',
  'baptist',
  'southern_baptist',
  'reformed',
  'presbyterian',
  'pca',
  'opc',
  'lutheran',
  'lcms',
  'wels',
  'methodist',
  'united_methodist',
  'wesleyan',
  'nazarene',
  'pentecostal',
  'assemblies_of_god',
  'ag',
  'foursquare',
  'calvary',
  'calvary_chapel',
  'christian',
  'christian_church',
  'church_of_christ',
  'disciples_of_christ',
  'congregational',
  'anglican',
  'anglican_church_in_north_america',
  'acna',
  'mennonite',
  'brethren',
  'evangelical_free',
  'cma',
  'christian_and_missionary_alliance',
  'interdenominational',
  'community',
])

const PREFERRED_NAME_PATTERNS = [
  /\bnon[- ]?denom/i,
  /\bindependent\b/i,
  /\bevangelical\b/i,
  /\bbible\s+(church|chapel|fellowship|community)\b/i,
  /\bbaptist\b/i,
  /\breformed\b/i,
  /\bpresbyterian\b/i,
  /\blutheran\b/i,
  /\bmethodist\b/i,
  /\bpentecostal\b/i,
  /\bassemblies\s+of\s+god\b/i,
  /\bcalvary\b/i,
  /\bcommunity\s+church\b/i,
  /\bfellowship\b/i,
  /\bgrace\b/i,
  /\bchapel\b/i,
]

function normalizeDenom(raw) {
  if (!raw) return ''
  return String(raw).trim().toLowerCase().replace(/-/g, '_')
}

function haystack(tags = {}, name = '') {
  const parts = [
    name,
    tags.name,
    tags.denomination,
    tags['denomination:en'],
    tags.brand,
    tags.operator,
    tags.church,
    tags.description,
  ]
  return parts.filter(Boolean).join(' · ')
}

export function isExcludedChurch({ name = '', tags = {} } = {}) {
  const denoms = normalizeDenom(tags.denomination)
    .split(/[;,/|]/)
    .map((d) => d.trim())
    .filter(Boolean)

  if (denoms.some((d) => EXCLUDED_DENOMINATIONS.has(d))) return true

  const text = haystack(tags, name)
  return EXCLUDED_NAME_PATTERNS.some((re) => re.test(text))
}

export function isPreferredChurch({ name = '', tags = {} } = {}) {
  const denoms = normalizeDenom(tags.denomination)
    .split(/[;,/|]/)
    .map((d) => d.trim())
    .filter(Boolean)

  if (denoms.some((d) => PREFERRED_DENOMINATIONS.has(d))) return true

  const text = haystack(tags, name)
  return PREFERRED_NAME_PATTERNS.some((re) => re.test(text))
}

/** Lower score sorts first. */
export function churchFitScore(church) {
  if (isExcludedChurch(church)) return 100
  if (isPreferredChurch(church)) {
    const denoms = normalizeDenom(church.tags?.denomination || church.denomination)
    if (/non.?denom|independent|evangelical|bible/.test(denoms) || /\bnon[- ]?denom|\bevangelical|\bbible\s/i.test(church.name || '')) {
      return 0
    }
    return 1
  }
  // Untagged Christian places of worship — keep, but after clear Protestant fits
  return 2
}

function haversineKm(lat1, lon1, lat2, lon2) {
  const toRad = (d) => (d * Math.PI) / 180
  const R = 6371
  const dLat = toRad(lat2 - lat1)
  const dLon = toRad(lon2 - lon1)
  const a =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLon / 2) ** 2
  return 2 * R * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
}

/**
 * Keep Protestant-fit churches, nearest first.
 * Fit score is a light secondary sort so non-denom peers still surface near the top when close.
 */
export function filterAndRankChurches(churches, origin = null) {
  const withMeta = churches
    .filter((c) => !isExcludedChurch(c))
    .map((c) => {
      const distanceKm =
        origin != null
          ? haversineKm(origin.lat, origin.lon, c.latitude, c.longitude)
          : null
      return {
        ...c,
        denomination: c.denomination || c.tags?.denomination || '',
        distanceKm,
        fitScore: churchFitScore(c),
      }
    })

  return withMeta.sort((a, b) => {
    if (a.distanceKm != null && b.distanceKm != null) {
      const byDistance = a.distanceKm - b.distanceKm
      if (Math.abs(byDistance) > 0.05) return byDistance
    }
    const byFit = a.fitScore - b.fitScore
    if (byFit !== 0) return byFit
    return (a.name || '').localeCompare(b.name || '')
  })
}

export const CHURCH_FIT_BLURB =
  'Bereans Path exists to help you find churches that treat Scripture as the final authority, examining the Word with the same care commended of the Bereans in Acts 17. We point users toward Protestant and non-denominational congregations committed to biblical authority, while excluding traditions such as Roman Catholic, Eastern Orthodox, LDS, Jehovah’s Witness, and similar groups whose doctrine rests on authorities beyond the closed canon of Scripture.'
