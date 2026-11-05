import type { ReactNode } from 'react'
import { ApiError } from '../api'

/** A labelled input with the API's validation messages for `field` underneath. */
export function Field({ label, field, errors, children }: { label: string; field: string; errors: Record<string, string[]>; children: ReactNode }) {
  const messages = errors[field] ?? []
  return (
    <label style={{ display: 'block', marginBottom: '0.75rem' }}>
      <span style={{ display: 'block' }}>{label}</span>
      {children}
      {messages.map((m) => (
        <small key={m} role="alert" style={{ display: 'block', color: 'crimson' }}>
          {m}
        </small>
      ))}
    </label>
  )
}

/** The message to show for a failed call: the ProblemDetails title, or a generic fallback. */
export function errorMessage(error: unknown): string {
  if (error instanceof ApiError) return error.message
  return error instanceof Error ? error.message : String(error)
}

/** Per-field errors from a 400, or none. */
export function fieldErrors(error: unknown): Record<string, string[]> {
  return error instanceof ApiError && error.status === 400 ? error.errors : {}
}
