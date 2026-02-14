import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ChatService {

  private hubConnection!: signalR.HubConnection;

  public startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5264/chathub') // change port if needed
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Connected'))
      .catch(err => console.log('Error while starting connection: ' + err));
  }

  public sendMessage(sender: string, message: string) {
    this.hubConnection.invoke('SendMessage', sender, message);
  }

  public onMessageReceived(callback: (sender: string, message: string) => void) {
    this.hubConnection.on('ReceiveMessage', (sender, message) => {
      callback(sender, message);
    });
  }
}
