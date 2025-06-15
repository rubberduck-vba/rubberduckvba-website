import { Injectable } from "@angular/core";
import { LatestTags, Tag } from "../model/tags.model";
import { AnnotationViewModel, AnnotationViewModelClass, AnnotationsFeatureViewModel, AnnotationsFeatureViewModelClass, FeatureViewModel, FeatureViewModelClass, InspectionViewModel, InspectionViewModelClass, InspectionsFeatureViewModel, InspectionsFeatureViewModelClass, MarkdownContent, PendingAuditsViewModel, QuickFixViewModel, QuickFixViewModelClass, QuickFixesFeatureViewModel, QuickFixesFeatureViewModelClass, SubFeatureViewModel, SubFeatureViewModelClass, UserViewModel, XmlDocOrFeatureViewModel } from "../model/feature.model";
import { DownloadInfo } from "../model/downloads.model";
import { DataService } from "./data.service";
import { environment } from "../../environments/environment.prod";
import { Observable, map } from "rxjs";
import { IndenterVersionViewModelClass, IndenterViewModel, IndenterViewModelClass } from "../model/indenter.model";

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

  public updateTagMetadata(): Observable<number> {
    const url = `${environment.apiBaseUrl}admin/update/tags`;
    return this.data.postAsync(url);
  }

  public updateXmldocMetadata(): Observable<number> {
    const url = `${environment.apiBaseUrl}admin/update/xmldoc`;
    return this.data.postAsync(url);
  }

  public clearCache(): Observable<any> {
    const url = `${environment.apiBaseUrl}admin/cache/clear`;
    return this.data.postAsync(url);
  }

  public getIndenterDefaults(): Observable<IndenterViewModel> {
    const url = `${environment.apiBaseUrl}indenter/defaults`;
    return this.data.getAsync<IndenterViewModel>(url).pipe(map(model => new IndenterViewModelClass(model)));
  }

  public indent(model: IndenterViewModel): Observable<IndenterViewModel> {
    const url = `${environment.apiBaseUrl}indenter/indent`;
    return this.data.postAsync<IndenterViewModel, string[]>(url, model).pipe(map(lines => {
      model.indentedCode = lines.join('\n');
      return model;
    }));
  }

  public createFeature(model: SubFeatureViewModel): Observable<SubFeatureViewModel> {
    const url = `${environment.apiBaseUrl}features/create`;
    return this.data.postAsync<SubFeatureViewModel, SubFeatureViewModel>(url, model).pipe(map(result => new SubFeatureViewModelClass(result as SubFeatureViewModel)));
  }

  public saveFeature(model: SubFeatureViewModel): Observable<SubFeatureViewModel> {
    const url = `${environment.apiBaseUrl}features/update`;
    return this.data.postAsync<SubFeatureViewModel, SubFeatureViewModel>(url, model).pipe(map(result => new SubFeatureViewModelClass(result as SubFeatureViewModel)));
  }

  public deleteFeature(model: SubFeatureViewModel): Observable<SubFeatureViewModel> {
    const url = `${environment.apiBaseUrl}features/delete`;
    return this.data.postAsync<SubFeatureViewModel, undefined>(url, model).pipe(map(() => model));
  }

  public getPendingAudits(): Observable<PendingAuditsViewModel> {
    const url = `${environment.apiBaseUrl}admin/audits`;
    return this.data.getAsync<PendingAuditsViewModel>(url);
  }

  public formatMarkdown(raw: string): Observable<MarkdownContent> {
    const url = `${environment.apiBaseUrl}markdown/format`;
    const content: MarkdownContent = {
      content: raw
    };
    return this.data.postAsync<MarkdownContent, MarkdownContent>(url, content);
  }
}
