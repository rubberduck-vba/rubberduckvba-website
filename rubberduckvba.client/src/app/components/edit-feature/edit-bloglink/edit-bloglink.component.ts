import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { BlogLink } from "../../../model/feature.model";

@Component({
    selector: 'edit-blog-link',
    templateUrl: './edit-bloglink.component.html',
    standalone: false
})
export class EditBlogLinkComponent implements OnInit {

  private _blogLink: BlogLink = null!;

  constructor() {

  }

  ngOnInit(): void {
  }

  @Output()
  public removeLink = new EventEmitter<BlogLink>();

  @Input()
  public set blogLink(value: BlogLink) {
    this._blogLink = value;
  }

  public get blogLink(): BlogLink {
    return this._blogLink;
  }

  public onRemove(): void {
    this.removeLink.emit(this.blogLink);
  }
}
