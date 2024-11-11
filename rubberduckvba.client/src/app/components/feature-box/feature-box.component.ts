import { Component, Input, OnChanges, OnInit, SimpleChanges, TemplateRef, ViewChild, inject } from '@angular/core';
import { FeatureViewModel, QuickFixViewModel, SubFeatureViewModel } from '../../model/feature.model';
import { BehaviorSubject } from 'rxjs';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'feature-box',
  templateUrl: './feature-box.component.html'
})
export class FeatureBoxComponent implements OnInit {

  private readonly _info: BehaviorSubject<FeatureViewModel> = new BehaviorSubject<FeatureViewModel>(null!);

  @ViewChild('content', { read: TemplateRef }) content: TemplateRef<any> | undefined;
  public modal = inject(NgbModal);

  @Input()
  public parentFeatureName: string = '';

  @Input()
  public hasOwnDetailsPage: boolean = false;

  @Input()
  public set feature(value: FeatureViewModel | undefined) {
    if (value != null) {
      this._info.next(value);
    }
  }

  public get feature(): FeatureViewModel | undefined {
    return this._info.value as FeatureViewModel;
  }

  public get subFeature(): SubFeatureViewModel | undefined {
    return this._info.value as SubFeatureViewModel;
  }

  private readonly _quickfixes: BehaviorSubject<QuickFixViewModel[]> = new BehaviorSubject<QuickFixViewModel[]>(null!);

  @Input()
  public set quickFixes(value: QuickFixViewModel[]) {
    if (value != null) {
      this._quickfixes.next(value);
    }
  }

  public get quickFixes(): QuickFixViewModel[] {
    return this._quickfixes.value;
  }

  constructor(private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
  }

  public showDetails(): void {
    this.modal.open(this.content);
  }
}
