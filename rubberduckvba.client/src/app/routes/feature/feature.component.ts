import { Component, OnChanges, OnInit, SimpleChanges, inject } from '@angular/core';
import { ApiClientService } from "../../services/api-client.service";
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { BehaviorSubject, switchMap } from 'rxjs';
import { Feature, FeatureViewModel } from '../../model/feature.model';
import { ActivatedRoute } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-feature',
  templateUrl: './feature.component.html',
})
export class FeatureComponent implements OnInit {

  public modal = inject(NgbModal);

  private readonly _feature: BehaviorSubject<FeatureViewModel> = new BehaviorSubject<FeatureViewModel>(null!);
  public set feature(value: FeatureViewModel){
    this._feature.next(value);
  }
  public get feature(): FeatureViewModel {
    return this._feature.getValue();
  }
  constructor(private api: ApiClientService, private fa: FaIconLibrary, private route: ActivatedRoute) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const name = params.get('name')!;
        return this.api.getFeature(name);
      })).subscribe(e => {
        this.feature = new FeatureViewModel(e);
      });
  }
}
