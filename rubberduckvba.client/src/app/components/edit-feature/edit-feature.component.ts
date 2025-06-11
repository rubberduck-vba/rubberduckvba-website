import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, TemplateRef, ViewChild, inject, input } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { EditSubFeatureViewModelClass, MarkdownContent, SubFeatureViewModel, SubFeatureViewModelClass, UserViewModel } from "../../model/feature.model";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { ApiClientService } from "../../services/api-client.service";

export enum AdminAction {
  Edit = 'edit',
  EditSummary = 'summary',
  Create = 'create',
  Delete = 'delete',
}

@Component({
  selector: 'edit-feature',
  templateUrl: './edit-feature.component.html'
})
export class EditFeatureComponent implements OnInit, OnChanges {
  private readonly _user: BehaviorSubject<UserViewModel> = new BehaviorSubject<UserViewModel>(null!);
  private readonly _feature: BehaviorSubject<EditSubFeatureViewModelClass> = new BehaviorSubject<EditSubFeatureViewModelClass>(null!);

  private _action: AdminAction = AdminAction.Create;

  @ViewChild('editModal', { read: TemplateRef }) editModal: TemplateRef<any> | undefined;
  @ViewChild('deleteModal', { read: TemplateRef }) deleteModal: TemplateRef<any> | undefined;

  public modal = inject(NgbModal);

  @Input()
  public set feature(value: SubFeatureViewModel | undefined) {
    if (value != null) {
      this._feature.next(new EditSubFeatureViewModelClass(value));
    }
  }

  public get feature(): EditSubFeatureViewModelClass {
    return this._feature.value;
  }

  @Input()
  public set action(value: AdminAction) {
    this._action = value;
  }

  public get action(): AdminAction {
    return this._action;
  }

  @Output()
  public onApplyChanges = new EventEmitter<SubFeatureViewModel>();


  constructor(private fa: FaIconLibrary, private api: ApiClientService) {
    fa.addIconPacks(fas);
  }

  ngOnChanges(changes: SimpleChanges): void {
  }

  ngOnInit(): void {
  }

  public doAction(): void {
    const localModal = this.action == 'delete' ? this.deleteModal : this.editModal;
    const size = this.action == 'delete' ? 'modal-m' : 'modal-xl';
    this.modal.open(localModal, { modalDialogClass: size });
  }

  public onConfirmChanges(): void {
    this.modal.dismissAll();
    this.api.saveFeature(this.feature).subscribe(saved => {
      this._feature.next(new EditSubFeatureViewModelClass(saved));
      this.onApplyChanges.emit(saved)
    });
  }

  public onPreviewDescription(): void {
    this.api.formatMarkdown(this.feature.description).subscribe((formatted: MarkdownContent) => {
      this.feature.descriptionPreview = formatted.content;
    });
  }

  public onDeleteFeature(): void {
    this.modal.dismissAll();
    this.api.deleteFeature(this.feature).subscribe(() => {
      this._feature.next(new EditSubFeatureViewModelClass(null!));
    });
  }
}
