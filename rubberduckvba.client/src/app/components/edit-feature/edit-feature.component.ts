import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, TemplateRef, ViewChild, inject, input } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { BlogLink, BlogLinkViewModelClass, EditSubFeatureViewModelClass, MarkdownContent, SubFeatureViewModel, SubFeatureViewModelClass, UserViewModel } from "../../model/feature.model";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { ApiClientService } from "../../services/api-client.service";

export enum AdminAction {
  Edit = 'edit',
  EditSummary = 'summary',
  EditLinks = 'links',
  Create = 'create',
  Delete = 'delete',
}

@Component({
  selector: 'edit-feature',
  templateUrl: './edit-feature.component.html'
})
export class EditFeatureComponent implements OnInit {
  private readonly _feature: BehaviorSubject<EditSubFeatureViewModelClass> = new BehaviorSubject<EditSubFeatureViewModelClass>(null!);

  private _action: AdminAction = AdminAction.Create;

  @ViewChild('editModal', { read: TemplateRef }) editModal: TemplateRef<any> | undefined;
  @ViewChild('createModal', { read: TemplateRef }) createModal: TemplateRef<any> | undefined;
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

  private _disabled: boolean = false;

  @Input()
  public set disabled(value: boolean) {
    this._disabled = value;
  }

  public get disabled(): boolean {
    return this._disabled;
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


  public subfeature: EditSubFeatureViewModelClass = null!;

  constructor(private fa: FaIconLibrary, private api: ApiClientService) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
  }

  public doAction(): void {
    const localModal = this.action == 'delete' ? this.deleteModal
                      : this.action == 'create' ? this.createModal
                      : this.editModal;
    const size = this.action == 'delete' ? 'modal-m' : 'modal-xl';

    if (this.action == 'create') {
      const parentId = this.feature.id;
      const parentName = this.feature.name;
      const parentTitle = this.feature.title;

      this.subfeature = new EditSubFeatureViewModelClass({
        dateInserted: '',
        dateUpdated: '',
        description: '',
        id: undefined,
        isHidden: false,
        isNew: false,
        name: 'NewFeature1',
        title: 'New Feature',
        shortDescription: '',
        featureId: parentId,
        featureName: parentName,
        featureTitle: parentTitle,
        isCollapsed: false,
        isDetailsCollapsed: true,
        links: []
      });
    }

    this.modal.open(localModal, { modalDialogClass: size });
  }

  public onConfirmChanges(): void {
    this.modal.dismissAll();    
    this.api.saveFeature(this.feature).subscribe(saved => {
      window.location.reload();
    });
  }

  public onConfirmCreate(): void {
    this.modal.dismissAll();
    this.api.createFeature(this.subfeature).subscribe(saved => {
      window.location.reload();
    });
  }

  public onPreviewDescription(): void {
    const raw = this.action == 'create'
      ? this.subfeature.description
      : this.feature.description;
    this.api.formatMarkdown(raw).subscribe((formatted: MarkdownContent) => {
      if (this.action == 'create') {
        this.subfeature.descriptionPreview = formatted.content;
      }
      else {
        this.feature.descriptionPreview = formatted.content;
      }
    });
  }

  public onDeleteFeature(): void {
    this.modal.dismissAll();
    this.api.deleteFeature(this.feature).subscribe(() => {
      window.location.reload();
    });
  }

  public onRemoveLink(link: BlogLink): void {
    this.feature.links = this.feature.links!.filter(e => e.name.length > 0 && e.url.length > 0 && (e.name != link.name || e.url != link.url));
  }

  public onAddLink(): void {
    this.feature.links.push(new BlogLinkViewModelClass({
      name: 'Title',
      author: 'Author',
      published: new Date().toISOString().substring(0, 10),
      url: 'https://rubberduckvba.blog/...'
    }));
  }
}
