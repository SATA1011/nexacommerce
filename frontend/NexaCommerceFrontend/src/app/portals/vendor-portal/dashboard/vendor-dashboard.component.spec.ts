import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { VendorDashboardComponent } from './vendor-dashboard.component';

describe('VendorDashboardComponent', () => {
  let component: VendorDashboardComponent;
  let fixture: ComponentFixture<VendorDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VendorDashboardComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VendorDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
