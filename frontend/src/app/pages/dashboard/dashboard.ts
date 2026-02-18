import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../services/chat.service';
import { jwtDecode } from 'jwt-decode';
import { ViewChildren, QueryList, ElementRef } from '@angular/core';

import { BaseChartDirective } from 'ng2-charts';
import {
  Chart,
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Title,
  Tooltip,
  Legend,
  ChartConfiguration,
  ChartType
} from 'chart.js';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatToolbarModule,
    FormsModule,
    BaseChartDirective
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit {

  role: string = '';
  loggedInUser: string = '';
  pendingUsers: any[] = [];
  successMessage: string = '';
  displayedColumns: string[] = ['name', 'email', 'role', 'action'];
  message: string = '';
  messages: any[] = [];
  selectedRole: string = 'all';

  // 📈 Chart Config
  public lineChartType: 'line' = 'line';

  public lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Daily Approved Users',
        borderColor: '#5e35b1',
        backgroundColor: 'rgba(94,53,177,0.15)',
        fill: true,
        tension: 0.4,
        pointRadius: 5,
        pointHoverRadius: 7,
        pointBackgroundColor: '#5e35b1',
        borderWidth: 3
      }
    ]
  };


 public lineChartOptions: ChartConfiguration<'line'>['options'] = {
  responsive: true,
  animation: {
    duration: 1200,
    easing: 'easeOutQuart'
  },
  plugins: {
    legend: {
      position: 'top'
    },
    tooltip: {
      backgroundColor: '#2c2c2c',
      titleColor: '#fff',
      bodyColor: '#fff',
      padding: 12,
      cornerRadius: 8
    }
  },
  scales: {
    x: {
      grid: {
        display: false
      }
    },
    y: {
      beginAtZero: true,
      grid: {
        color: 'rgba(0,0,0,0.05)'
      }
    }
  }
};


  constructor(
    private router: Router,
    private http: HttpClient,
    private cd: ChangeDetectorRef,
    private chatService: ChatService,
  ) {
    // 🔥 Required for Chart.js v4
    Chart.register(
      LineController,
      LineElement,
      PointElement,
      LinearScale,
      CategoryScale,
      Title,
      Tooltip,
      Legend
    );
  }

  ngOnInit(): void {

    this.role = sessionStorage.getItem('role')?.toLowerCase() || '';
    const token = sessionStorage.getItem('token');

    if (token) {
      const decoded: any = jwtDecode(token);
      this.loggedInUser =
        decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
        decoded.unique_name ||
        decoded.email ||
        decoded.sub ||
        decoded.name ||
        '';
    }

    if (this.role === 'admin') {
      this.loadPendingUsers();
      this.loadUserGrowth();
    }

    this.chatService.startConnection();
    this.loadHistory();

    this.chatService.onMessageReceived((sender, message) => {
      this.messages.push({
        senderId: sender,
        message: message,
        timestamp: new Date()
      });
      this.cd.detectChanges();
      this.scrollToBottom();
    });
  }

  @ViewChildren('chatContainer') chatContainers!: QueryList<ElementRef>;

  scrollToBottom() {
    setTimeout(() => {
      if (this.chatContainers?.length > 0) {
        const container = this.chatContainers.last.nativeElement;
        container.scrollTop = container.scrollHeight;
      }
    }, 50);
  }

  loadPendingUsers() {
    const token = sessionStorage.getItem('token');
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.get<any[]>('http://localhost:5264/api/admin/pending', { headers })
      .subscribe(res => {
        this.pendingUsers = res;
        this.cd.detectChanges();
      });
  }

  approveUser(id: string) {
    const token = sessionStorage.getItem('token');
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.put(
      `http://localhost:5264/api/admin/approve/${id}`,
      {},
      { headers }
    ).subscribe(() => {
      this.successMessage = 'User approved successfully!';
      this.loadPendingUsers();
      this.loadUserGrowth(); // 🔥 Auto update chart
    });
  }

  loadUserGrowth() {
    const token = sessionStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.get<any[]>('http://localhost:5264/api/analytics/user-growth', { headers })
      .subscribe(res => {

        const labels = res.map(x => x.date);
        const data = res.map(x => x.count);

        this.lineChartData = {
          ...this.lineChartData,
          labels: labels,
          datasets: [
            {
              ...this.lineChartData.datasets[0],
              data: data
            }
          ]
        };

        this.cd.detectChanges();
      });
  }




  sendMessage() {
    if (!this.message.trim()) return;
    this.chatService.sendMessage(this.message, this.selectedRole);
    this.message = '';
  }

  loadHistory() {
    this.chatService.getHistory().then(data => {
      this.messages = data;
      this.cd.detectChanges();
      this.scrollToBottom();
    });
  }

  logout() {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('role');
    this.router.navigate(['/']);
  }
}
