import { Component, OnInit } from '@angular/core';
import { ApiClientService } from "../../services/api-client.service";
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { BehaviorSubject } from 'rxjs';
import { FeatureViewModel, QuickFixViewModel } from '../../model/feature.model';

@Component({
  selector: 'app-features',
  templateUrl: './features.component.html',
})
export class FeaturesComponent implements OnInit {

  private readonly _features: BehaviorSubject<FeatureViewModel[]> = new BehaviorSubject<FeatureViewModel[]>(null!);
  public set features(value: FeatureViewModel[]) {
    this._features.next(value);
  }
  public get features(): FeatureViewModel[] {
    return this._features.getValue();
  }

  constructor(private api: ApiClientService, private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.api.getFeatureSummaries().subscribe(result => {
      if (result) {
        this._features.next(result.filter(e => !e.isHidden));
      }
    });
  }
}
