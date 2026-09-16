import { getAuthToken } from './authStorage'

export type NotificationResponse = {
  id: string
  title: string
  message: string
  sentAt: string | null
  createdAt: string
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''
const READ_NOTIFICATION_KEY = 'ekp.notifications.read'
export const NOTIFICATIONS_CHANGED_EVENT = 'ekp:notifications-changed'

async function request<T>(path: string): Promise<T> {
  const headers = new Headers()
  const token = getAuthToken()
  if (token) headers.set('Authorization', `Bearer ${token}`)
  let response: Response
  try { response = await fetch(`${API_BASE_URL}${path}`, { headers }) }
  catch { throw new Error('Unable to reach the notification service. Please try again.') }
  if (!response.ok) {
    let message = `Notification request failed (${response.status}).`
    try { const body = await response.json() as { message?: string }; if (body.message) message = body.message } catch { /* status message */ }
    throw new Error(message)
  }
  return response.json() as Promise<T>
}

export function getNotifications() { return request<NotificationResponse[]>('/api/Notification') }

export function getReadNotificationIds() {
  try {
    const stored = sessionStorage.getItem(READ_NOTIFICATION_KEY)
    const parsed = stored ? JSON.parse(stored) : []
    return Array.isArray(parsed) ? parsed.filter((id): id is string => typeof id === 'string') : []
  } catch { return [] }
}

export function markNotificationRead(id: string) {
  const readIds = new Set(getReadNotificationIds())
  readIds.add(id)
  sessionStorage.setItem(READ_NOTIFICATION_KEY, JSON.stringify([...readIds]))
  window.dispatchEvent(new Event(NOTIFICATIONS_CHANGED_EVENT))
}
