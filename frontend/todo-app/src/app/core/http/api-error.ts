import { HttpErrorResponse } from '@angular/common/http';

import { ProblemDetails } from '../models/problem-details';

const DEFAULT_MESSAGE = 'Something went wrong. Please try again.';

/**
 * An API error with the ProblemDetails `title` and field `errors` already read (ARCHITECTURE §6).
 * Field names are camelCase, matching the request properties (e.g. `email`, `name`).
 */
export class ApiError extends Error {
  readonly status: number;
  readonly fieldErrors: Readonly<Record<string, string[]>>;

  constructor(response: HttpErrorResponse) {
    const problem = readProblemDetails(response.error);
    super(problem?.title ?? DEFAULT_MESSAGE);
    this.name = 'ApiError';
    this.status = response.status;
    this.fieldErrors = problem?.errors ?? {};
  }

  /** The first message for a field, or `null`. */
  fieldError(field: string): string | null {
    return this.fieldErrors[field]?.[0] ?? null;
  }
}

function readProblemDetails(body: unknown): ProblemDetails | null {
  return typeof body === 'object' && body !== null ? (body as ProblemDetails) : null;
}
