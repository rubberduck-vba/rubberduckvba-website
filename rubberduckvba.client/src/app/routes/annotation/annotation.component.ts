import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { BehaviorSubject, switchMap } from "rxjs";
import { AnnotationViewModel } from "../../model/feature.model";
import { ApiClientService } from "../../services/api-client.service";

@Component({
  selector: 'app-annotation',
  templateUrl: './annotation.component.html',
})
export class AnnotationComponent implements OnInit {

  private readonly _info: BehaviorSubject<AnnotationViewModel> = new BehaviorSubject<AnnotationViewModel>(null!);

  public set info(value: AnnotationViewModel) {
    this._info.next(value);
  }
  public get info(): AnnotationViewModel {
    return this._info.getValue();
  }

  constructor(private api: ApiClientService, private fa: FaIconLibrary, private route: ActivatedRoute) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const name = params.get('name')!;
        return this.api.getAnnotation(name);
      })).subscribe(e => {
        this.info = <AnnotationViewModel>e;
      });
  }
}
