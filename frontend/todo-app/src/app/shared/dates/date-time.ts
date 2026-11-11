const pad = (value: number): string => value.toString().padStart(2, '0');

/**
 * UTC ISO string from the API → `yyyy-MM-ddTHH:mm` in the browser's local time,
 * the value format of `<input type="datetime-local">` (Assumption 3).
 */
export function toLocalDateTimeInput(utcIso: string | null): string {
  if (utcIso === null) {
    return '';
  }

  const date = new Date(utcIso);
  return (
    `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` +
    `T${pad(date.getHours())}:${pad(date.getMinutes())}`
  );
}

/** `datetime-local` value (local time) → UTC ISO string for the API, or `null` when empty. */
export function fromLocalDateTimeInput(localValue: string): string | null {
  if (localValue.trim() === '') {
    return null;
  }

  const date = new Date(localValue);
  return Number.isNaN(date.getTime()) ? null : date.toISOString();
}
