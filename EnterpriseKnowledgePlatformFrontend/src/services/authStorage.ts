const AUTH_TOKEN_KEY = 'ekp.auth.token'

export function storeAuthToken(token: string) {
  sessionStorage.setItem(AUTH_TOKEN_KEY, token)
}

export function getAuthToken() {
  return sessionStorage.getItem(AUTH_TOKEN_KEY)
}

export function clearAuthToken() {
  sessionStorage.removeItem(AUTH_TOKEN_KEY)
}

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const payload = token.split('.')[1]
    if (!payload) return null

    const normalizedPayload = payload.replace(/-/g, '+').replace(/_/g, '/')
    const paddedPayload = normalizedPayload.padEnd(Math.ceil(normalizedPayload.length / 4) * 4, '=')
    const binaryPayload = atob(paddedPayload)
    const bytes = Uint8Array.from(binaryPayload, (character) => character.charCodeAt(0))
    return JSON.parse(new TextDecoder().decode(bytes)) as Record<string, unknown>
  } catch {
    return null
  }
}

export function getAuthIdentity(): string | null {
  const token = getAuthToken()
  if (!token) return null

  const payload = decodeJwtPayload(token)
  const identityClaimKeys = [
    'preferred_username',
    'username',
    'name',
    'unique_name',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
  ]

  for (const claimKey of identityClaimKeys) {
    const claim = payload?.[claimKey]
    if (typeof claim === 'string' && claim.trim()) return claim.trim()
  }

  return null
}
