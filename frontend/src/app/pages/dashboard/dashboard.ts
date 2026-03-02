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
import { PieController, ArcElement } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import {
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Title,
  Tooltip,
  Legend,

} from 'chart.js';
import { OnDestroy } from '@angular/core';
import { Chart, type ChartConfiguration } from 'chart.js';
import { BarController, BarElement } from 'chart.js';

Chart.register(BarController, BarElement);


// 📊 Role Distribution Chart


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
    BaseChartDirective,

  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit, OnDestroy {

  role: string = '';
  loggedInUser: string = '';
  pendingUsers: any[] = [];
  successMessage: string = '';
  displayedColumns: string[] = ['name', 'email', 'role', 'action'];
  message: string = '';
  messages: any[] = [];
  selectedRole: string = 'all';
  public selectedTabIndex = 0;
  private tabTimeout: any;
  private autoRefreshInterval: any;
  private readonly TAB_TIMEOUT = 10000; // 20 seconds

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
  public pieChartType: 'pie' = 'pie';

  public pieChartData: ChartConfiguration<'pie'>['data'] = {
    labels: [],
    datasets: [
      {
        data: [],
        backgroundColor: [
          '#5e35b1',
          '#42a5f5'
        ]
      }
    ]
  };

  public pieChartOptions: ChartConfiguration<'pie'>['options'] = {
    responsive: true,
    plugins: {
      legend: {
        position: 'bottom'
      }
    }
  };
  public userLineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Messages Sent',
        borderColor: '#ff7043',
        backgroundColor: 'rgba(255,112,67,0.15)',
        fill: true,
        tension: 0.4
      }
    ]
  };

  public barChartType: 'bar' = 'bar';

  public userBarChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Sent',
        backgroundColor: '#5e35b1'
      },
      {
        data: [],
        label: 'Received',
        backgroundColor: '#42a5f5'
      }
    ]
  };

  public barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    scales: {
      y: {
        beginAtZero: true
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
      Legend,
      PieController,
      ArcElement
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
      this.loadUsersByRole();   // 👈 ADD THIS

    }
    if (this.role === 'user') {
      this.loadMyMessagesPerDay();
      this.loadMyActivityTrend();
    }


    this.chatService.startConnection();
    this.loadHistory();

    this.chatService.onMessageReceived((sender, receiverRole, message) => {
      this.messages.push({
        senderId: sender,
        receiverRole: receiverRole,
        message: message,
        timestamp: new Date()
      });
      this.cd.detectChanges();
      this.scrollToBottom();
    });
    // 🔄 Auto refresh analytics every 10 seconds
    this.autoRefreshInterval = setInterval(() => {

      if (this.role === 'admin') {
        this.loadPendingUsers();
        this.loadUserGrowth();
        this.loadUsersByRole();
      }

      if (this.role === 'user') {
        this.loadMyMessagesPerDay();
        this.loadMyActivityTrend();
      }

    }, 10000); // 5 seconds

  }

  @ViewChildren('chatContainer') chatContainers!: QueryList<ElementRef>;




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

  loadUsersByRole() {
    const token = sessionStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.get<any[]>('http://localhost:5264/api/analytics/users-by-role', { headers })
      .subscribe(res => {

        this.pieChartData = {
          labels: res.map(x => x.role),
          datasets: [
            {
              data: res.map(x => x.count),
              backgroundColor: ['#5e35b1', '#42a5f5']
            }
          ]
        };

        this.cd.detectChanges();
      });
  }

  loadMyMessagesPerDay() {
    const token = sessionStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.get<any[]>('http://localhost:5264/api/analytics/my-messages-per-day', { headers })
      .subscribe(res => {

        this.userLineChartData = {
          ...this.userLineChartData,
          labels: res.map(x => x.date),
          datasets: [
            {
              ...this.userLineChartData.datasets[0],
              data: res.map(x => x.count)
            }
          ]
        };

        this.cd.detectChanges();
      });
  }

  loadMyActivityTrend() {
    const token = sessionStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.get<any[]>('http://localhost:5264/api/analytics/my-activity-trend', { headers })
      .subscribe(res => {

        this.userBarChartData = {
          labels: res.map(x => x.date),
          datasets: [
            {
              label: 'Sent',
              data: res.map(x => x.sent),
              backgroundColor: '#5e35b1'
            },
            {
              label: 'Received',
              data: res.map(x => x.received),
              backgroundColor: '#42a5f5'
            }
          ]
        };

        this.cd.detectChanges();
      });
  }
  onTabChange(event: any) {

    // Clear previous timer
    if (this.tabTimeout) {
      clearTimeout(this.tabTimeout);
    }

    const selectedLabel = event.tab.textLabel;

    if (selectedLabel === 'Messages') {
      setTimeout(() => {
        this.scrollToBottom();
      }, 200);
    }

    // Admin → Protect Analytics tab
    if (this.role === 'admin' && selectedLabel === 'Analytics') {
      this.startTabTimer();
    }

    // User → Protect Messages tab (assuming this is Tab F)
    if (this.role === 'user' && selectedLabel === 'My Analytics') {
      this.startTabTimer();
    }
  }

  startTabTimer() {

    this.tabTimeout = setTimeout(() => {

      alert("Session expired for this tab.");

      // Switch to next safe tab
      if (this.role === 'admin') {
        this.selectedTabIndex = 1; // Admin: go to Messages
      }

      if (this.role === 'user') {
        this.selectedTabIndex = 1; // User: go to Messages
      }

    }, this.TAB_TIMEOUT);
  }



  sendMessage() {
    if (!this.message.trim()) return;
    this.chatService.sendMessage(this.message, this.selectedRole);
    this.message = '';
    this.scrollToBottom();
  }

  loadHistory() {
    this.chatService.getHistory().then(data => {
      this.messages = data;
      this.cd.detectChanges();
      this.scrollToBottom();
    });
  }

  scrollToBottom() {
    setTimeout(() => {
      if (!this.chatContainers || this.chatContainers.length === 0) return;

      const container = this.chatContainers.last.nativeElement;

      container.scrollTop = container.scrollHeight;
    }, 100); // increased delay slightly
  }

  logout() {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('role');
    this.router.navigate(['/']);
  }

  ngOnDestroy() {
    if (this.tabTimeout) {
      clearTimeout(this.tabTimeout);
    }

    if (this.autoRefreshInterval) {
      clearInterval(this.autoRefreshInterval);
    }
  }


}



