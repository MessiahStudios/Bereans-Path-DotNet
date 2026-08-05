export const THEMES = {
  faith: {
    name: 'Faith & Scripture',
    swatchBg: '#0c0b0a',
    swatchAccent: '#c4a574',
    tokens: {
      // Matched to https://www.messiahstudios.site/ brand tokens
      '--bp-ink': '#f3ece4',
      '--bp-muted': '#c4b8ad',
      '--bp-paper': '#0c0b0a',
      '--bp-surface': '#1c1814',
      '--bp-accent': '#c4a574',
      '--bp-accent-deep': '#e0c090',
      '--bp-sea': '#e0c090',
      '--bp-border': 'rgba(243, 236, 228, 0.12)',
      '--bp-shadow': '0 10px 30px rgba(0, 0, 0, 0.45)',
      '--bp-btn-text': '#0c0b0a',
      '--bp-elevated': '#161311',
      '--bp-font-body': '"Source Sans 3", "Segoe UI", sans-serif',
      '--bp-font-display': '"Cormorant Garamond", Georgia, serif',
    },
  },
  'orange-saas': {
    name: 'Rock Orange',
    swatchBg: '#CC5200',
    swatchAccent: '#FF8533',
    tokens: {
      '--bp-ink': '#F0F2F5',
      '--bp-muted': '#8B95A1',
      '--bp-paper': '#111418',
      '--bp-surface': '#1E2329',
      '--bp-accent': '#FF6A00',
      '--bp-accent-deep': '#FF8533',
      '--bp-sea': '#FF6A00',
      '--bp-border': 'rgba(255,255,255,0.08)',
      '--bp-shadow': '0 10px 30px rgba(0,0,0,0.45)',
      '--bp-btn-text': '#FFFFFF',
    },
  },
  'desert-phoenix': {
    name: 'Desert Phoenix',
    swatchBg: '#1D1D1D',
    swatchAccent: '#F4A261',
    tokens: {
      '--bp-ink': '#F1FAEE',
      '--bp-muted': 'rgba(241,250,238,0.60)',
      '--bp-paper': '#1D1D1D',
      '--bp-surface': '#264653',
      '--bp-accent': '#F4A261',
      '--bp-accent-deep': '#E8926F',
      '--bp-sea': '#F4A261',
      '--bp-border': 'rgba(255,255,255,0.07)',
      '--bp-shadow': '0 10px 30px rgba(0,0,0,0.40)',
      '--bp-btn-text': '#1D1D1D',
    },
  },
  'modern-dev': {
    name: 'Modern Dev',
    swatchBg: '#111827',
    swatchAccent: '#38BDF8',
    tokens: {
      '--bp-ink': '#E5E7EB',
      '--bp-muted': '#9CA3AF',
      '--bp-paper': '#111827',
      '--bp-surface': '#1F2937',
      '--bp-accent': '#38BDF8',
      '--bp-accent-deep': '#22D3EE',
      '--bp-sea': '#38BDF8',
      '--bp-border': 'rgba(255,255,255,0.07)',
      '--bp-shadow': '0 10px 30px rgba(0,0,0,0.40)',
      '--bp-btn-text': '#111827',
    },
  },
  cinematic: {
    name: 'Cinematic',
    swatchBg: '#1A1A2E',
    swatchAccent: '#E94560',
    tokens: {
      '--bp-ink': '#F1F1F1',
      '--bp-muted': 'rgba(241,241,241,0.55)',
      '--bp-paper': '#1A1A2E',
      '--bp-surface': '#16213E',
      '--bp-accent': '#E94560',
      '--bp-accent-deep': '#FF5570',
      '--bp-sea': '#E94560',
      '--bp-border': 'rgba(255,255,255,0.07)',
      '--bp-shadow': '0 10px 30px rgba(0,0,0,0.40)',
      '--bp-btn-text': '#FFFFFF',
    },
  },
  editorial: {
    name: 'Minimal Editorial',
    swatchBg: '#FAFAFA',
    swatchAccent: '#2563EB',
    tokens: {
      '--bp-ink': '#1A1A1A',
      '--bp-muted': '#6B7280',
      '--bp-paper': '#FAFAFA',
      '--bp-surface': '#FFFFFF',
      '--bp-accent': '#2563EB',
      '--bp-accent-deep': '#1D4ED8',
      '--bp-sea': '#1A1A1A',
      '--bp-border': '#E5E7EB',
      '--bp-shadow': '0 10px 30px rgba(0,0,0,0.08)',
      '--bp-btn-text': '#FFFFFF',
    },
  },
}

const STORAGE_KEY = 'bereans-theme'

const BASE_TOKENS = {
  '--bp-elevated': '#161311',
  '--bp-font-body': '"Source Sans 3", "Segoe UI", sans-serif',
  '--bp-font-display': '"Cormorant Garamond", Georgia, serif',
}

export function applyTheme(themeId) {
  const theme = THEMES[themeId] || THEMES.faith
  const root = document.documentElement
  const merged = { ...BASE_TOKENS, ...theme.tokens }
  Object.entries(merged).forEach(([prop, val]) => {
    root.style.setProperty(prop, val)
  })
  root.setAttribute('data-theme', themeId)
  localStorage.setItem(STORAGE_KEY, themeId)
  return themeId
}

export function getSavedTheme() {
  return localStorage.getItem(STORAGE_KEY) || 'faith'
}
