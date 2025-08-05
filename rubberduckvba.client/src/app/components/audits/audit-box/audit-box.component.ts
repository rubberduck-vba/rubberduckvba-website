import { Component, Input, OnInit, TemplateRef, ViewChild, inject } from "@angular/core";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { BehaviorSubject } from "rxjs";
import { AuditRecordViewModel, FeatureEditViewModel, FeatureOperationViewModel, UserViewModel } from "../../../model/feature.model";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { ApiClientService } from "../../../services/api-client.service";
import { AuthService } from "../../../services/auth.service";

@Component({
    selector: 'audit-box',
    templateUrl: './audit-box.component.html',
    standalone: false
})
export class AuditBoxComponent implements OnInit {
  private readonly _audit: BehaviorSubject<AuditRecordViewModel> = new BehaviorSubject<AuditRecordViewModel>(null!);
  private _isEdit: boolean = false;

  @ViewChild('confirmApproveModal', { read: TemplateRef }) confirmApproveModal: TemplateRef<any> | undefined;
  @ViewChild('confirmRejectModal', { read: TemplateRef }) confirmRejectModal: TemplateRef<any> | undefined;

  constructor(private fa: FaIconLibrary, private api: ApiClientService, private auth: AuthService, private modal: NgbModal) {
    fa.addIconPacks(fas);
  }

  private _user: UserViewModel = null!;

  ngOnInit(): void {
    this.auth.getUser().subscribe(user => {
      this._user = user;
    });
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

  public isCollapsed: boolean = true;
  public isCollapsible: boolean = true;

  @Input()
  public set collapsible(value: boolean) {
    this.isCollapsible = value;
    this.isCollapsed = this.isCollapsible;
  }

  public get canReview(): boolean {
    return this._user && (!this.collapsible || !this.isCollapsed)
      && this._user.isAdmin || (this._user.isReviewer && this.audit.author != this._user.name);
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

  public confirmApprove(): void {
    this.modal.open(this.confirmApproveModal);
  }

  public confirmReject(): void {
    this.modal.open(this.confirmRejectModal);
  }

  public onCancelModal(): void {
    this.modal.dismissAll();
  }

  public onConfirmApprove(): void {    
    console.log(`approving audit operation id ${this.audit.id}`);
    console.log(this.audit);
    this.modal.dismissAll();
    this.api.approvePendingAudit(this.audit.id).subscribe(() => {
      window.location.reload();
    });
  }

  public onConfirmReject(): void {
    console.log(`rejecting audit operation id ${this.audit.id}`);
    console.log(this.audit);
    this.modal.dismissAll();
    this.api.rejectPendingAudit(this.audit.id).subscribe(() => {
      window.location.reload();
    });
  }
}
