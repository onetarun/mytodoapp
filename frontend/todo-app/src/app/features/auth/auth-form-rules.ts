/** Field rules shared by the auth forms; they mirror the API validators (NFR-8, FR-1). */
export const AUTH_FORM_RULES = {
  nameMaxLength: 100,
  emailMaxLength: 256,
  passwordMinLength: 8,
  passwordHint: 'Min 8 characters, with a letter and a number',
} as const;
