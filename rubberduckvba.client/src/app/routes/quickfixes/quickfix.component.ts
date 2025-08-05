import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { BehaviorSubject, switchMap } from "rxjs";
import { QuickFixViewModelClass } from "../../model/feature.model";
import { ApiClientService } from "../../services/api-client.service";

@Component({
    selector: 'app-quickfix',
    templateUrl: './quickfix.component.html',
    standalone: false
})
export class QuickFixComponent implements OnInit {

  private readonly _info: BehaviorSubject<QuickFixViewModelClass> = new BehaviorSubject<QuickFixViewModelClass>(null!);

  public set info(value: QuickFixViewModelClass) {
    this._info.next(value);
  }
  public get info(): QuickFixViewModelClass {
    return this._info.getValue();
  }

  constructor(private api: ApiClientService, private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap((params :any) => {
        const name = params.get('name')!;
        return this.api.getQuickFix(name);
      })).subscribe((e :any) => {
        this.info = <QuickFixViewModelClass>e;
        console.log(this.info);
      });
  }
}
