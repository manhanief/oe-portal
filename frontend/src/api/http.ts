/**
 * Small wrapper around fetch for calling the .NET API.
 * The API returns errors as "ProblemDetails" JSON; we turn those into an ApiError
 * with a message that can be shown to the user.
 */

export class ApiError extends Error {
  readonly status: number
  /** Field errors from validation (HTTP 400), e.g. { TableId: ["Please choose a table..."] } */
  readonly fieldErrors: Record<string, string[]>

  constructor(message: string, status: number, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  let response: Response
  try {
    response = await fetch(`/api${path}`, {
      ...init,
      headers: {
        Accept: 'application/json',
        ...(init.body ? { 'Content-Type': 'application/json' } : {}),
        ...init.headers,
      },
    })
  } catch {
    throw new ApiError('Cannot reach the restaurant server. Check your connection and try again.', 0)
  }

  if (response.ok) {
    return response.status === 204 ? (undefined as T) : ((await response.json()) as T)
  }

  let problem: ProblemDetails = {}
  try {
    problem = (await response.json()) as ProblemDetails
  } catch {
    // body was not JSON; fall back to a generic message below
  }

  const fieldErrors = problem.errors ?? {}
  const firstFieldError = Object.values(fieldErrors)[0]?.[0]
  const message =
    firstFieldError ?? problem.detail ?? problem.title ?? `Request failed (HTTP ${response.status}).`

  throw new ApiError(message, response.status, fieldErrors)
}

export const http = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
  patch: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PATCH', body: JSON.stringify(body) }),
}
