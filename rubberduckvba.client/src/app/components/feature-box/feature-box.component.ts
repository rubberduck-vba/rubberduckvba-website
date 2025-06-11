import { Component, Input, OnChanges, OnInit, SimpleChanges, TemplateRef, ViewChild, inject } from '@angular/core';
import { FeatureViewModel, QuickFixViewModel, SubFeatureViewModel, UserViewModel } from '../../model/feature.model';
import { BehaviorSubject } from 'rxjs';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AuthService } from '../../services/auth.service';
import { AdminAction } from '../edit-feature/edit-feature.component';

@Component({
  selector: 'feature-box',
  templateUrl: './feature-box.component.html'
})
export class FeatureBoxComponent implements OnInit, OnChanges {

  private readonly _info: BehaviorSubject<FeatureViewModel> = new BehaviorSubject<FeatureViewModel>(null!);
  private readonly _user: BehaviorSubject<UserViewModel> = new BehaviorSubject<UserViewModel>(null!);

  @ViewChild('content', { read: TemplateRef }) content: TemplateRef<any> | undefined;
  public modal = inject(NgbModal);

  @Input()
  public parentFeatureName: string = '';

  @Input()
  public hasOwnDetailsPage: boolean = false;

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

  public get feature(): FeatureViewModel | undefined {
    return this._info.value as FeatureViewModel;
  }

  public get subFeature(): SubFeatureViewModel | undefined {
    return this._info.value as SubFeatureViewModel;
  }

  constructor(private fa: FaIconLibrary, private auth: AuthService) {
    fa.addIconPacks(fas);
  }
  ngOnChanges(changes: SimpleChanges): void {
    console.log(changes);
  }

  ngOnInit(): void {
    this.auth.getUser().subscribe(vm => {
      this._user.next(vm);
    });
  }

  public applyChanges(model: any): void {
    this._info.next(model);
  }

  public showDetails(): void {
    this.modal.open(this.content);
  }

  public get user(): UserViewModel {
    return this._user.getValue();
  }
}
