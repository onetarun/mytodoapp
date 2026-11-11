import { AuthUser } from './auth-user';

/** Returned by register and login. `expiresAt` is an ISO-8601 UTC string. */
export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: AuthUser;
}
