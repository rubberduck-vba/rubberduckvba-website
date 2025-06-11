import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule, UrlSerializer } from '@angular/router';

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

import { DataService } from './services/data.service';
import { ApiClientService } from './services/api-client.service';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { LoadingContentComponent } from './components/loading-content/loading-content.component';
import { TagDownloadComponent } from './components/tag-download/tag-download.component';
import { DownloadItemComponent } from './components/download-item/download-item.component';
import { FeatureBoxComponent } from './components/feature-box/feature-box.component';
import { FeatureInfoComponent } from './components/feature-info/feature-info.component';
import { FeatureItemBoxComponent } from './components/feature-item-box/feature-item-box.component';
import { ExampleBoxComponent } from './components/example-box/example-box.component';
import { FeatureItemExampleComponent } from './components/quickfix-example.modal/quickfix-example.modal.component';
import { InspectionItemBoxComponent } from './components/feature-item-box/inspection-item-box/inspection-item-box.component';
import { AnnotationItemBoxComponent } from './components/feature-item-box/annotation-item-box/annotation-item-box.component';
import { BlogLinkBoxComponent } from './components/blog-link-box/blog-link-box.component';
import { QuickFixItemBoxComponent } from './components/feature-item-box/quickfix-item-box/quickfix-item-box.component';

import { EditFeatureComponent } from './components/edit-feature/edit-feature.component';

import { HomeComponent } from './routes/home/home.component';
import { AboutComponent } from './routes/about/about.component';
import { FeaturesComponent } from './routes/features/features.component';
import { FeatureComponent } from './routes/feature/feature.component';
import { InspectionComponent } from './routes/inspection/inspection.component';
import { AnnotationComponent } from './routes/annotation/annotation.component';
import { QuickFixComponent } from './routes/quickfixes/quickfix.component';
import { IndenterComponent } from './routes/indenter/indenter.component';

import { DefaultUrlSerializer, UrlTree } from '@angular/router';
import { AuthComponent } from './routes/auth/auth.component';
import { AuthMenuComponent } from './components/auth-menu/auth-menu.component';

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
    AuthMenuComponent,
    IndenterComponent,
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
    EditFeatureComponent
  ],
  bootstrap: [AppComponent],
  imports: [
    CommonModule,
    BrowserModule,
    FormsModule,
    RouterModule.forRoot([
      // legacy routes:
      { path: 'inspections/details/:name', redirectTo: 'inspections/:name' },
      // actual routes:
      { path: 'auth/github', component: AuthComponent },
      { path: 'features', component: FeaturesComponent },
      { path: 'features/:name', component: FeatureComponent },
      { path: 'inspections/:name', component: InspectionComponent },
      { path: 'annotations/:name', component: AnnotationComponent },
      { path: 'quickfixes/:name', component: QuickFixComponent },
      { path: 'about', component: AboutComponent },
      { path: 'indenter', component: IndenterComponent },
      { path: '', component: HomeComponent, pathMatch: 'full' },
    ]),
    FontAwesomeModule,
    NgbModule
  ],
  providers: [
    DataService,
    ApiClientService,
    provideHttpClient(withInterceptorsFromDi()),
    {
      provide: UrlSerializer,
      useClass: LowerCaseUrlSerializer
    }
  ]
})

export class AppModule { }
