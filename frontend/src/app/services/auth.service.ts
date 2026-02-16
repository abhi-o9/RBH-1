import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService {


  constructor(private http: HttpClient, private router: Router) {}

  login(data: any) {
    return this.http.post<any>(`${this.baseUrl}/login`, data);
  }

  saveToken(token: string) {
    sessionStorage.setItem('token', token);
  }

  getToken(): string | null {
    return sessionStorage.getItem('token');
  }


  isLoggedIn(): boolean {
    return !!this.getToken();
  }

 
  getUserRole(): string | null {
    const token = this.getToken();
    if (!token) return null;

    const decoded: any = jwtDecode(token);
    return decoded.role; // must match backend claim name
  }

  logout() {
    sessionStorage.clear();
    this.router.navigate(['/']);
  }

  private baseUrl = 'http://localhost:5264/api/auth';

  register(data: any) {
    return this.http.post(
      `${this.baseUrl}/register`,
      data,
      { responseType: 'text' }   // 👈 ADD THIS
    );
  }



}
