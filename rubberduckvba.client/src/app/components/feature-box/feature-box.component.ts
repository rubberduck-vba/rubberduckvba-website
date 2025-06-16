import { Component, Input, OnChanges, OnInit, SimpleChanges, TemplateRef, ViewChild, inject } from '@angular/core';
import { AuditRecordViewModel, FeatureEditViewModel, FeatureOperation, FeatureOperationViewModel, FeatureViewModel, PendingAuditsViewModel, QuickFixViewModel, SubFeatureViewModel, UserViewModel } from '../../model/feature.model';
import { BehaviorSubject } from 'rxjs';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AuthService } from '../../services/auth.service';
import { AdminAction } from '../edit-feature/edit-feature.component';
import { ApiClientService } from '../../services/api-client.service';

@Component({
  selector: 'feature-box',
  templateUrl: './feature-box.component.html'
})
export class FeatureBoxComponent implements OnInit, OnChanges {

  private readonly _info: BehaviorSubject<FeatureViewModel> = new BehaviorSubject<FeatureViewModel>(null!);
  private readonly _edits: BehaviorSubject<AuditRecordViewModel[]> = new BehaviorSubject<AuditRecordViewModel[]>([]);
  private readonly _ops: BehaviorSubject<FeatureOperationViewModel[]> = new BehaviorSubject<FeatureOperationViewModel[]>([]);

  private _audits?: PendingAuditsViewModel;

  @ViewChild('confirmDeleteFeature', { read: TemplateRef }) confirmDeleteFeatureModal: TemplateRef<any> | undefined;
  public modal = inject(NgbModal);

  private readonly _user: BehaviorSubject<UserViewModel> = new BehaviorSubject<UserViewModel>(null!);

  @Input()
  public set user(value: UserViewModel) {
    this._user.next(value);
  }

  public get user(): UserViewModel {
    return this._user.getValue();
  }

  @Input()
  public parentFeatureName: string = '';

  @Input()
  public hasOwnDetailsPage: boolean = false;

  @Input()
  public set pendingAudits(value: PendingAuditsViewModel) {
    this._audits = value;
    this._edits.next(value.edits.filter(e => this.feature && e.featureId == this.feature.id?.toString()));
    this._ops.next(value.other.filter(e => this.feature && e.featureName == this.feature.name));
  };

  public get prndingAudits(): PendingAuditsViewModel {
    return this._audits ?? {
      edits: [],
      other: []
    };
  }


  constructor(private fa: FaIconLibrary, private api: ApiClientService, private auth: AuthService) {
    fa.addIconPacks(fas);
  }

  ngOnChanges(changes: SimpleChanges): void {
  }

  ngOnInit(): void {
  }

  public get isProtected(): boolean {
    return this.feature?.name == 'Inspections'
      || this.feature?.name == 'QuickFixes'
      || this.feature?.name == 'Annotations'
      || this.feature?.name == 'CodeInspections'
      || this.feature?.name == 'CommentAnnotations';
  }

  public get hasXmlDocFeatures(): boolean {
    return this.feature?.name == 'Inspections'
      || this.feature?.name == 'QuickFixes'
      || this.feature?.name == 'Annotations';
  }

  @Input()
  public set feature(value: FeatureViewModel | undefined) {
    if (value != null) {
      this._info.next(value);
    }
  }

  public editAction: AdminAction = AdminAction.EditSummary;
  public editDetailsAction: AdminAction = AdminAction.Edit;
  public createAction: AdminAction = AdminAction.Create;
  public deleteAction: AdminAction = AdminAction.Delete;

  public get pendingEdits(): AuditRecordViewModel[] {
    return this._edits.getValue();
  }

  public get pendingOperations(): FeatureOperationViewModel[] {
    return this._ops.getValue();
  }

  public get hasPendingEdits(): boolean {
    return this._edits.getValue().length > 0;
  }

  public get pendingEdit(): AuditRecordViewModel {
    return this._edits.getValue()[0];
  }

  public get hasPendingDelete(): boolean {
    return this._ops.getValue().some(e => e.featureAction == FeatureOperation.Delete);
  }

  public get pendingDelete(): FeatureOperationViewModel {
    return this._ops.getValue().find(e => e.featureAction == FeatureOperation.Delete)!;
  }

  public onApprovePendingDelete(): void {
    if (!this.hasPendingDelete) {
      return;
    }
    this.modal.open(this.confirmDeleteFeatureModal, { modalDialogClass: 'modal-l'})
  }

  public onConfirmApprovePendingDelete(): void {
    if (!this.hasPendingDelete) {
      return;
    }
    this.modal.dismissAll();
    this.api.approvePendingAudit(this.pendingDelete.id).subscribe(e => {
      window.location.reload();
    });
  }

  public onConfirmCreate(): void {
    if (this.feature?.id && this.feature?.isCreatePending) {
      this.api.approvePendingAudit(this.feature.id).subscribe(e => {
        window.location.reload();
      })
    }
  }

  public onRejectPendingCreate(): void {
    if (this.feature?.id && this.feature?.isCreatePending) {
      this.api.rejectPendingAudit(this.feature.id).subscribe(e => {
        window.location.reload();
      })
    }
  }

  public onReviewPendingEdits(): void {
    if (!this.hasPendingEdits) {
      return;
    }    
  }

  public get feature(): FeatureViewModel | undefined {
    return this._info.value as FeatureViewModel;
  }

  public get subFeature(): SubFeatureViewModel | undefined {
    return this._info.value as SubFeatureViewModel;
  }

  public applyChanges(model: any): void {
    this._info.next(model);
  }
}
