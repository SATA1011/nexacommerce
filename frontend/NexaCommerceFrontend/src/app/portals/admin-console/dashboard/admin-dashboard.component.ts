import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent {
  readonly authService = inject(AuthService);

  pendingApplications = [
    { id: '1', storeName: 'Nordic Sound Labs', ownerName: 'Lukas Meyer', email: 'lukas@nordicsound.io', taxNumber: 'EU-9283741' },
    { id: '2', storeName: 'Kinetics Apparel', ownerName: 'Elena Rostova', email: 'elena@kinetics.style', taxNumber: 'US-8291038' },
    { id: '3', storeName: 'Velocita Mechanicals', ownerName: 'Marco Bellini', email: 'marco@velocita.it', taxNumber: 'IT-3928104' }
  ];
}
