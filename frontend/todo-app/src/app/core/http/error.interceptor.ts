import { HttpErrorResponse, HttpInterceptorFn, HttpStatusCode } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import { ToastService } from '../ui/toast.service';
import { ApiError } from './api-error';

const LOGIN_URL = `${environment.apiBaseUrl}/auth/login`;
const SESSION_EXPIRED_MESSAGE = 'Your session has expired. Please log in again.';
const TOO_MANY_REQUESTS_MESSAGE = 'Too many requests, try again shortly.';
const SERVER_ERROR_MESSAGE = 'The server could not be reached or had a problem. Please try again.';

/**
 * Central API error handling (NFR-18): 401 → logout + login page; 429 → "try again shortly";
 * network/5xx → friendly message. Every error is re-thrown as an {@link ApiError}
 * so pages can show the ProblemDetails title and field errors (400, 404, 423).
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const toast = inject(ToastService);

  return next(request).pipe(
    catchError((response: unknown) => {
      if (!(response instanceof HttpErrorResponse)) {
        return throwError(() => response);
      }

      const isLoginCall = request.url === LOGIN_URL;
      if (response.status === HttpStatusCode.Unauthorized && !isLoginCall) {
        toast.error(SESSION_EXPIRED_MESSAGE);
        auth.logout();
      } else if (response.status === HttpStatusCode.TooManyRequests) {
        toast.error(TOO_MANY_REQUESTS_MESSAGE);
      } else if (response.status === 0 || response.status >= HttpStatusCode.InternalServerError) {
        toast.error(SERVER_ERROR_MESSAGE);
      }

      return throwError(() => new ApiError(response));
    }),
  );
};
