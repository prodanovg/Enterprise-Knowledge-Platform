export type LoginRequest = {
  username: string
  password: string
}

export type RegisterRequest = {
  name: string
  username: string
  email: string
  password: string
}

export type AuthResponse = {
  token: string
  expiration: string
  username: string
  email: string
  role: string
}

type ApiError = { message?: string }

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

async function post<T>(path: string, payload: object): Promise<T> {
  let response: Response

  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    })
  } catch {
    throw new Error('Unable to reach the authentication service. Please try again.')
  }

  if (!response.ok) {
    let message = 'The request could not be completed.'
    try {
      const error = await response.json() as ApiError
      if (error.message) message = error.message
    } catch {
      // Use the generic message when the server does not return JSON.
    }
    throw new Error(message)
  }

  return response.json() as Promise<T>
}

export function login(request: LoginRequest) {
  return post<AuthResponse>('/api/Auth/login', request)
}

export function register(request: RegisterRequest) {
  return post<AuthResponse>('/api/Auth/register', request)
}

