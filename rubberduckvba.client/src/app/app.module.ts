import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

import { DataService } from './services/data.service';
import { AdminApiClientService, ApiClientService } from './services/api-client.service';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule, UrlSerializer } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { TagDownloadComponent } from './components/tag-download/tag-download.component';
import { DownloadItemComponent } from './components/download-item/download-item.component';
import { FeatureBoxComponent } from './components/feature-box/feature-box.component';
import { FeatureInfoComponent } from './components/feature-info/feature-info.component';
import { FeatureItemBoxComponent } from './components/feature-item-box/feature-item-box.component';
import { ExampleBoxComponent } from './components/example-box/example-box.component';
import { FeatureItemExampleComponent } from './components/quickfix-example.modal/quickfix-example.modal.component';
import { LoadingContentComponent } from './components/loading-content/loading-content.component';
import { InspectionItemBoxComponent } from './components/feature-item-box/inspection-item-box/inspection-item-box.component';
import { AnnotationItemBoxComponent } from './components/feature-item-box/annotation-item-box/annotation-item-box.component';
import { BlogLinkBoxComponent } from './components/blog-link-box/blog-link-box.component';
import { QuickFixItemBoxComponent } from './components/feature-item-box/quickfix-item-box/quickfix-item-box.component';

import { HomeComponent } from './routes/home/home.component';
import { AboutComponent } from './routes/about/about.component';
import { FeaturesComponent } from './routes/features/features.component';
import { FeatureComponent } from './routes/feature/feature.component';
import { InspectionComponent } from './routes/inspection/inspection.component';
import { AnnotationComponent } from './routes/annotation/annotation.component';
import { QuickFixComponent } from './routes/quickfixes/quickfix.component';

import { DefaultUrlSerializer, UrlTree } from '@angular/router';
import { AuthMenuComponent } from './components/auth-menu/auth-menu.component';
import { AuthComponent } from './routes/auth/auth.component';

/**
 * https://stackoverflow.com/a/39560520
 */
export class LowerCaseUrlSerializer extends DefaultUrlSerializer {
  override parse(url: string): UrlTree {
    // Optional Step: Do some stuff with the url if needed.

    // If you lower it in the optional step 
    // you don't need to use "toLowerCase" 
    // when you pass it down to the next function
    return super.parse(url.toLowerCase());
  }
}

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    AuthComponent,
    FeaturesComponent,
    FeatureComponent,
    TagDownloadComponent,
    NavMenuComponent,
    DownloadItemComponent,
    FeatureBoxComponent,
    FeatureInfoComponent,
    FeatureItemBoxComponent,
    ExampleBoxComponent,
    FeatureItemExampleComponent,
    LoadingContentComponent,
    AnnotationItemBoxComponent,
    InspectionItemBoxComponent,
    QuickFixItemBoxComponent,
    BlogLinkBoxComponent,
    InspectionComponent,
    AnnotationComponent,
    QuickFixComponent,
    AboutComponent,
    AuthMenuComponent
  ],
  bootstrap: [AppComponent],
  imports: [
    BrowserModule,
    CommonModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'features', component: FeaturesComponent },
      { path: 'features/:name', component: FeatureComponent },
      { path: 'inspections/:name', component: InspectionComponent },
      { path: 'annotations/:name', component: AnnotationComponent },
      { path: 'quickfixes/:name', component: QuickFixComponent },
      { path: 'about', component: AboutComponent },
      { path: 'auth/github', component: AuthComponent },
      // legacy routes:
      { path: 'inspections/details/:name', redirectTo: 'inspections/:name' },
    ]),
    FontAwesomeModule,
    NgbModule
  ],
  providers: [
    DataService,
    ApiClientService,
    AdminApiClientService,
    provideHttpClient(withInterceptorsFromDi()),
    {
      provide: UrlSerializer,
      useClass: LowerCaseUrlSerializer
    }
  ]
})
export class AppModule { }
