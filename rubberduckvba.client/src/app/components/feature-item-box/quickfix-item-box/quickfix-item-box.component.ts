import { Component, OnInit, OnChanges, ViewChild, TemplateRef, SimpleChanges, inject, Input } from "@angular/core";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { BehaviorSubject } from "rxjs";
import { XmlDocItemViewModel, QuickFixViewModel, QuickFixViewModelClass } from "../../../model/feature.model";

@Component({
  selector: 'quickfix-item-box',
  templateUrl: './quickfix-item-box.component.html'
})
export class QuickFixItemBoxComponent implements OnInit, OnChanges {

  private readonly _info: BehaviorSubject<XmlDocItemViewModel> = new BehaviorSubject<XmlDocItemViewModel>(null!);
  private readonly _quickFixInfo: BehaviorSubject<QuickFixViewModelClass> = new BehaviorSubject<QuickFixViewModelClass>(null!);

  @ViewChild('quickFixDetails', { read: TemplateRef }) QuickFixDetails: TemplateRef<any> | undefined;

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

      this._quickFixInfo.next(value as QuickFixViewModelClass);
    }
  }

  public get item(): XmlDocItemViewModel {
    return this._info.value;
  }

  public showDetailsModal(): void {
    console.log(`Showing details for QuickFix: ${this.quickFixInfo.name}`);
    this.modal.open(this.QuickFixDetails);
  }

  public get quickFixInfo(): QuickFixViewModelClass {
    return this._quickFixInfo.value;
  }
}
