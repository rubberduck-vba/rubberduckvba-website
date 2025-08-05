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
    selector: 'tag-download',
    templateUrl: './tag-download.component.html',
    standalone: false
})
export class TagDownloadComponent implements OnInit, OnDestroy, OnChanges {

  private readonly tagInfo: BehaviorSubject<Tag> = new BehaviorSubject<Tag>(null!);

  public canDownload: boolean;

  public modal = inject(NgbModal);

  public info: TagDownloadInfo | undefined;

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  @ViewChild('content', { read: TemplateRef }) content: TemplateRef<any> | undefined;

  @Input()
  public set tag(value: Tag | undefined) {
    if (value != null) {
      this.tagInfo.next(value);
      this.info = new TagDownloadInfo(value);
    }
  }

  public get tag(): Tag | undefined {
    return this.tagInfo.value;
  }

  @Input()
  public text: string = 'Download it here:';

  constructor(fa: FaIconLibrary, private platform: AngularDeviceInformationService) {
    fa.addIconPacks(fas);
    fa.addIconPacks(fab);
    this.canDownload = platform.isDesktop() && platform.getDeviceInfo().os == 'Windows';
  }

  ngOnInit(): void {
  }

  ngOnDestroy(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['tag'] != null) {
      const change: SimpleChange = changes['tag'];
      if (change.currentValue != null) {
        this.tagInfo.next(change.currentValue as Tag);
      }
    }
  }

  public confirmDownload(): void {
    this.modal.open(this.content);
  }
}
