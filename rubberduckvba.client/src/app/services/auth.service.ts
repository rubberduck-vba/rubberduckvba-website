import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "../../environments/environment";
import { UserViewModel } from "../model/feature.model";
import { AuthViewModel, DataService } from "./data.service";


@Injectable({ providedIn: 'root' })
export class AuthService {
  private timeout: number = 10000;
  constructor(private data: DataService) { }

  private sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  private writeStorage(key: string, value: string): void {
    sessionStorage.setItem(key, value);
    while (sessionStorage.getItem(key) != value) {
      this.sleep(1000);
    }
  }

  public getUser(): Observable<UserViewModel> {
    const url = `${environment.apiBaseUrl}auth`;
    return this.data.getAsync<UserViewModel>(url);
  }

  public signin(): void {
    const vm = AuthViewModel.withRandomState();
    this.writeStorage('xsrf:state', vm.state);

    const url = `${environment.apiBaseUrl}auth/signin`;
    this.data.postAsync<AuthViewModel, string>(url, vm)
      .subscribe(redirectUrl => location.href = redirectUrl);
  }

  public signout(): void {
    sessionStorage.clear();
  }

  public onGithubCallback(): void {
    const urlParams = new URLSearchParams(location.search);
    const code: string = urlParams.get('code')!;
    const state: string = urlParams.get('state')!;
      
    if (state === sessionStorage.getItem('xsrf:state')) {
      try {
        const vm: AuthViewModel = { state, code };
        const url = `${environment.apiBaseUrl}auth/github`;

        this.data.postAsync<AuthViewModel, AuthViewModel>(url, vm)
          .subscribe(result => {
            this.writeStorage('github:access_token', result.token!);
            location.href = '/';
          });
      }
      catch (error) {
        console.log(error);
        location.href = '/';
      }
    }
    else {
      console.log('xsrf:state mismatched!');
      location.href = '/';
    }
  }
}
