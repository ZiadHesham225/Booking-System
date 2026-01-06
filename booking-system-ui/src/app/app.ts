import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './shared/components/navbar/navbar';
import { Footer } from './shared/components/footer/footer';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Navbar, Footer],
  template: `
    <div class="min-h-screen flex flex-col">
      <app-navbar />
      <main class="flex-1">
        <router-outlet />
      </main>
      <app-footer />
    </div>
  `
})
export class App {}
