import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DOCUMENT } from '@angular/common';
import { strict } from 'assert';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;

  constructor(
    private http: HttpClient,
    @Inject(DOCUMENT) private document: Document,
    @Inject('BASE_URL') private baseUrl: string) {
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  onLogoutClick(): void {
    this.http.get(this.baseUrl + 'auth/Logout', { responseType: 'text' }).subscribe(result => {
      const path = result
      this.document.location.href = path;
    }, error => console.error(error));
  }
}
