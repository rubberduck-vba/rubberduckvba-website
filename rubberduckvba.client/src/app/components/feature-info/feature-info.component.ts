import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Feature, FeatureItem, FeatureItemViewModel, FeatureViewModel, InspectionInfoViewModel, QuickFixViewModel } from '../../model/feature.model';
import { BehaviorSubject } from 'rxjs';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { ApiClientService } from '../../services/api-client.service';

@Component({
  selector: 'feature-info',
  templateUrl: './feature-info.component.html',
})
export class FeatureInfoComponent implements OnInit, OnChanges {

  private readonly _info: BehaviorSubject<FeatureViewModel> = new BehaviorSubject<FeatureViewModel>(null!);

  public filterState = {
    filterText: '',
    donotshow: false,
    hint: true,
    suggestion: true,
    warning: true,
    error: true,
  };

  @Input()
  public set feature(value: FeatureViewModel | undefined) {
    if (value != null) {
      this._info.next(value);
      this.filterByNameOrDescription(this.filterState.filterText)
    }
  }

  private _filteredItems: FeatureItemViewModel[] = [];
  public get filteredItems(): FeatureItemViewModel[] {
    return this._filteredItems;
  }

  public get feature(): FeatureViewModel | undefined {
    return this._info.value;
  }

  private readonly _quickfixes: BehaviorSubject<FeatureItem[]> = new BehaviorSubject<FeatureItem[]>(null!);

  @Input()
  public set quickFixes(value: FeatureItem[]) {
    if (value != null) {
      this._quickfixes.next(value);
    }
  }

  public get quickFixes(): FeatureItem[] {
    return this._quickfixes.value;
  }

  constructor(private api: ApiClientService, private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.api.getFeature('QuickFixes').subscribe(result => {
      if (result) {
        this._quickfixes.next(result.items.slice());
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
      const vm = <InspectionInfoViewModel>item.info;
      if (vm.isHidden /* !this.filterState.showHiddenStuff? */) {
        return false;
      }
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

      return true;
    });
  }

  private filterByNameOrDescription(filter: string) {
    const contains = (value: string, filter: string): boolean => value ? value.toLowerCase().indexOf(filter.toLowerCase()) >= 0 : false;
    this._filteredItems = this.feature
      ? this.feature.items.filter(item => filter === ''
        || contains(item.name, filter)
        || contains(item.summary, filter)
        || contains(item.reasoning, filter)
      ) : [];

    if (this.feature?.name === 'Inspections') {
      this.onSeverityFilter();
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
