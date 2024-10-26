import { HTTP_INTERCEPTORS, HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";


//@Injectable()
//export class HttpRequestInterceptor implements HttpInterceptor {
//    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
//      req = req.clone({
//        withCredentials: true,
//        headers: req.headers.append('Content-Type', 'application/json')
//      });
//      ;
//      return next.handle(req);
//    }
//}

//export const httpInterceptorProviders = [
//  { provide: HTTP_INTERCEPTORS, useClass: HttpRequestInterceptor, multi: true },
//];
