import { getAuthToken } from './authStorage'

export type GraphEntityResponse = {
  id: string
  name: string
  canonicalName: string
  entityTypeId: string
  entityTypeName: string
}

export type GraphRelationshipResponse = {
  id: string
  sourceEntityId: string
  targetEntityId: string
  predicate: string
  confidence: number
}

export type EntityGraphResponse = {
  entity: GraphEntityResponse
  outgoingRelationships: GraphRelationshipResponse[]
  incomingRelationships: GraphRelationshipResponse[]
  connectedEntities: GraphEntityResponse[]
}

export type GraphSubgraphResponse = {
  entities: GraphEntityResponse[]
  relationships: GraphRelationshipResponse[]
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
    throw new Error('Unable to reach the knowledge graph service. Please try again.')
  }

  if (!response.ok) {
    let message = `Knowledge graph request failed (${response.status}).`
    try {
      const body = await response.json() as { message?: string }
      if (body.message) message = body.message
    } catch {
      // Keep the status-based message when the response is not JSON.
    }
    throw new Error(message)
  }

  return response.json() as Promise<T>
}

export function searchGraphEntities(query: string) {
  return request<GraphEntityResponse[]>(`/api/GraphQuery/search?query=${encodeURIComponent(query)}`)
}

export function getGraphEntity(id: string) {
  return request<EntityGraphResponse>(`/api/GraphQuery/entity/${encodeURIComponent(id)}`)
}

export function getGraphSubgraph(id: string, depth = 1) {
  return request<GraphSubgraphResponse>(`/api/GraphQuery/subgraph/${encodeURIComponent(id)}?depth=${depth}`)
}

