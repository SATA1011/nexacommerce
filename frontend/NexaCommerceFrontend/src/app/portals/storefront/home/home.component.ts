import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  categories = [
    { name: 'Electronics & Gadgets', slug: 'electronics', icon: '💻', count: 420, bgClass: 'bg-blue-100 text-blue-600' },
    { name: 'Home & Living', slug: 'home-living', icon: '🏡', count: 290, bgClass: 'bg-amber-100 text-amber-600' },
    { name: 'Apparel & Fashion', slug: 'fashion', icon: '👕', count: 680, bgClass: 'bg-pink-100 text-pink-600' },
    { name: 'Health & Beauty', slug: 'health-beauty', icon: '✨', count: 180, bgClass: 'bg-emerald-100 text-emerald-600' }
  ];
}
