import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred';
      
      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = error.error.message;
      } else {
        // Server-side error
        switch (error.status) {
          case 400:
            errorMessage = error.error?.message || error.error?.title || 'Bad request';
            if (error.error?.errors) {
              const errors = Object.values(error.error.errors).flat();
              errorMessage = errors.join(', ');
            }
            break;
          case 401:
            errorMessage = 'Unauthorized. Please log in.';
            break;
          case 403:
            errorMessage = 'You do not have permission to perform this action.';
            break;
          case 404:
            errorMessage = error.error?.message || 'Resource not found';
            break;
          case 409:
            errorMessage = error.error?.message || 'Conflict occurred';
            break;
          case 500:
            errorMessage = 'Server error. Please try again later.';
            break;
          default:
            errorMessage = error.error?.message || `Error: ${error.status}`;
        }
      }
      
      console.error('HTTP Error:', errorMessage, error);
      
      return throwError(() => ({
        status: error.status,
        message: errorMessage,
        originalError: error
      }));
    })
  );
};
