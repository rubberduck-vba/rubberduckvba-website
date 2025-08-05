import { Component, Input, OnInit } from "@angular/core";
import { BlogLink } from "../../model/feature.model";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { fab } from "@fortawesome/free-brands-svg-icons";
import { BehaviorSubject } from "rxjs";

@Component({
    selector: 'blog-link-box',
    templateUrl: './blog-link-box.component.html',
    standalone: false
})
export class BlogLinkBoxComponent implements OnInit {

  private readonly _info: BehaviorSubject<BlogLink> = new BehaviorSubject<BlogLink>(null!);

  @Input()
  public parentFeatureItemName: string = '';

  @Input()
  public set link(value: BlogLink | undefined) {
    if (value != null) {
      this._info.next(value);
    }
  }

  public get link(): BlogLink | undefined {
    return this._info.value as BlogLink;
  }



  constructor(private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
    fa.addIconPacks(fab);
  }

  ngOnInit(): void {
  }
}
