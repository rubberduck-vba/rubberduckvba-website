import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { AnnotationViewModel, AnnotationsFeatureViewModel, BlogLink, FeatureViewModel, InspectionViewModel, InspectionsFeatureViewModel, QuickFixViewModel, QuickFixesFeatureViewModel, SubFeatureViewModel, XmlDocItemViewModel, XmlDocOrFeatureViewModel, XmlDocViewModel } from '../../model/feature.model';
import { BehaviorSubject } from 'rxjs';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { ApiClientService } from '../../services/api-client.service';

@Component({
  selector: 'feature-info',
  templateUrl: './feature-info.component.html',
})
export class FeatureInfoComponent implements OnInit, OnChanges {

  private readonly _info: BehaviorSubject<XmlDocOrFeatureViewModel> = new BehaviorSubject<XmlDocOrFeatureViewModel>(null!);

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
      this.filterByNameOrDescription(this.filterState.filterText)
    }
  }

  private _filteredItems: XmlDocItemViewModel[] = [];
  public get filteredItems(): XmlDocItemViewModel[] {
    return this._filteredItems;
  }

  public get feature(): XmlDocOrFeatureViewModel | undefined {
    return this._info.value;
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
  private readonly _quickfixes: BehaviorSubject<QuickFixViewModel[]> = new BehaviorSubject<QuickFixViewModel[]>(null!);

  public get subfeatures(): FeatureViewModel[] {
    return (this.feature as FeatureViewModel)?.features ?? [];
  }

  @Input()
  public set quickFixes(value: QuickFixViewModel[]) {
    if (value != null) {
      this._quickfixes.next(value);
    }
  }

  public get quickFixes(): QuickFixViewModel[] {
    return this._quickfixes.value;
  }

  public get links(): BlogLink[] {
    let feature = <FeatureViewModel>this.feature;
    return feature?.links ?? [];
  }

  constructor(private api: ApiClientService, private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.api.getFeature('quickfixes').subscribe(result => {
      if (result) {
        this._quickfixes.next((result as QuickFixesFeatureViewModel).quickFixes.slice());
      }
    });
  }


  ngOnChanges(changes: SimpleChanges): void {
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
