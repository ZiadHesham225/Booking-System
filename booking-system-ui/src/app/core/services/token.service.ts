import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';

  getToken(): string | null {
    return sessionStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return sessionStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  setTokens(accessToken: string, refreshToken: string): void {
    sessionStorage.setItem(this.TOKEN_KEY, accessToken);
    sessionStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  clearTokens(): void {
    sessionStorage.removeItem(this.TOKEN_KEY);
    sessionStorage.removeItem(this.REFRESH_TOKEN_KEY);
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp * 1000;
      return Date.now() >= expiry;
    } catch {
      return true;
    }
  }

  getTokenPayload(): any {
    const token = this.getToken();
    if (!token) return null;

    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch {
      return null;
    }
  }

  getUserRoles(): string[] {
    const payload = this.getTokenPayload();
    if (!payload) return [];

    const roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] 
      || payload['role'] 
      || [];
    
    return Array.isArray(roles) ? roles : [roles];
  }

  hasRole(role: string): boolean {
    return this.getUserRoles().includes(role);
  }

  isAdmin(): boolean {
    return this.hasRole('Admin');
  }
}
