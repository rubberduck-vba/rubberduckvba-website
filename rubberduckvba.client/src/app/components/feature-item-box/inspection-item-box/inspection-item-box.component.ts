import { Component, Input, OnChanges, OnInit, SimpleChanges, TemplateRef, ViewChild, inject } from '@angular/core';
import { AnnotationViewModel, InspectionViewModel, QuickFixViewModel, XmlDocItemViewModel } from '../../../model/feature.model';
import { BehaviorSubject } from 'rxjs';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ApiClientService } from '../../../services/api-client.service';

@Component({
  selector: 'inspection-item-box',
  templateUrl: './inspection-item-box.component.html'
})
export class InspectionItemBoxComponent implements OnInit, OnChanges {

  private readonly _info: BehaviorSubject<XmlDocItemViewModel> = new BehaviorSubject<XmlDocItemViewModel>(null!);
  private readonly _inspectionInfo: BehaviorSubject<InspectionViewModel> = new BehaviorSubject<InspectionViewModel>(null!);

  private quickFixVM: QuickFixViewModel = null!;

  constructor(private fa: FaIconLibrary, private api: ApiClientService) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
  }

  @ViewChild('inspectionDetails', { read: TemplateRef }) inspectionDetails: TemplateRef<any> | undefined;

  @ViewChild('content', { read: TemplateRef }) content: TemplateRef<any> | undefined;
  public modal = inject(NgbModal);

  @Input()
  public set item(value: XmlDocItemViewModel) {
    if (value != null) {
      this._info.next(value);
      this._inspectionInfo.next(value as InspectionViewModel);
    }
  }

  public get item(): XmlDocItemViewModel {
    return this._info.value;
  }

  public showDetailsModal(): void {
      this.api.getInspection(this.inspectionInfo.name).subscribe((inspection: InspectionViewModel) => {
        this._inspectionInfo.next(inspection);
        this.modal.open(this.inspectionDetails, { modalDialogClass: 'modal-xl'});
      });
  }

  public showQuickFixModal(name: string): void {
    this.quickFixVM = this.inspectionInfo.quickFixes.find(e => e.title == name)!;
    this.modal.open(this.content)
  }

  isInspectionInfo: boolean = false;
  public get inspectionInfo(): InspectionViewModel {
    return this._inspectionInfo.value;
  }

  public get severityIconClass(): string {
    return `icon icon-severity-${this.inspectionInfo.defaultSeverity.toLowerCase()}`;
  }

  public get severityTitle(): string {
    switch (this.inspectionInfo?.defaultSeverity) {
      case 'DoNotShow':
        return 'Inspections at this severity level are disabled until/unless configured differently.';
      case 'Hint':
        return 'Inspections at this severity level are merely making an observation about the code.';
      case 'Suggestion':
        return 'Inspections at this severity level are making an actionnable observation about the code.';
      case 'Warning':
        return 'Inspections at this severity level are flagging a potential issue that is usually more serious than a simple observation.';
      case 'Error':
        return 'Inspections at this severity level are flagging a potential bug, or a possible run-time or compile-time error.';
      default:
        return '';
    }
  }

  public get inspectionTypeTitle(): string {
    switch (this.inspectionInfo?.inspectionType) {
      case 'Code Quality Issues':
        return 'Inspections of this type indicate a problem (real or potential) with the code.';
      case 'Naming and Convention Issues':
        return 'Inspections of this type point out issues that arise out of naming style and programming conventions.';
      case 'Language Opportunities':
        return 'Inspections of this type indicate a probable misuse of a language feature that exists for backward compatibility.';
      case 'Rubberduck Opportunities':
        return 'Inspections of this type relate specifically to Rubberduck features.';
      default:
        return 'Uncategorized, I guess.';
    }
  }
}
