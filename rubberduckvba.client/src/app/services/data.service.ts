import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable, Query } from "@angular/core";
import { map, timeout, catchError, throwError, Observable } from "rxjs";
import { ApiClientService } from "./api-client.service";

@Injectable()
export class DataService {
  private timeout: number = 30000;

  constructor(private http: HttpClient) {

  }

  public getAsync<TResult>(url: string): Observable<TResult> {
    const headers = new HttpHeaders()
      .append('accept', 'application/json');

    return this.http.get(url, { headers })
      .pipe(
        map(result => <TResult>result),
        timeout(this.timeout),
        catchError((err: Response) => {
          console.log(err);
          return throwError(() => err.text);
        })
      );
  }

  public postAsync<TContent, TResult>(url: string, content?: TContent): Observable<TResult> {
    const headers = new HttpHeaders()
      .append('accept', 'application/json')
      .append('Content-Type', 'application/json; charset=utf-8');

    return (content
      ? this.http.post(url, content, { headers })
      : this.http.post(url, { headers }))
      .pipe(
        map(result => <TResult>result),
        timeout(this.timeout),
        catchError((err: Response) => throwError(() => err.text))
      );
  }

  
  public getWithAccessTokenAsync<TResult>(url: string): Observable<TResult> {
    const headers = new HttpHeaders()
      .append('accept', 'application/json');

    const token = sessionStorage.getItem('github:access_token');
    if (token) {
      headers.append('X-ACCESS-TOKEN', token);
    }

    return this.http.get(url, { headers })
      .pipe(
        map(result => <TResult>result),
        timeout(this.timeout),
        catchError((err: Response) => {
          console.log(err);
          return throwError(() => err.text);
        })
      );
  }

  public postWithAccessTokenAsync<TContent, TResult>(url: string, content?: TContent): Observable<TResult> {
    const headers = new HttpHeaders()
      .append('accept', 'application/json')
      .append('Content-Type', 'application/json; charset=utf-8');

    const token = sessionStorage.getItem('github:access_token');
    if (token) {
      headers.append('X-ACCESS-TOKEN', token);
    }

    return (content
      ? this.http.post(url, content, { headers })
      : this.http.post(url, { headers }))
      .pipe(
        map(result => <TResult>result),
        timeout(this.timeout),
        catchError((err: Response) => throwError(() => err.text))
      );
  }
}

export class AuthViewModel {
  state: string;
  code?: string;
  token?: string;

  constructor(state: string, code?: string, token?: string) {
    this.state = state;
    this.code = code;
    this.token = token;
  }

  public static withRandomState() {
    const state = crypto.randomUUID();
    return new AuthViewModel(state);
  }
}
