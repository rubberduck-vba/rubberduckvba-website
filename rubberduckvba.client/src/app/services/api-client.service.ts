import { Injectable } from "@angular/core";
import { LatestTags, Tag } from "../model/tags.model";
import { Feature, FeatureSummary, PaginatedFeature } from "../model/feature.model";
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

  public getFeatureSummaries(): Observable<FeatureSummary[]> {
    return this.data.getAsync<FeatureSummary[]>(`${environment.apiBaseUrl}features`);
  }

  public getFeature(name: string): Observable<Feature> {
    return this.data.getAsync<any>(`${environment.apiBaseUrl}features/${name}`);
  }

  public getMarkdown(value: string, syntaxHighlighting: boolean): Observable<MarkdownFormattingInfo> {
    const payload = new MarkdownFormattingViewModel(value, syntaxHighlighting);
    return this.data.postAsync(`${environment.apiBaseUrl}features/markdown`, payload);
  }
}

export interface MarkdownFormattingInfo {
  markdownContent: string;
  withVbeCodeBlocks: boolean;
}

export class MarkdownFormattingViewModel implements MarkdownFormattingInfo {
  public markdownContent: string = '';
  public withVbeCodeBlocks: boolean = false;

  constructor(content: string, syntax: boolean) {
    this.markdownContent = content;
    this.withVbeCodeBlocks = syntax;
  }
}
