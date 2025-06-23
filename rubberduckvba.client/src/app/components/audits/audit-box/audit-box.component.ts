import { Component, Input, OnInit, TemplateRef, ViewChild, inject } from "@angular/core";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { BehaviorSubject } from "rxjs";
import { AuditRecordViewModel, FeatureEditViewModel, FeatureOperationViewModel } from "../../../model/feature.model";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { ApiClientService } from "../../../services/api-client.service";

@Component({
  selector: 'audit-box',
  templateUrl: './audit-box.component.html'
})
export class AuditBoxComponent implements OnInit {
  private readonly _audit: BehaviorSubject<AuditRecordViewModel> = new BehaviorSubject<AuditRecordViewModel>(null!);
  private _isEdit: boolean = false;

  constructor(private fa: FaIconLibrary, private api: ApiClientService) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
  }

  @Input()
  public set auditOp(value: FeatureOperationViewModel | FeatureEditViewModel) {
    this._audit.next(value);
  }

  @Input()
  public set auditEdit(value: FeatureEditViewModel) {
    this._audit.next(value);
    this._isEdit = true;
  }

  public get auditOp(): FeatureOperationViewModel | undefined {
    if (this._isEdit) {
      return undefined;
    }
    return <FeatureOperationViewModel>this._audit.value;
  }

  public get auditEdit(): FeatureEditViewModel | undefined {
    if (!this._isEdit) {
      return undefined;
    }
    return <FeatureEditViewModel>this._audit.value;
  }

  public get audit(): AuditRecordViewModel {
    if (this._isEdit) {
      return this.auditEdit!;
    }
    return this.auditOp!;
  }

  public get dateSubmitted(): string {
    return (this._isEdit
      ? this.auditEdit!.dateInserted
      : this.auditOp!.dateInserted) ?? '';
  }

  public get dateModified(): string {
    return (this._isEdit
      ? this.auditEdit!.dateModified
      : this.auditOp!.dateModified) ?? '';
  }

  public get author(): string {
    return (this._isEdit
      ? this.auditEdit!.author
      : this.auditOp!.author) ?? '';
  }

  public get isEdit(): boolean {
    return this._isEdit;
  }

  public get isCreateOp(): boolean {
    if (this._isEdit) {
      return false;
    }
    return this.auditOp!.featureAction == 1;
  }

  public get isDeleteOp(): boolean {
    if (this._isEdit) {
      return false;
    }
    return this.auditOp!.featureAction == 2;
  }

  public onConfirmApprove(): void {
    
    this.api.approvePendingAudit(this.audit.id).subscribe(() => {
      window.location.reload();
    });
  }

  public onConfirmReject(): void {
    this.api.rejectPendingAudit(this.audit.id).subscribe(() => {
      window.location.reload();
    });
  }
}
