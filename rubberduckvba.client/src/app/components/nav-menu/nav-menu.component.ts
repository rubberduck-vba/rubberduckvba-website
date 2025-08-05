import { Component, OnInit } from '@angular/core';
import { ApiClientService } from '../../services/api-client.service';
import { DownloadInfo } from '../../model/downloads.model';
import { fab } from '@fortawesome/free-brands-svg-icons';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { BehaviorSubject } from 'rxjs';
import { filter, map } from 'rxjs/operators';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { Router, NavigationEnd } from '@angular/router';
import { AngularDeviceInformationService } from 'angular-device-information';

@Component({
    selector: 'app-nav-menu',
    templateUrl: './nav-menu.component.html',
    styleUrls: ['./nav-menu.component.css'],
    standalone: false
})
export class NavMenuComponent implements OnInit {

  public isHomePage: boolean = true;
  public isFeaturesPage: boolean = false;
  public isAboutPage: boolean = false;
  public isIndenterPage: boolean = false;

  public isExpanded: boolean = false;
  public canDownload: boolean = false;

  constructor(private api: ApiClientService, private fa: FaIconLibrary, private platform: AngularDeviceInformationService, private router: Router) {
    fa.addIconPacks(fas);
    fa.addIconPacks(fab);
    this.canDownload = platform.isDesktop() && platform.getDeviceInfo().os == 'Windows';

    this.router.events
      .pipe(
        filter(e => e instanceof NavigationEnd),
        map(e => <NavigationEnd>e)
      )
      .subscribe(navEnd => {
        const url = navEnd.urlAfterRedirects;
        this.isFeaturesPage = url.startsWith('/features');
        this.isAboutPage = url.startsWith('/about');
        this.isIndenterPage = url.startsWith('/indenter');
        this.isHomePage = !(this.isFeaturesPage || this.isAboutPage || this.isIndenterPage);
      });
  }

  ngOnInit(): void {
    this.api.getAvailableDownloads().forEach(result => {
      this.tagDownloads = result.filter(e => e.kind == 'tag' || e.kind == 'pre');
      this.tagDownloads.forEach(e => e.filename = <string>e.downloadUrl.split('/').pop())
      this.pdfDownloads = result.filter(e => e.kind == 'pdf');
    });
  }

  private readonly _tagDownloads: BehaviorSubject<DownloadInfo[]> = new BehaviorSubject<DownloadInfo[]>([]);
  public set tagDownloads(value: DownloadInfo[]) {
    this._tagDownloads.next(value);
  }
  public get tagDownloads(): DownloadInfo[] {
    return this._tagDownloads.getValue();
  }

  private readonly _pdfDownloads: BehaviorSubject<DownloadInfo[]> = new BehaviorSubject<DownloadInfo[]>([]);
  public set pdfDownloads(value: DownloadInfo[]) {
    this._pdfDownloads.next(value);
  }
  public get pdfDownloads(): DownloadInfo[] {
    return this._pdfDownloads.getValue();
  }

  public collapse(): void {
    this.isExpanded = false;
  }

  public toggle(): void {
    this.isExpanded = !this.isExpanded;
  }
}
