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
export class FeatureBoxComponent implements OnInit {

  private readonly _info: BehaviorSubject<FeatureViewModel> = new BehaviorSubject<FeatureViewModel>(null!);
  private readonly _edits: BehaviorSubject<FeatureEditViewModel[]> = new BehaviorSubject<FeatureEditViewModel[]>([]);
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
    this._edits.next(value.edits?.filter(e => this.feature && e.featureName == this.feature.name));
    this._ops.next(value.other?.filter(e => this.feature && e.featureName == this.feature.name));

    if (this.pendingEdit) {
      if (this.pendingEdit.fieldName == 'ShortDescription') {
        this.api.formatMarkdown(this.pendingEdit.valueAfter).subscribe(e => this._pendingSummaryHtml = e.content);
      }
      else if (this.pendingEdit.fieldName == 'Description') {
        this.api.formatMarkdown(this.pendingEdit.valueAfter).subscribe(e => this._pendingDescriptionHtml = e.content);
      }
    }
  };

  public get pendingAudits(): PendingAuditsViewModel {
    return this._audits ?? {
      edits: [],
      other: []
    };
  }

  constructor(private fa: FaIconLibrary, private api: ApiClientService, private auth: AuthService) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.api.formatMarkdown(this.feature?.description ?? '').subscribe(e => this._descriptionHtml = e.content);
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
      this.api.formatMarkdown(value.shortDescription).subscribe(e => this._summaryHtml = e.content);
      this.api.formatMarkdown(value.description).subscribe(e => this._descriptionHtml = e.content);
    }
  }

  public editAction: AdminAction = AdminAction.EditSummary;
  public editDetailsAction: AdminAction = AdminAction.Edit;
  public createAction: AdminAction = AdminAction.Create;
  public deleteAction: AdminAction = AdminAction.Delete;

  public showPendingEdit: boolean = true;
  public get canToggleShowPendingEdit(): boolean {
    return this.hasPendingEdits && (this.user.isAdmin || this.pendingEdit.author == this.user.name);
  }

  public get pendingEdits(): FeatureEditViewModel[] {
    return this._edits.getValue().filter(e => e.fieldName == 'ShortDescription' || e.fieldName == 'Description');
  }

  public get pendingOperations(): FeatureOperationViewModel[] {
    return this._ops.getValue();
  }

  public get hasPendingEdits(): boolean {
    return this.pendingEdits.length > 0;
  }

  public get pendingEdit(): FeatureEditViewModel {
    return this.pendingEdits[0];
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

  private _pendingSummaryHtml: string = '';
  private _summaryHtml: string = '';

  public get summaryHtml(): string {
    return this.showPendingEdit && this.canToggleShowPendingEdit && this.pendingEdit.fieldName == 'ShortDescription'
      ? this._pendingSummaryHtml
      : this._summaryHtml;
  }

  private _pendingDescriptionHtml: string = '';

  private _descriptionHtml: string = '';
  public get descriptionHtml(): string {
    return this.showPendingEdit && this.canToggleShowPendingEdit && this.pendingEdit.fieldName == 'Description'
      ? this._pendingDescriptionHtml
      : this._descriptionHtml;
  }

  public applyChanges(model: any): void {
    this._info.next(model);
  }
}
