import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChange, SimpleChanges, TemplateRef, ViewChild, inject } from '@angular/core';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { fab } from '@fortawesome/free-brands-svg-icons';
import { Tag, TagDownloadInfo } from '../../model/tags.model';
import { BehaviorSubject } from 'rxjs';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AngularDeviceInformationService } from 'angular-device-information';

@Component({
  //standalone: true,
  selector: 'loading-content',
  templateUrl: './loading-content.component.html'
})
export class LoadingContentComponent {
  @Input() public show: boolean = true;
}
