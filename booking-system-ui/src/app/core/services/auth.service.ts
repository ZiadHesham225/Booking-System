import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, BehaviorSubject, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TokenService } from './token.service';
import {
  User,
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  RefreshTokenRequest
} from '../models';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();

  // Signals for reactive state
  private _isAuthenticated = signal(false);
  private _currentUser = signal<User | null>(null);

  isAuthenticated = this._isAuthenticated.asReadonly();
  currentUser = this._currentUser.asReadonly();
  isAdmin = computed(() => this.tokenService.isAdmin());

  constructor(
    private http: HttpClient,
    private router: Router,
    private tokenService: TokenService
  ) {
    this.initializeAuth();
  }

  private initializeAuth(): void {
    const token = this.tokenService.getToken();
    if (token && !this.tokenService.isTokenExpired()) {
      this._isAuthenticated.set(true);
      this.loadUserFromToken();
    }
  }

  private loadUserFromToken(): void {
    const payload = this.tokenService.getTokenPayload();
    if (payload) {
      const user: User = {
        id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['sub'] || '',
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload['email'] || '',
        firstName: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'] || '',
        lastName: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname'] || '',
        fullName: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || '',
        roles: this.tokenService.getUserRoles()
      };
      this._currentUser.set(user);
      this.currentUserSubject.next(user);
    }
  }

  login(request: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.handleAuthResponse(response.data);
        }
      }),
      catchError(error => throwError(() => error))
    );
  }

  register(request: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/register`, request).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.handleAuthResponse(response.data);
        }
      }),
      catchError(error => throwError(() => error))
    );
  }

  refreshToken(): Observable<ApiResponse<AuthResponse>> {
    const request: RefreshTokenRequest = {
      accessToken: this.tokenService.getToken() || '',
      refreshToken: this.tokenService.getRefreshToken() || ''
    };

    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/refresh-token`, request).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.handleAuthResponse(response.data);
        }
      }),
      catchError(error => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/forgot-password`, request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/reset-password`, request);
  }

  logout(): void {
    const refreshToken = this.tokenService.getRefreshToken();
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/revoke`, { refreshToken }).subscribe();
    }
    
    this.tokenService.clearTokens();
    this._isAuthenticated.set(false);
    this._currentUser.set(null);
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  private handleAuthResponse(response: AuthResponse): void {
    this.tokenService.setTokens(response.accessToken, response.refreshToken);
    this._isAuthenticated.set(true);
    this.loadUserFromToken();
  }
}
