import { Component, Input, OnChanges, OnInit, SimpleChanges, TemplateRef, ViewChild, inject } from '@angular/core';
import { AnnotationFeatureItem, AnnotationInfo, FeatureItem, FeatureItemViewModel, InspectionInfo, QuickFixInfo, QuickFixViewModel } from '../../model/feature.model';
import { BehaviorSubject } from 'rxjs';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'feature-item-box',
  templateUrl: './feature-item-box.component.html'
})
export class FeatureItemBoxComponent implements OnInit, OnChanges {

  private readonly _info: BehaviorSubject<FeatureItemViewModel> = new BehaviorSubject<FeatureItemViewModel>(null!);
  private readonly _inspectionInfo: BehaviorSubject<InspectionInfo> = new BehaviorSubject<InspectionInfo>(null!);
  private readonly _quickfixInfo: BehaviorSubject<QuickFixInfo> = new BehaviorSubject<QuickFixInfo>(null!);
  private readonly _annotationInfo: BehaviorSubject<AnnotationInfo> = new BehaviorSubject<AnnotationInfo>(null!);

  private readonly _quickfixes: BehaviorSubject<FeatureItem[]> = new BehaviorSubject<FeatureItem[]>(null!);
  private _quickfixMap: Map<string, FeatureItem> = null!;

  constructor(private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
  }

  @ViewChild('content', { read: TemplateRef }) content: TemplateRef<any> | undefined;
  public modal = inject(NgbModal);
  public quickFixVM: QuickFixViewModel = null!;


  @Input()
  public set quickFixes(value: any[]) {
    if (value != null) {
      this._quickfixes.next(value);
      this._quickfixMap = new Map(this._quickfixes.value.map(e => [e.name, this.getQuickFixItem(e)]));
    }
  }

  private getQuickFixItem(item: FeatureItem): FeatureItem {
    item.title = item.name.replace('QuickFix', '');
    
    return item;
  }

  public get quickFixes(): FeatureItem[] {
    return this._quickfixes.value;
  }

  @Input()
  public set item(value: FeatureItemViewModel) {
    if (value != null) {
      this._info.next(value);

      if (value.featureName == 'Inspections') {
        this._inspectionInfo.next(JSON.parse(value.serialized));
        this.isInspectionInfo = true;
        if (this.item.isNew) {
          console.log(this.item);
        }
      }

      if (value.featureName == 'QuickFixes') {
        this._quickfixInfo.next(value.info as QuickFixInfo);
        this.isQuickfixInfo = true;
      }

      if (value.featureName == 'Annotations') {
        this._annotationInfo.next(value.info as AnnotationInfo);
        this.isAnnotationInfo = true;
      }
    }
  }

  public get item(): FeatureItemViewModel {
    return this._info.value;
  }

  public getQuickFix(name: string): FeatureItem {
    if (this._quickfixMap) {
      return this._quickfixMap.get(name)!;
    }
    return null!;
  }

  public showQuickFixModal(name: string): void {
    this.quickFixVM = new QuickFixViewModel(this.getQuickFix(name)!);
    this.modal.open(this.content)
  }

  public getQuickFixSummary(name: string): string {
    return this.getQuickFix(name).summary;
  }

  isInspectionInfo: boolean = false;
  public get inspectionInfo(): InspectionInfo | undefined {
    if (!this.isInspectionInfo) {
      return undefined;
    }
    return this._inspectionInfo.value;
  }

  public get severityTitle(): string {
    switch (this.inspectionInfo?.DefaultSeverity) {
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

  isQuickfixInfo: boolean = false;
  public get quickfixInfo(): QuickFixInfo | undefined {
    if (!this.isQuickfixInfo) {
      return undefined;
    }
    return this._quickfixInfo.value;
  }

  isAnnotationInfo: boolean = false;
  public get annotationInfo(): AnnotationInfo | undefined {
    if (!this.isAnnotationInfo) {
      return undefined;
    }
    return this._annotationInfo.value;
  }
}
