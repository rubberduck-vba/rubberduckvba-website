import { Component, OnInit } from "@angular/core";
import { InspectionViewModel } from "../../model/feature.model";
import { ActivatedRoute, Router } from "@angular/router";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { BehaviorSubject, switchMap } from "rxjs";
import { ApiClientService } from "../../services/api-client.service";

@Component({
    selector: 'app-inspection',
    templateUrl: './inspection.component.html',
    standalone: false
})
export class InspectionComponent implements OnInit {

  private readonly _info: BehaviorSubject<InspectionViewModel> = new BehaviorSubject<InspectionViewModel>(null!);

  public set info(value: InspectionViewModel) {
    this._info.next(value);
  }
  public get info(): InspectionViewModel {
    return this._info.getValue();
  }

  constructor(private api: ApiClientService, private fa: FaIconLibrary, private route: ActivatedRoute, private router: Router) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const name = params.get('name')!;
        return this.api.getInspection(name);
      })).subscribe(e => {
        console.log(e);
        this.info = <InspectionViewModel>e;
      });
  }

  public get severityIconClass(): string {
    return `icon icon-severity-${this.info.defaultSeverity.toLowerCase()}`;
  }

  public get severityTitle(): string {
    switch (this.info?.defaultSeverity) {
      case 'DoNotShow':
        return 'Inspections at this severity level are disabled until/unless configured differently.';
      case 'Hint':
        return 'Inspections at this severity level are merely making an observation about the code.';
      case 'Suggestion':
        return 'Inspections at this severity level are making an actionnable observation about the code.';
      case 'Warning':
        return 'Inspections at this severity level are flagging a potential issue that is usually more serious than a simple observation.';
      case 'Error':
        return 'Inspections at this severity level are flagging a potential bug, or a possible run-time or compile-time error.';
      default:
        return '';
    }
  }

  public get inspectionTypeTitle(): string {
    switch (this.info?.inspectionType) {
      case 'Code Quality Issues':
        return 'Inspections of this type indicate a problem (real or potential) with the code.';
      case 'Naming and Convention Issues':
        return 'Inspections of this type point out issues that arise out of naming style and programming conventions.';
      case 'Language Opportunities':
        return 'Inspections of this type indicate a probable misuse of a language feature that exists for backward compatibility.';
      case 'Rubberduck Opportunities':
        return 'Inspections of this type relate specifically to Rubberduck features.';
      default:
        return 'Uncategorized, I guess.';
    }
  }
}
