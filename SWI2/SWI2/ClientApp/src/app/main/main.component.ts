import { Component, HostListener } from '@angular/core';
import { AuthenticationService } from '../shared/services/authentication.service';

@Component({
    selector: 'swi-main',
    templateUrl: './main.component.html',
    styleUrls: ['./main.component.css']
})
export class MainComponent {
  screenWidth: number;

  constructor() {
    this.screenWidth = window.innerWidth;

  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.screenWidth = window.innerWidth;
  }
}
