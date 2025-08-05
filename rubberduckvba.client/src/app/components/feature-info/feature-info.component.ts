import { Component, Input, OnInit } from '@angular/core';
import { AnnotationViewModel, AnnotationsFeatureViewModel, BlogLink, FeatureOperationViewModel, FeatureViewModel, InspectionViewModel, InspectionsFeatureViewModel, PendingAuditsViewModel, QuickFixViewModel, QuickFixesFeatureViewModel, UserViewModel, XmlDocItemViewModel, XmlDocOrFeatureViewModel } from '../../model/feature.model';
import { BehaviorSubject } from 'rxjs';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { AdminAction } from '../edit-feature/edit-feature.component';
import { ApiClientService } from '../../services/api-client.service';

@Component({
    selector: 'feature-info',
    templateUrl: './feature-info.component.html',
    standalone: false
})
export class FeatureInfoComponent implements OnInit {

  private readonly _info: BehaviorSubject<XmlDocOrFeatureViewModel> = new BehaviorSubject<XmlDocOrFeatureViewModel>(null!);
  private readonly _user: BehaviorSubject<UserViewModel> = new BehaviorSubject<UserViewModel>(null!);
  private readonly _audits: BehaviorSubject<PendingAuditsViewModel> = new BehaviorSubject<PendingAuditsViewModel>(null!);

  public editAction: AdminAction = AdminAction.Edit;
  public editLinksAction: AdminAction = AdminAction.EditLinks;
  public createAction: AdminAction = AdminAction.Create;
  public deleteAction: AdminAction = AdminAction.Delete;

  public filterState = {
    // searchbox
    filterText: '',
    // inspection severity
    donotshow: false,
    hint: true,
    suggestion: true,
    warning: true,
    error: true,
    // inspection type
    codeQualityIssues: true,
    languageOpportunities: true,
    namingAndConventionsIssues: true,
    rubberduckOpportunities: true
  };

  @Input()
  public set feature(value: XmlDocOrFeatureViewModel | undefined) {
    if (value != null) {
      this._info.next(value);
      this._formattedDescriptionHtml = value.description;
      this._formattedShortDescriptionHtml = value.shortDescription;
      this.filterByNameOrDescription(this.filterState.filterText)

      this.api.formatMarkdown(value.description).subscribe(e => this._formattedDescriptionHtml = e.content);
      this.api.formatMarkdown(value.shortDescription).subscribe(e => this._formattedShortDescriptionHtml = e.content);
    }
  }
  public get feature(): XmlDocOrFeatureViewModel | undefined {
    return this._info.getValue();
  }

  private _formattedDescriptionHtml: string = '';
  private _formattedShortDescriptionHtml: string = '';

  public get formattedDescriptionHtml(): string {
    return this._formattedDescriptionHtml;
  }

  public get formattedShortDescriptionHtml(): string {
    return this._formattedShortDescriptionHtml;
  }
  @Input()
  public set user(value: UserViewModel) {
    this._user.next(value);
  }

  public get user(): UserViewModel {
    return this._user.getValue();
  }

  @Input()
  public set audits(value: PendingAuditsViewModel) {
    this._audits.next(value);
  }

  public get audits(): PendingAuditsViewModel {
    return this._audits.getValue();
  }

  public get pendingFeatures(): FeatureViewModel[] {
    if (!this.audits?.other) {
      return [];
    }

    return this.audits.other.filter(e => e.featureAction == 1 && e.parentId == this.feature?.id)
      .map<FeatureViewModel>((e) => {
        return {
          id: e.id,
          dateInserted: e.dateInserted,          
          dateUpdated: '',
          features: [],
          name: e.name ?? '',
          title: e.title ?? '',
          description: e.description ?? '',
          shortDescription: e.shortDescription ?? '',
          featureId: e.parentId ?? undefined,
          featureName: undefined,
          featureTitle: undefined,
          hasImage: e.hasImage ?? false,
          isHidden: e.isHidden ?? false,
          isNew: e.isNew ?? false,
          links: e.links ?? [],
          isCollapsed: false,
          isDetailsCollapsed: true,

          isCreatePending: true
        }
      });
  }

