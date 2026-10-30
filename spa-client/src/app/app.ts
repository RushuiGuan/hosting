import { Component, inject, signal } from '@angular/core';
import { SignalRService } from './signalr.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly signalR = inject(SignalRService);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.signalR.start().catch((err) => this.error.set(String(err)));
  }
}
