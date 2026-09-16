import { getAuthToken } from './authStorage'
import type { DocumentResponse } from './documentApi'
import type { ProcessingJobResponse } from './processingJobApi'

export type SemanticBlockResponse = {
  id: string
  documentId: string
  text: string
  blockIndex: number
  page: number
  createdAt: string
}

export type TripleResponse = {
  id: string
  subject: string
  predicate: string
  object: string
  confidence: number
  status: 'Pending' | 'Validated' | 'Rejected' | number
  createdAt: string
}

export type TripleProvenanceResponse = {
  id: string
  tripleId: string
  documentId: string
  semanticBlockId: string
  createdAt: string
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

async function request<T>(path: string): Promise<T> {
  const headers = new Headers()
  const token = getAuthToken()
  if (token) headers.set('Authorization', `Bearer ${token}`)

  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, { headers })
  } catch {
    throw new Error('Unable to reach the processing results service. Please try again.')
  }

  if (!response.ok) {
    let message = `Results request failed (${response.status}).`
    try {
      const body = await response.json() as { message?: string }
      if (body.message) message = body.message
    } catch {
      // Keep the status-based message when the server response is not JSON.
    }
    throw new Error(message)
  }

  return response.json() as Promise<T>
}

export function getProcessingJob(id: string) {
  return request<ProcessingJobResponse>(`/api/ProcessingJob/${encodeURIComponent(id)}`)
}

export function getDocument(id: string) {
  return request<DocumentResponse>(`/api/Document/${encodeURIComponent(id)}`)
}

export function getSemanticBlocks() {
  return request<SemanticBlockResponse[]>('/api/SemanticBlock')
}

export function getTriples() {
  return request<TripleResponse[]>('/api/Triple')
}

export function getTripleProvenance() {
  return request<TripleProvenanceResponse[]>('/api/TripleProvenance')
}

