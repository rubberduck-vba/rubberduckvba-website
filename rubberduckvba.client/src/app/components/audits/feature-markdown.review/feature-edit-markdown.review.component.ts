import { Component, Input, OnInit } from "@angular/core";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { ApiClientService } from "../../../services/api-client.service";
import { BehaviorSubject } from "rxjs";
import { FeatureEditViewModel } from "../../../model/feature.model";
import { Change, diffWords } from "diff";

@Component({
  selector: 'review-edit-markdown',
  templateUrl: './feature-edit-markdown.review.component.html'
})
export class AuditFeatureEditMarkdownComponent implements OnInit {

  private readonly _audit: BehaviorSubject<FeatureEditViewModel> = new BehaviorSubject<FeatureEditViewModel>(null!); 
  private readonly _htmlValue: BehaviorSubject<string> = new BehaviorSubject<string>(null!);
  private readonly _diffSource: BehaviorSubject<string> = new BehaviorSubject<string>(null!);

  constructor(private fa: FaIconLibrary, private api: ApiClientService) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.renderPreview();
  }

  private renderPreview() {
    const markdown = this.showDiff
      ? this.getDiffHtml(this.audit.valueBefore ?? '', this.audit.valueAfter)
      : this.audit.valueAfter;

    this._diffSource.next(markdown);
    this.api.formatMarkdown(markdown).subscribe(md => {
      this._htmlValue.next(md.content);
    });
  }

  @Input()
  public set audit(value: FeatureEditViewModel | undefined) {
    if (value) {
      this._audit.next(value);
    }
  }

  public get audit(): FeatureEditViewModel {
    return this._audit.getValue();
  }

  public get htmlSource(): string {
    return this._diffSource.getValue();
  }

  public get htmlPreview(): string {
    return this._htmlValue.getValue();
  }

  private _showDiff: boolean = true;
  public get showDiff(): boolean {
    return this._showDiff;
  }
  public set showDiff(value: boolean) {
    this._showDiff = value;
    this.renderPreview();
  }

  private _showBefore: boolean = true;
  public get showBefore(): boolean {
    return this._showBefore;
  }

  public set showBefore(value: boolean) {
    this._showBefore = value;
  }

  private getDiffHtml(before: string, after: string): string {
    const diff = diffWords(before, after, { ignoreCase: false });
    return diff.map((part: Change) => {
      if (part.added) {
        return `<span class="text-diff-added">${part.value}</span>`;
      } else if (part.removed) {
        return `<span class="text-diff-removed">${part.value}</span>`;
      } else {
        return part.value;
      }
    }).join('');
  }
}
