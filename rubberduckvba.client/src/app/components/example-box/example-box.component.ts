import { Component, Input, OnChanges, OnInit, SimpleChanges, TemplateRef, ViewChild, inject } from '@angular/core';
import { Example, InspectionExample, ExampleModule, AnnotationExample, QuickFixExample, XmlDocExample } from '../../model/feature.model';
import { BehaviorSubject } from 'rxjs';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ApiClientService } from '../../services/api-client.service';

@Component({
  selector: 'example-box',
  templateUrl: './example-box.component.html',
  styleUrls: ['./example-box.component.css']
})
export class ExampleBoxComponent implements OnInit {

  private readonly _info: BehaviorSubject<XmlDocExample> = new BehaviorSubject<XmlDocExample>(null!);

  //@ViewChild('content', { read: TemplateRef }) content: TemplateRef<any> | undefined;
  //public modal = inject(NgbModal);

  private _isInspectionExample: boolean = false;
  private _isAnnotationExample: boolean = false;
  private _isQuickFixExample: boolean = false;

  @Input()
  public set inspectionExample(value: XmlDocExample | undefined) {
    if (value != null) {
      this._info.next(value);
      this._isInspectionExample = true;
    }
  }

  public get inspectionExample(): InspectionExample | undefined {
    return this._info.value as InspectionExample;
  }

  @Input()
  public set annotationExample(value: XmlDocExample | undefined) {
    if (value != null) {
      this._info.next(value);
      this._isAnnotationExample = true;
    }
  }

  public get annotationExample(): AnnotationExample | undefined {
    return this._info.value as AnnotationExample;
  }

  @Input()
  public set quickFixExample(value: XmlDocExample | undefined) {
    if (value != null) {
      this._info.next(value);
      this._isQuickFixExample = true;
    }
  }

  public get quickFixExample(): QuickFixExample | undefined {
    return this._info.value as QuickFixExample;
  }

  public get isInspection(): boolean {
    return this._isInspectionExample;
  }

  public get isAnnotation(): boolean {
    return this._isAnnotationExample;
  }
  public get isQuickFix(): boolean {
    return this._isQuickFixExample;
  }

  constructor(private fa: FaIconLibrary, private api: ApiClientService) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
  }

  public getModuleIconClass(module: ExampleModule): string {
    switch (module.moduleTypeName) {
      case "Class Module":
        return "icon-class-module";
      case "Document Module":
        return "icon-document-module";
      case "Interface Module":
        return "icon-interface-module";
      case "Predeclared Class":
        return "icon-predeclared-class";
      case "UserForm Module":
        return "icon-userform-module";
      default:
        return "icon-standard-module";
    }
  }
}
