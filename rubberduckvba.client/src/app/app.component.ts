import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
})
export class AppComponent {
  title = 'rubberduckvba.com';
  currentYear = new Date().getFullYear();
}
