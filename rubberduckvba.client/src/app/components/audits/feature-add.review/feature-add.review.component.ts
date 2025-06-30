import { Component, Input, OnInit } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { FeatureOperationViewModel } from "../../../model/feature.model";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { ApiClientService } from "../../../services/api-client.service";

@Component({
  selector: 'review-feature-add',
  templateUrl: './feature-add.review.component.html'
})
export class AuditFeatureAdditionComponent implements OnInit {

  private readonly _audit: BehaviorSubject<FeatureOperationViewModel> = new BehaviorSubject<FeatureOperationViewModel>(null!); 
  private readonly _summary: BehaviorSubject<string> = new BehaviorSubject<string>(null!); 
  private readonly _description: BehaviorSubject<string> = new BehaviorSubject<string>(null!); 

  constructor(private fa: FaIconLibrary, private api: ApiClientService) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.api.formatMarkdown(this.audit.shortDescription ?? '').subscribe(html => {
      this._summary.next(html.content);
    });
    this.api.formatMarkdown(this.audit.description ?? '').subscribe(html => {
      this._description.next(html.content);
    });
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

  public get htmlSummary(): string {
    return this._summary.getValue();
  }
  public get htmlDescription(): string {
    return this._description.getValue();
  }
}
