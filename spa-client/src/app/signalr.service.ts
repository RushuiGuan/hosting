import { Injectable, signal } from '@angular/core';
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';

// Serverless mode, receive only: the client posts to /api/signalr/negotiate, which returns { url, accessToken },
// then connects directly to Azure SignalR Service with that token.  The token is bound to this application's hub,
// so the client receives only what is sent to that hub.
@Injectable({ providedIn: 'root' })
export class SignalRService {
  readonly messages = signal<string[]>([]);
  readonly state = signal<HubConnectionState>(HubConnectionState.Disconnected);

  // absolute path because the spa is hosted under /app/
  private readonly connection = new HubConnectionBuilder()
    .withUrl('/api/signalr')
    .withAutomaticReconnect()
    .build();

  constructor() {
    this.connection.on('message', (user: string | null, message: string) =>
      this.messages.update((list) => [...list, `${user ?? '<anon>'}: ${message}`]),
    );
    this.connection.onreconnecting(() => this.state.set(this.connection.state));
    this.connection.onreconnected(() => this.state.set(this.connection.state));
    this.connection.onclose(() => this.state.set(this.connection.state));
  }

  async start(): Promise<void> {
    this.state.set(HubConnectionState.Connecting);
    try {
      await this.connection.start();
    } finally {
      this.state.set(this.connection.state);
    }
  }
}
