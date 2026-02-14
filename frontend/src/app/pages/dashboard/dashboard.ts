import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { ChatService } from '../../services/chat.service'; 
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatToolbarModule,
    FormsModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit {

  role: string = '';
  pendingUsers: any[] = [];
  successMessage: string = '';
  displayedColumns: string[] = ['name', 'email', 'role', 'action'];
  message: string = '';
  messages: string[] = [];


  constructor(
    private router: Router,
    private http: HttpClient,
    private cd: ChangeDetectorRef,
    private chatService: ChatService,
  ) {}

  ngOnInit(): void {

    this.role = localStorage.getItem('role')?.toLowerCase() || '';
    console.log('Logged in role:', this.role);

    if (this.role === 'admin') {
      console.log('Admin detected. Loading pending users...');
      this.loadPendingUsers();
    }

    // Start SignalR
    this.chatService.startConnection();

    this.chatService.onMessageReceived((sender, message) => {
      this.messages.push(`${sender}: ${message}`);
      this.cd.detectChanges();
    });

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

  sendMessage() {
    if (!this.message.trim()) return;

    this.chatService.sendMessage('User1', this.message);
    this.message = '';
  }


  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    this.router.navigate(['/']);
  }
}
