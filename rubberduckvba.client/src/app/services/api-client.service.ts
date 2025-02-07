import { Injectable } from "@angular/core";
import { LatestTags, Tag } from "../model/tags.model";
import { AnnotationViewModel, AnnotationViewModelClass, AnnotationsFeatureViewModel, AnnotationsFeatureViewModelClass, FeatureViewModel, FeatureViewModelClass, InspectionViewModel, InspectionViewModelClass, InspectionsFeatureViewModel, InspectionsFeatureViewModelClass, QuickFixViewModel, QuickFixViewModelClass, QuickFixesFeatureViewModel, QuickFixesFeatureViewModelClass, SubFeatureViewModel, SubFeatureViewModelClass, UserViewModel, XmlDocOrFeatureViewModel } from "../model/feature.model";
import { DownloadInfo } from "../model/downloads.model";
import { DataService } from "./data.service";
import { environment } from "../../environments/environment.prod";
import { Observable, map } from "rxjs";

@Injectable()
export class ApiClientService {

  constructor(private data: DataService) {
  }

  public getAvailableDownloads(): Observable<DownloadInfo[]> {

    return this.data.getAsync<DownloadInfo[]>(`${environment.apiBaseUrl}downloads`);
  }

  public getLatestTags(): Observable<LatestTags> {
    return this.data.getAsync<LatestTags>(`${environment.apiBaseUrl}tags/latest`);
  }

  public getFeatureSummaries(): Observable<FeatureViewModel[]> {
    return this.data.getAsync<FeatureViewModel[]>(`${environment.apiBaseUrl}features`);
  }

  public getFeature(name: string): Observable<XmlDocOrFeatureViewModel> {
    const url = `${environment.apiBaseUrl}features/${name}`;
    const featureName = name.toLowerCase();

    switch (featureName) {
      case "inspections":
        return this.data.getAsync<InspectionsFeatureViewModel>(url).pipe(map(e => new InspectionsFeatureViewModelClass(e)));
      case "quickfixes":
        return this.data.getAsync<QuickFixesFeatureViewModel>(url).pipe(map(e => new QuickFixesFeatureViewModelClass(e)));
      case "annotations":
        return this.data.getAsync<AnnotationsFeatureViewModel>(url).pipe(map(e => new AnnotationsFeatureViewModelClass(e)));
      default:
        return this.data.getAsync<FeatureViewModel>(url).pipe(map(e => new FeatureViewModelClass(e)));
    }
  }

  public getInspection(name: string): Observable<InspectionViewModel> {
    const url = `${environment.apiBaseUrl}inspections/${name}`
    return this.data.getAsync<InspectionViewModel>(url).pipe(map(e => new InspectionViewModelClass(e)));
  }
  public getAnnotation(name: string): Observable<AnnotationViewModel> {
    const url = `${environment.apiBaseUrl}annotations/${name}`
    return this.data.getAsync<AnnotationViewModel>(url).pipe(map(e => new AnnotationViewModelClass(e)));
  }
  public getQuickFix(name: string): Observable<QuickFixViewModel> {
    const url = `${environment.apiBaseUrl}quickfixes/${name}`
    return this.data.getAsync<QuickFixViewModel>(url).pipe(map(e => new QuickFixViewModelClass(e)));
  }
}

@Injectable()
export class AdminApiClientService {

  constructor(private data: DataService) {
  }

  public updateTagMetadata(): void {
    const url = `${environment.apiBaseUrl}admin/update/tags`;
    const jwt = sessionStorage.getItem("jwt");
    if (jwt) {
      this.data.postWithAccessTokenAsync(jwt, url);
    }
  }

  public updateXmldocMetadata(): void {
    const url = `${environment.apiBaseUrl}admin/update/xmldoc`;
    const jwt = sessionStorage.getItem("jwt");
    if (jwt) {
      this.data.postWithAccessTokenAsync(jwt, url);
    }
  }
}
