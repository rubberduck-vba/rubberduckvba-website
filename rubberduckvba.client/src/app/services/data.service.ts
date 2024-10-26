import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map, timeout, catchError, throwError, Observable } from "rxjs";
import { environment } from "../../environments/environment";

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
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private http: HttpClient) { }

  public signin(): Observable<any> {
    const headers = new HttpHeaders()
      .append('accept', 'application/json')
      .append('Content-Type', 'application/json; charset=utf-8');

    return this.http.post(`${environment.apiBaseUrl}auth/signin`, undefined, { headers });
  }

  public signout(): Observable<any> {
    const headers = new HttpHeaders()
      .append('accept', 'application/json')
      .append('Content-Type', 'application/json; charset=utf-8');

    return this.http.post(`${environment.apiBaseUrl}auth/signout`, undefined, { headers });
  }
}
