import { Component, OnInit } from "@angular/core";
import { ApiClientService } from "../../../services/api-client.service";
import { ActivatedRoute } from "@angular/router";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { BehaviorSubject, switchMap } from "rxjs";
import { AuditRecordViewModel, FeatureEditViewModel, FeatureOperationViewModel, UserActivityType } from "../../../model/feature.model";

@Component({
  selector: 'app-audit-item',
  templateUrl: './audit-item.component.html',
})
export class AuditItemComponent implements OnInit {

  private _item: BehaviorSubject<AuditRecordViewModel> = new BehaviorSubject<AuditRecordViewModel>(null!);
  private _op: BehaviorSubject<FeatureOperationViewModel> = new BehaviorSubject<FeatureOperationViewModel>(null!);
  private _edit: BehaviorSubject<FeatureEditViewModel> = new BehaviorSubject<FeatureEditViewModel>(null!);

  private _isEdit: boolean = false;

  constructor(private fa: FaIconLibrary, private api: ApiClientService, private route: ActivatedRoute) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    const route = this.route;
    route.paramMap.pipe(
      switchMap(params => {
        const id = Number.parseInt(params.get('id')!);
        if (route.routeConfig?.path?.includes('/edit')) {
          return this.api.getAudit(id, UserActivityType.SubmitEdit);
        }
        //else if (route.routeConfig?.path?.includes('/op')) {
        //  return this.api.getAudit(id, UserActivityType.SubmitCreate);
        //}
        return this.api.getAudit(id, UserActivityType.SubmitCreate);
      })
    ).subscribe(e => {
      if (e.edits.length > 0) {
        const edit = e.edits[0];
        this._item.next(edit);
        this._edit.next(edit);
        this._isEdit = true;
      }
      else if (e.other.length > 0) {
        const op = e.other[0];
        this._item.next(op);
        this._op.next(op);
        this._isEdit = false;
      }
    });
  }

  public get isEdit(): boolean {
    return this._isEdit;
  }

  public get featureOp(): FeatureOperationViewModel {
    return this._op.getValue();
  }

  public get featureEdit(): FeatureEditViewModel {
    return this._edit.getValue();
  }

  public get item(): AuditRecordViewModel {
    return this._item.getValue();
  }
}
