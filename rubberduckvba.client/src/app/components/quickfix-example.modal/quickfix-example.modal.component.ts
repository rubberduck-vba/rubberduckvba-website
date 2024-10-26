import { Component, Input, OnChanges, OnInit, SimpleChanges, TemplateRef, ViewChild, inject } from '@angular/core';
import { AnnotationFeatureItem, AnnotationInfo, FeatureItem, FeatureItemViewModel, InspectionInfo, QuickFixInfo, QuickFixViewModel } from '../../model/feature.model';
import { BehaviorSubject } from 'rxjs';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'quickfix-example',
  templateUrl: './quickfix-example.modal.component.html'
})
export class FeatureItemExampleComponent implements OnInit, OnChanges {

  private readonly _quickfix: BehaviorSubject<FeatureItem> = new BehaviorSubject<FeatureItem>(null!);

  constructor(private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
  }

  @ViewChild('content', { read: TemplateRef }) content: TemplateRef<any> | undefined;
  public modal = inject(NgbModal);
  public modalQuickFixVM: QuickFixViewModel = null!;


  @Input()
  public set quickFix(value: FeatureItem) {
    if (value != null) {
      this._quickfix.next(value);
    }
  }

  public get quickFix(): FeatureItem {
    return this._quickfix.value!;
  }

  public showModal(): void {
    this.modal.open(this.content);
  }
}
