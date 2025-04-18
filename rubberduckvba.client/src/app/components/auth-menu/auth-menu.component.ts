import { Component, OnInit, TemplateRef, ViewChild, inject } from "@angular/core";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { BehaviorSubject } from "rxjs";
import { UserViewModel } from "../../model/feature.model";
import { AuthService } from "src/app/services/auth.service";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { fab } from "@fortawesome/free-brands-svg-icons";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { ApiClientService } from "../../services/api-client.service";

@Component({
  selector: 'auth-menu',
  templateUrl: './auth-menu.component.html'
})
export class AuthMenuComponent implements OnInit {

  private readonly _user: BehaviorSubject<UserViewModel> = new BehaviorSubject<UserViewModel>(null!);
  
  public set user(value: UserViewModel) {
    this._user.next(value);
  }
  public get user(): UserViewModel {
    return this._user.getValue();
  }

  @ViewChild('confirmbox', { read: TemplateRef }) confirmbox: TemplateRef<any> | undefined;
  @ViewChild('confirmtagsbox', { read: TemplateRef }) confirmtagsbox: TemplateRef<any> | undefined;
  @ViewChild('confirmxmldocsbox', { read: TemplateRef }) confirmxmldocsbox: TemplateRef<any> | undefined;
  @ViewChild('confirmclearcachebox', { read: TemplateRef }) confirmclearcachebox: TemplateRef<any> | undefined;
  public modal = inject(NgbModal);

  constructor(private auth: AuthService, private api: ApiClientService, private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
    fa.addIconPacks(fab);
  }

  ngOnInit(): void {
    this.getUserInfo();
  }

  getUserInfo(): void {
    this.auth.getUser().subscribe(result => {
      if (result) {
        this._user.next(result);
      }
    });
  }

  public confirm(): void {
    this.modal.open(this.confirmbox);
  }

  public confirmUpdateTags(): void {
    this.modal.open(this.confirmtagsbox);
  }

  public confirmUpdateXmldocs(): void {
    this.modal.open(this.confirmxmldocsbox);
  }

  public confirmClearCache(): void {
    this.modal.open(this.confirmclearcachebox);
  }

  public signin(): void {
    this.auth.signin();
    this.getUserInfo();
  }

  public signout(): void {
    this.auth.signout();
    this.getUserInfo();
  }

  public updateTags(): void {
    this.modal.dismissAll();
    this.api.updateTagMetadata().subscribe(jobId => console.log(`UpdateTagMetadata has scheduled job id ${jobId}`));
  }

  public updateXmldocs(): void {
    this.modal.dismissAll();
    this.api.updateXmldocMetadata().subscribe(jobId => console.log(`UpdateXmldocMetadata has scheduled job id ${jobId}`));
  }

  public clearCache(): void {
    this.modal.dismissAll();
    this.api.clearCache().subscribe(() => console.log(`Cache has been cleared`));
  }
}
