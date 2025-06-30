import { Component, Input, OnInit } from "@angular/core";
import { FeatureOperationViewModel } from "../../../model/feature.model";
import { BehaviorSubject } from "rxjs";

@Component({
  selector: 'review-feature-delete',
  templateUrl: './feature-delete.review.component.html'
})
export class AuditFeatureDeleteComponent implements OnInit {

  private readonly _audit: BehaviorSubject<FeatureOperationViewModel> = new BehaviorSubject<FeatureOperationViewModel>(null!); 

  constructor() { }

  ngOnInit(): void {
  }

  @Input()
  public set audit(value: FeatureOperationViewModel | undefined) {
    if (value) {
      this._audit.next(value);
    }
  }

  public get audit(): FeatureOperationViewModel {
    return this._audit.getValue();
  }
}
