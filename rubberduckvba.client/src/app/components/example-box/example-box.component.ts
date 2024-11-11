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

  @Input()
  public parentFeatureItemName: string = '';

  @Input()
  public set inspectionExample(value: XmlDocExample | undefined) {
    if (value != null) {
      this._info.next(value);
    }
  }

  public get inspectionExample(): InspectionExample | undefined {
    return this._info.value as InspectionExample;
  }

  constructor(private fa: FaIconLibrary, private api: ApiClientService) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
  }

//  public showDetails(): void {
//    this.modal.open(this.content);
//  }
}
