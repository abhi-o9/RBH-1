import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ChatService {

  private hubConnection!: signalR.HubConnection;

  public startConnection(): void {

    const role = sessionStorage.getItem('role');
    const token = sessionStorage.getItem('token');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`http://localhost:5264/chathub?role=${role}`, {
        accessTokenFactory: () => token!
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Connected'))
      .catch(err => console.log('Error while starting connection: ' + err));
  }



  public sendMessage(message: string, receiverRole: string) {

    const payload = {
      Message: message,
      ReceiverRole: receiverRole
    };

    return fetch('http://localhost:5264/api/message', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${sessionStorage.getItem('token')}`
      },
      body: JSON.stringify(payload)
    });
  }




  public getHistory() {
    return fetch('http://localhost:5264/api/message/history', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${sessionStorage.getItem('token')}`
      }
    }).then(res => res.json());
  }


  public onMessageReceived(callback: (sender: string, receiverRole: string, message: string) => void) {
    this.hubConnection.on('ReceiveMessage', (sender, receiverRole, message) => {
      callback(sender, receiverRole, message);
    });
  }
}
