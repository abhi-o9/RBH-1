import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit {

  role: string = '';
  pendingUsers: any[] = [];
  successMessage: string = '';

  constructor(
    private router: Router,
    private http: HttpClient,
    private cd: ChangeDetectorRef
  ) {}

  ngOnInit(): void {

    this.role = localStorage.getItem('role')?.toLowerCase() || '';
    console.log('Logged in role:', this.role);

    if (this.role === 'admin') {
      console.log('Admin detected. Loading pending users...');
      this.loadPendingUsers();
    }
  }

  loadPendingUsers() {

    const token = localStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.get<any[]>('http://localhost:5264/api/admin/pending', { headers })
      .subscribe({
        next: (res) => {
          console.log('Pending users:', res);
          this.pendingUsers = res;
          this.cd.detectChanges(); // Force UI update
        },
        error: (err) => console.error('Error loading pending users:', err)
      });
  }

  approveUser(id: string) {

    const token = localStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.put(
      `http://localhost:5264/api/admin/approve/${id}`,
      {},
      {
        headers,
        responseType: 'text' as 'json'   // 👈 THIS LINE FIXES IT
      }
    )
      .subscribe({
        next: () => {
          setTimeout(() => {
            this.successMessage = 'User approved successfully!';
          }, 3000);
          this.loadPendingUsers();

        },
        error: (err) => console.error('Approve error:', err)
      });

  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    this.router.navigate(['/']);
  }
}
