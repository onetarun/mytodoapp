/** RFC 7807 error body returned by the API (400 adds field-level `errors`). */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
