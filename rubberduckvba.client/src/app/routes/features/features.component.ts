import { Component, OnInit } from '@angular/core';
import { ApiClientService } from "../../services/api-client.service";
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { BehaviorSubject } from 'rxjs';
import { FeatureViewModel, PendingAuditsViewModel, QuickFixViewModel, UserViewModel } from '../../model/feature.model';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-features',
    templateUrl: './features.component.html',
    standalone: false
})
export class FeaturesComponent implements OnInit {

  private readonly _user: BehaviorSubject<UserViewModel> = new BehaviorSubject<UserViewModel>(null!);
  private readonly _audits: BehaviorSubject<PendingAuditsViewModel> = new BehaviorSubject<PendingAuditsViewModel>(null!);

  private readonly _features: BehaviorSubject<FeatureViewModel[]> = new BehaviorSubject<FeatureViewModel[]>(null!);

  public set features(value: FeatureViewModel[]) {
    this._features.next(value);
  }
  public get features(): FeatureViewModel[] {
    return this._features.getValue();
  }

  public get user() {
    return this._user.getValue();
  }

  public get audits() {
    return this._audits.getValue() ?? {
      edits: [],
      other: []
    };
  }

  constructor(private api: ApiClientService, private auth: AuthService, private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.api.getFeatureSummaries().subscribe(result => {
      if (result) {
        this._features.next(result.filter(e => !e.isHidden));
      }
    });
    this.auth.getUser().subscribe(result => {
      if (result) {
        this._user.next(result);
        if (this.user.isAdmin) {
          this.api.getAllPendingAudits().subscribe(audits => {
            if (audits) {
              this._audits.next(audits);
            }
          })
        }
      }
    });
  }
}
