import { Component, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { ApiClientService } from "../../services/api-client.service";
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { BehaviorSubject } from 'rxjs';
import { Feature, FeatureItem, FeatureViewModel, QuickFixViewModel } from '../../model/feature.model';

@Component({
  selector: 'app-features',
  templateUrl: './features.component.html',
})
export class FeaturesComponent implements OnInit, OnChanges {

  private readonly _features: BehaviorSubject<FeatureViewModel[]> = new BehaviorSubject<FeatureViewModel[]>(null!);
  public set features(value: FeatureViewModel[]) {
    this._features.next(value);
  }
  public get features(): FeatureViewModel[] {
    return this._features.getValue();
  }

  private readonly _quickFixes: BehaviorSubject<FeatureItem[]> = new BehaviorSubject<FeatureItem[]>(null!);
  public get quickFixes(): FeatureItem[] {
    return this._quickFixes.value;
  }

  constructor(private api: ApiClientService, private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
  }

  ngOnChanges(changes: SimpleChanges): void {
    console.log(changes);
  }

  ngOnInit(): void {
    this.api.getFeatureSummaries().subscribe(result => {
      if (result) {
        this._features.next(result.map(feature => new FeatureViewModel(feature)).filter(e => !e.isHidden));
      }
    });
  }
}
