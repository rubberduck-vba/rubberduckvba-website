import { Component, OnInit } from "@angular/core";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { IndenterViewModel, IndenterViewModelClass } from "../../model/indenter.model";
import { ApiClientService } from "../../services/api-client.service";
import { environment } from "../../../environments/environment";

export interface IndenterOptionGroups {
  isExpanded: boolean;
  isIndentOptionsExpanded: boolean;
  isOutdentOptionsExpanded: boolean;
  isAlignmentOptionsExpanded: boolean;
  isCommentOptionsExpanded: boolean;
  isVerticalOptionsExpanded: boolean;
  isApiAboutBoxExpanded: boolean;
}

@Component({
  selector: 'app-indenter',
  templateUrl: './indenter.component.html',
})
export class IndenterComponent implements OnInit, IndenterOptionGroups {
  private _model!: IndenterViewModel;
  public wasCopied: boolean = false;
  public wasTemplateCopied: boolean = false;


  constructor(fa: FaIconLibrary, private service: ApiClientService) {
    fa.addIconPacks(fas);
  }

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    const localModel = localStorage.getItem('indenter.model');
    const localOptionGroups = localStorage.getItem('indenter.options');
    this.isLocalStorageOK = localModel != null || localOptionGroups != null;

    if (localModel) {
      this.model = <IndenterViewModel>JSON.parse(localModel);
    }
    else {
      this.getDefaults();
    }

    if (localOptionGroups) {
      const optionGroups = <IndenterOptionGroups>JSON.parse(localOptionGroups);
      this.isExpanded = optionGroups.isExpanded;
      this.isIndentOptionsExpanded = optionGroups.isIndentOptionsExpanded;
      this.isOutdentOptionsExpanded = optionGroups.isOutdentOptionsExpanded;
      this.isAlignmentOptionsExpanded = optionGroups.isAlignmentOptionsExpanded;
      this.isCommentOptionsExpanded = optionGroups.isCommentOptionsExpanded;
      this.isVerticalOptionsExpanded = optionGroups.isVerticalOptionsExpanded;
      this.isApiAboutBoxExpanded = optionGroups.isApiAboutBoxExpanded;
    }
  }

  private clearStorage(): void {
    localStorage.removeItem('indenter.model');
    localStorage.removeItem('indenter.options');
   }

  public getDefaults(): void {
    this.service.getIndenterDefaults().subscribe(model => {
      this.model = model;
    });
  }

  private _isExpanded: boolean = false;
  private _isIndentOptionsExpanded: boolean = true;
  private _isOutdentOptionsExpanded: boolean = false;
  private _isAlignmentOptionsExpanded: boolean = false;
  private _isCommentOptionsExpanded: boolean = false;
  private _isVerticalOptionsExpanded: boolean = false;
  private _isApiAboutBoxExpanded: boolean = false;

  public isIndenterBusy: boolean = false;

  private _isLocalStorageOK: boolean = false;
  public get isLocalStorageOK(): boolean {
    return this._isLocalStorageOK;
  }
  public set isLocalStorageOK(value: boolean) {
    this._isLocalStorageOK = value;
    if (!value) {
      this.clearStorage();
    }
    else {
      this.saveModel();
      this.saveOptions();
    }
  }

  public get model(): IndenterViewModel {
    return this._model;
  }

  private set model(value: IndenterViewModel) {
    this._model = value;
    this.invalidateClipboard();
    this.saveModel();
  }

  public get asJson(): string {
    const copy = new IndenterViewModelClass(this._model);
    copy.indentedCode = '';
    return JSON.stringify(copy);
  }

  public get isExpanded(): boolean {
    return this._isExpanded;
  }
  public set isExpanded(value: boolean) {
    this._isExpanded = value;
    this.saveOptions();
  }
  public get isIndentOptionsExpanded(): boolean {
    return this._isIndentOptionsExpanded;
  }
  public set isIndentOptionsExpanded(value: boolean) {
    this._isIndentOptionsExpanded = value;
    this.saveOptions();
  }
  public get isCommentOptionsExpanded(): boolean {
    return this._isCommentOptionsExpanded;
  }
  public set isCommentOptionsExpanded(value: boolean) {
    this._isCommentOptionsExpanded = value;
    this.saveOptions();
  }
  public get isVerticalOptionsExpanded(): boolean {
    return this._isVerticalOptionsExpanded;
  }
  public set isVerticalOptionsExpanded(value: boolean) {
    this._isVerticalOptionsExpanded = value;
    this.saveOptions();
  }
  public get isApiAboutBoxExpanded(): boolean {
    return this._isApiAboutBoxExpanded;
  }
  public set isApiAboutBoxExpanded(value: boolean) {
    this._isApiAboutBoxExpanded = value;
    this.saveOptions();
  }
  public get isOutdentOptionsExpanded(): boolean {
    return this._isOutdentOptionsExpanded;
  }
  public set isOutdentOptionsExpanded(value: boolean) {
    this._isOutdentOptionsExpanded = value;
    this.saveOptions();
  }
  public get isAlignmentOptionsExpanded(): boolean {
    return this._isAlignmentOptionsExpanded;
  }
  public set isAlignmentOptionsExpanded(value: boolean) {
    this._isAlignmentOptionsExpanded = value;
    this.saveOptions();
  }

  private get asOptionGroups(): IndenterOptionGroups {
    return {
      isExpanded: this.isExpanded,
      isIndentOptionsExpanded: this.isIndentOptionsExpanded,
      isAlignmentOptionsExpanded: this.isAlignmentOptionsExpanded,
      isApiAboutBoxExpanded: this.isApiAboutBoxExpanded,
      isCommentOptionsExpanded: this.isCommentOptionsExpanded,
      isOutdentOptionsExpanded: this.isOutdentOptionsExpanded,
      isVerticalOptionsExpanded: this.isVerticalOptionsExpanded
    };
  }

  private saveModel(): void {
    if (this.isLocalStorageOK) {
      localStorage.setItem('indenter.model', JSON.stringify(this.model));
    }
  }
  private saveOptions(): void {
    if (this.isLocalStorageOK) {
      localStorage.setItem('indenter.options', JSON.stringify(this.asOptionGroups));
    }
  }

  public indent(): void {
    this.isIndenterBusy = true;
    this.service.indent(this.model).subscribe(vm => {
      this.model.indentedCode = vm.indentedCode;
      this.model.code = vm.indentedCode;
      this.isIndenterBusy = false;

      this.invalidateClipboard();
      this.saveModel();
      this.saveOptions();
    });
  }

  public copy(): void {
    navigator.clipboard.writeText(this.model.code).then(e => this.wasCopied = true);
  }
  public copyTemplate(): void {
    navigator.clipboard.writeText(this.asJson).then(e => this.wasTemplateCopied = true);
  }

  private invalidateClipboard(): void {
    this.wasCopied = false;
    this.wasTemplateCopied = false;
  }

  public get apiBaseUrl(): string {
    return environment.apiBaseUrl.replace('https://', '');
  }

  public onModelChanged(code: string): void {
    this.model.code = code;
    this.invalidateClipboard();
    this.saveModel();
  }
}
