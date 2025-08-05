import { Component, Input, OnChanges, OnInit, SimpleChanges, TemplateRef, ViewChild, inject } from '@angular/core';
import { AnnotationViewModel, XmlDocItemViewModel } from '../../../model/feature.model';
import { BehaviorSubject } from 'rxjs';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
    selector: 'annotation-item-box',
    templateUrl: './annotation-item-box.component.html',
    standalone: false
})
export class AnnotationItemBoxComponent implements OnInit, OnChanges {

  private readonly _info: BehaviorSubject<XmlDocItemViewModel> = new BehaviorSubject<XmlDocItemViewModel>(null!);
  private readonly _annotationInfo: BehaviorSubject<AnnotationViewModel> = new BehaviorSubject<AnnotationViewModel>(null!);

  @ViewChild('annotationDetails', { read: TemplateRef }) annotationDetails: TemplateRef<any> | undefined;

  constructor(private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
  }

  @ViewChild('content', { read: TemplateRef }) content: TemplateRef<any> | undefined;
  public modal = inject(NgbModal);

  @Input()
  public set item(value: XmlDocItemViewModel) {
    if (value != null) {
      this._info.next(value);

      this._annotationInfo.next(value as AnnotationViewModel);
    }
  }

  public get item(): XmlDocItemViewModel {
    return this._info.value;
  }

  public showDetailsModal(): void {
    console.log(`Showing details for annotation: ${this.annotationInfo.name}`);
    this.modal.open(this.annotationDetails, { modalDialogClass: this.annotationInfo.parameters.length > 0 || this.annotationInfo.examples.length > 0 ? 'modal-xl' : 'modal-l' });
  }

  public get annotationInfo(): AnnotationViewModel {
    return this._annotationInfo.value;
  }
}
