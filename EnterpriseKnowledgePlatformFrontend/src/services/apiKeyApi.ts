import { getAuthToken } from './authStorage'

export type ApiKeyResponse = {
  id: string
  label: string
  expiresAt: string | null
  isActive: boolean
  createdAt: string
}

export type CreateApiKeyResponse = ApiKeyResponse & { apiKey: string }

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  const token = getAuthToken()
  if (token) headers.set('Authorization', `Bearer ${token}`)
  let response: Response
  try { response = await fetch(`${API_BASE_URL}${path}`, { ...init, headers }) }
  catch { throw new Error('Unable to reach the API key service. Please try again.') }
  if (!response.ok) {
    let message = `API key request failed (${response.status}).`
    try { const body = await response.json() as { message?: string }; if (body.message) message = body.message } catch { /* status message */ }
    throw new Error(message)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

export function getApiKeys() { return request<ApiKeyResponse[]>('/api/ApiKey') }

export function createApiKey(label: string, expiresAt: string | null) {
  return request<CreateApiKeyResponse>('/api/ApiKey', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ Label: label, ExpiresAt: expiresAt }) })
}

export function updateApiKey(key: ApiKeyResponse, isActive: boolean) {
  return request<ApiKeyResponse>(`/api/ApiKey/${encodeURIComponent(key.id)}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ Label: key.label, ExpiresAt: key.expiresAt, IsActive: isActive }) })
}

export function deleteApiKey(id: string) { return request<void>(`/api/ApiKey/${encodeURIComponent(id)}`, { method: 'DELETE' }) }
