import { Component, Input, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { DownloadInfo } from "../../model/downloads.model";
import { BehaviorSubject } from 'rxjs';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
    selector: 'download-item',
    templateUrl: './download-item.component.html',
    standalone: false
})
export class DownloadItemComponent implements OnInit {

  private readonly _downloadInfo: BehaviorSubject<DownloadInfo> = new BehaviorSubject<DownloadInfo>(null!);

  @ViewChild('content', { read: TemplateRef }) content: TemplateRef<any> | undefined;
  public modal = inject(NgbModal);

  @Input()
  public set info(value: DownloadInfo | undefined) {
    if (value != null) {
      this._downloadInfo.next(value);
    }
  }

  public get info(): DownloadInfo | undefined {
    return this._downloadInfo.value;
  }

  ngOnInit(): void {
  }

  public confirmDownload(): void {
    this.modal.open(this.content);
  }
}
