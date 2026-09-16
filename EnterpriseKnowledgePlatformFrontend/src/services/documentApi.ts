import { getAuthToken } from './authStorage'

export type DocumentStatus = 'Pending' | 'Processing' | 'Processed' | 'Failed' | number

export type DocumentResponse = {
  id: string
  name: string
  filePath: string
  fileType: string
  status: DocumentStatus
  createdAt: string
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = getAuthToken()
  const headers = new Headers(init.headers)
  if (token) headers.set('Authorization', `Bearer ${token}`)

  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, { ...init, headers })
  } catch {
    throw new Error('Unable to reach the document service. Please try again.')
  }

  if (!response.ok) {
    let message = `Document request failed (${response.status}).`
    try {
      const body = await response.json() as { message?: string }
      if (body.message) message = body.message
    } catch {
      // Keep the status-based message when the server response is not JSON.
    }
    throw new Error(message)
  }

  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

export function getDocuments() {
  return request<DocumentResponse[]>('/api/Document')
}

export function uploadDocument(name: string, file: File) {
  const formData = new FormData()
  formData.append('Name', name)
  formData.append('File', file)
  return request<DocumentResponse>('/api/Document', { method: 'POST', body: formData })
}

export function deleteDocument(id: string) {
  return request<void>(`/api/Document/${encodeURIComponent(id)}`, { method: 'DELETE' })
}

