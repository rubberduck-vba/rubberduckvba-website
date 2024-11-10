import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

import { DataService } from './services/data.service';
import { ApiClientService } from './services/api-client.service';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { HomeComponent } from './routes/home/home.component';
import { TagDownloadComponent } from './components/tag-download/tag-download.component';
import { DownloadItemComponent } from './components/download-item/download-item.component';
import { FeatureBoxComponent } from './components/feature-box/feature-box.component';
import { FeaturesComponent } from './routes/features/features.component';
import { FeatureInfoComponent } from './components/feature-info/feature-info.component';
import { FeatureItemBoxComponent } from './components/feature-item-box/feature-item-box.component';
import { FeatureComponent } from './routes/feature/feature.component';
import { ExampleBoxComponent } from './components/example-box/example-box.component';
import { FeatureItemExampleComponent } from './components/quickfix-example.modal/quickfix-example.modal.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { LoadingContentComponent } from './components/loading-content/loading-content.component';
//import { httpInterceptorProviders } from './services/HttpRequestInterceptor';


@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
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
    LoadingContentComponent
  ],
  bootstrap: [AppComponent],
  imports: [
    BrowserModule,
    CommonModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'features', component: FeaturesComponent, pathMatch: 'full' },
      { path: 'features/:name', component: FeatureComponent, pathMatch: 'full' }
    ]),
    FontAwesomeModule,
    NgbModule
  ],
  providers: [
    DataService,
    ApiClientService,
    provideHttpClient(withInterceptorsFromDi()),
    //httpInterceptorProviders
  ]
})
export class AppModule { }
