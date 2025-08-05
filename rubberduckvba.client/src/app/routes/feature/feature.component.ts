import { Component, OnChanges, OnInit, SimpleChanges, inject } from '@angular/core';
import { ApiClientService } from "../../services/api-client.service";
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { BehaviorSubject, switchMap } from 'rxjs';
import { PendingAuditsViewModel, UserViewModel, XmlDocOrFeatureViewModel } from '../../model/feature.model';
import { ActivatedRoute } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-feature',
    templateUrl: './feature.component.html',
    standalone: false
})
export class FeatureComponent implements OnInit {

  public modal = inject(NgbModal);

  private readonly _feature: BehaviorSubject<XmlDocOrFeatureViewModel> = new BehaviorSubject<XmlDocOrFeatureViewModel>(null!);
  public get feature(): XmlDocOrFeatureViewModel {
    return this._feature.getValue();
  }

  private readonly _user: BehaviorSubject<UserViewModel> = new BehaviorSubject<UserViewModel>(null!);
  public get user(): UserViewModel {
    return this._user.getValue();
  }

  private readonly _audits: BehaviorSubject<PendingAuditsViewModel> = new BehaviorSubject<PendingAuditsViewModel>(null!);
  public get audits(): PendingAuditsViewModel {
    return this._audits.getValue();
  }

  constructor(private auth: AuthService, private api: ApiClientService, private fa: FaIconLibrary, private route: ActivatedRoute) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const name = params.get('name')!;
        return this.api.getFeature(name);
      })).subscribe(e => {
        this._feature.next(e);
      });

    this.auth.getUser().subscribe(e => {
      this._user.next(e);
      if (e.isAdmin) {
        this.api.getAllPendingAudits().subscribe(a => {
          this._audits.next(a);
        });
      }
    });
  }
}
