import { Component, OnInit } from "@angular/core";
import { AuthService } from "src/app/services/auth.service";

@Component({
    selector: 'app-auth',
    templateUrl: './auth.component.html',
    standalone: false
})
export class AuthComponent implements OnInit {

  constructor(private service: AuthService) {
  }

  ngOnInit(): void {
    this.service.onGithubCallback();
  }
}
