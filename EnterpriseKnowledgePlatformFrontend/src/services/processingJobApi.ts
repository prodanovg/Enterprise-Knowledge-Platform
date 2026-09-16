import { getAuthToken } from './authStorage'

export type ProcessingJobStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed' | number

export type ProcessingJobResponse = {
  id: string
  documentId: string
  status: ProcessingJobStatus
  startedAt: string | null
  finishedAt: string | null
  errorMessage: string | null
  createdAt: string
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  const token = getAuthToken()
  if (token) headers.set('Authorization', `Bearer ${token}`)

  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, { ...init, headers })
  } catch {
    throw new Error('Unable to reach the processing service. Please try again.')
  }

  if (!response.ok) {
    let message = `Processing request failed (${response.status}).`
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

export function getProcessingJobs() {
  return request<ProcessingJobResponse[]>('/api/ProcessingJob')
}

export function startProcessing(documentIds: string[]) {
  return request<ProcessingJobResponse[]>('/api/ProcessingJob/run', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ DocumentIds: documentIds }),
  })
}

export function retryProcessingJob(id: string) {
  return request<ProcessingJobResponse>(`/api/ProcessingJob/${encodeURIComponent(id)}/retry`, { method: 'POST' })
}