  private _filteredItems: XmlDocItemViewModel[] = [];
  public get filteredItems(): XmlDocItemViewModel[] {
    return this._filteredItems;
  }

  public get inspectionItems(): InspectionViewModel[] {
    return (this.feature as InspectionsFeatureViewModel)?.inspections?.filter(e => !e.isHidden) ?? [];
  }

  public get annotationItems(): AnnotationViewModel[] {
    return (this.feature as AnnotationsFeatureViewModel)?.annotations?.filter(e => !e.isHidden) ?? [];
  }

  public get quickfixItems(): QuickFixViewModel[] {
    return (this.feature as QuickFixesFeatureViewModel)?.quickFixes?.filter(e => !e.isHidden) ?? [];
  }

  public get subfeatures(): FeatureViewModel[] {
    return (this.feature as FeatureViewModel)?.features ?? [];
  }

  public get links(): BlogLink[] {
    let feature = <FeatureViewModel>this.feature;
    return feature?.links ?? [];
  }

  constructor(private fa: FaIconLibrary, private api: ApiClientService) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
  }

  public onFilter(): void {
    this.filterByNameOrDescription(this.filterState.filterText);
  }

  private onSeverityFilter(): void {
    this._filteredItems = this._filteredItems.filter(item => {
      const vm = <InspectionViewModel>item;
      if (vm.isHidden /* !this.filterState.showHiddenStuff? */) {
        return false;
      }

      if (this.feature?.name == 'Inspections') {
        if (!this.filterState.donotshow && vm.defaultSeverity == 'DoNotShow') {
          return false;
        }
        if (!this.filterState.hint && vm.defaultSeverity == 'Hint') {
          return false;
        }
        if (!this.filterState.suggestion && vm.defaultSeverity == 'Suggestion') {
          return false;
        }
        if (!this.filterState.warning && vm.defaultSeverity == 'Warning') {
          return false;
        }
        if (!this.filterState.error && vm.defaultSeverity == 'Error') {
          return false;
        }
      }
      return true;
    });
  }

  private onInspectionTypeFilter(): void {
    this._filteredItems = this._filteredItems.filter(item => {
      const vm = <InspectionViewModel>item;
      if (vm.isHidden) {
        return false;
      }

      if (this.feature?.name == 'Inspections') {
        if (!this.filterState.codeQualityIssues && vm.inspectionType == 'Code Quality Issues') {
          return false;
        }
        if (!this.filterState.languageOpportunities && vm.inspectionType == 'Language Opportunities') {
          return false;
        }
        if (!this.filterState.namingAndConventionsIssues && vm.inspectionType == 'Naming and Convention Issues') {
          return false;
        }
        if (!this.filterState.rubberduckOpportunities && vm.inspectionType == 'Rubberduck Opportunities') {
          return false;
        }
      }

      return true;
    })
  }

  private filterByNameOrDescription(filter: string) {
    const contains = (value: string, filter: string): boolean => value ? value.toLowerCase().indexOf(filter.toLowerCase()) >= 0 : false;

    const features = (this.feature as InspectionsFeatureViewModel).inspections
                  || (this.feature as QuickFixesFeatureViewModel).quickFixes
                  || (this.feature as AnnotationsFeatureViewModel).annotations
                  || (this.feature as FeatureViewModel).features;

    this._filteredItems = features != undefined
      ? features.filter(item => filter === ''
        || contains(item.name, filter)
        || contains(item.summary, filter)
        || contains(item.reasoning, filter)
      ) : [];

    if (this.feature?.name === 'Inspections') {
      this.onSeverityFilter();
      this.onInspectionTypeFilter();
    }
  }

  public onSearchFilterChange(event: Event): void {
    const filter: string = (event.target as HTMLInputElement).value;
    this.filterState.filterText = filter;
    this.onFilter();
  }

  public onSearchClear(): void {
    this.filterState.filterText = '';
    this.onFilter();
  }
}
