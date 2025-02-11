import { Component, OnInit } from "@angular/core";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { IndenterViewModel, IndenterViewModelClass } from "../../model/indenter.model";
import { ApiClientService } from "../../services/api-client.service";

@Component({
  selector: 'app-indenter',
  templateUrl: './indenter.component.html',
})
export class IndenterComponent implements OnInit {
  private _model!: IndenterViewModel;
  public wasCopied: boolean = false;


  constructor(fa: FaIconLibrary, private service: ApiClientService) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.service.getIndenterDefaults().subscribe(model => {
      this._model = model;
    });
  }

  public isExpanded: boolean = false;
  public isIndentOptionsExpanded: boolean = true;
  public isOutdentOptionsExpanded: boolean = false;
  public isAlignmentOptionsExpanded: boolean = false;
  public isCommentOptionsExpanded: boolean = false;
  public isVerticalOptionsExpanded: boolean = false;
  public isApiAboutBoxExpanded: boolean = false;

  public isIndenterBusy: boolean = false;

  public get model(): IndenterViewModel {
    return this._model;
  }

  public get asJson(): string {
    const copy = new IndenterViewModelClass(this._model);
    copy.indentedCode = '';
    return JSON.stringify(copy);
  }

  public indent(): void {
    this.isIndenterBusy = true;
    this.service.indent(this.model).subscribe(vm => {
      this.model.indentedCode = vm.indentedCode;
      this.model.code = vm.indentedCode;
      this.isIndenterBusy = false;
    });
  }

  public clear(): void {
    this.model.code = '';
  }

  public copy(): void {
    navigator.clipboard.writeText(this.model.code).then(e => this.wasCopied = true);
  }

  public onModelChanged(code: string): void {
    this.model.code = code;
    this.wasCopied = false;
  }
}
