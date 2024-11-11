import { Injectable } from "@angular/core";
import { LatestTags, Tag } from "../model/tags.model";
import { AnnotationsFeatureViewModel, FeatureViewModel, InspectionsFeatureViewModel, QuickFixViewModel, QuickFixesFeatureViewModel, SubFeatureViewModel } from "../model/feature.model";
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

  public getFeature(name: string): Observable<SubFeatureViewModel|InspectionsFeatureViewModel|QuickFixesFeatureViewModel|AnnotationsFeatureViewModel> {
    const url = `${environment.apiBaseUrl}features/${name}`;
    const featureName = name.toLowerCase();

    switch (featureName) {
      case "inspections":
        return this.data.getAsync<InspectionsFeatureViewModel>(url);
      case "quickfixes":
        return this.data.getAsync<QuickFixesFeatureViewModel>(url);
      case "annotations":
        return this.data.getAsync<AnnotationsFeatureViewModel>(url);
      default:
        return this.data.getAsync<SubFeatureViewModel>(`${environment.apiBaseUrl}features/${name}`);
    }
  }
}
