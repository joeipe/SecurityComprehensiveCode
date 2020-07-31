import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html'
})
export class AuthComponent {
  public token: Token;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<Token>(baseUrl + 'auth/GetToken').subscribe(result => {
      this.token = result;
    }, error => console.error(error));
  }
}

interface Token {
  idToken: string;
  accessToken: string;
  refreshToken: string;
  firstName: string;
  lastName: string;
  address: string;
  roles: string;
}