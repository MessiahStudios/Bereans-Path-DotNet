/**
 * Optional hand-curated notes for churches that matter to Bereans Path.
 * Key by OSM id when known (stable), or normalized name as fallback.
 */
export const CHURCH_ENRICHMENT = {
  'way/916875517': {
    website: 'https://nccphx.org/',
    address: '16615 N 43rd Ave, Phoenix, AZ 85053',
    seminaryNote:
      'Non-denominational Bible-teaching church. Ask about pastoral training and whether Lord’s Day preaching walks verse-by-verse through Scripture.',
  },
  'northwest community church': {
    website: 'https://nccphx.org/',
    address: '16615 N 43rd Ave, Phoenix, AZ 85053',
    seminaryNote:
      'Non-denominational Bible-teaching church. Ask about pastoral training and whether Lord’s Day preaching walks verse-by-verse through Scripture.',
  },
}

/** Default guidance when we do not have a curated seminary note. */
export const SEMINARY_GUIDANCE = {
  title: 'Seminary & teaching',
  body: 'OpenStreetMap rarely lists pastoral training. When you visit, ask whether the pastors are seminary-trained and whether the pulpit is committed to expository (verse-by-verse) preaching — the same care commended of the Bereans in Acts 17.',
  links: [
    { name: "The Master's Seminary", href: 'https://www.tms.edu/' },
    { name: "The Expositor's Seminary", href: 'https://www.expositors.org/' },
  ],
}

export function normalizeChurchKey(name = '') {
  return String(name).trim().toLowerCase().replace(/\s+/g, ' ')
}

export function getEnrichment(church) {
  if (!church) return null
  if (church.osmId && CHURCH_ENRICHMENT[church.osmId]) {
    return CHURCH_ENRICHMENT[church.osmId]
  }
  const byName = CHURCH_ENRICHMENT[normalizeChurchKey(church.name)]
  return byName || null
}
