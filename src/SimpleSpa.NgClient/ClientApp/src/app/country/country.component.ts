import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-country',
  templateUrl: './country.component.html'
})
export class CountryComponent {
  public countries: Country[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<Country[]>(baseUrl + 'country').subscribe(result => {
      this.countries = result;
    }, error => console.error(error));
  }
}

interface Country {
  name: string;
}