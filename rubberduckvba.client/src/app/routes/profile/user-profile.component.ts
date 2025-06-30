import { Component, OnInit } from "@angular/core";
import { UserActivityItem, UserActivityItemClass, UserActivityType, UserViewModel } from "../../model/feature.model";
import { AuthService } from "../../services/auth.service";
import { BehaviorSubject } from "rxjs";
import { FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { IconDefinition, fas } from "@fortawesome/free-solid-svg-icons";
import { ApiClientService } from "../../services/api-client.service";

@Component({
  selector: 'app-profile',
  templateUrl: './user-profile.component.html',
})
export class UserProfileComponent implements OnInit {

  private _user: BehaviorSubject<UserViewModel> = new BehaviorSubject<UserViewModel>(null!);
  private _activity: BehaviorSubject<UserActivityItemClass[]> = new BehaviorSubject<UserActivityItemClass[]>([]);

  constructor(private fa: FaIconLibrary, private auth: AuthService, private api: ApiClientService) {
    fa.addIconPacks(fas);
    this.auth.getUser().subscribe(user => {
      if (user) {
        this._user.next(user);
        this.api.getUserActivity().subscribe(activities => {
          if (activities) {
            this._activity.next(activities.map(e => new UserActivityItemClass(e)));
          }
        });
      }
    });
  }

  ngOnInit(): void {
  }

  public get name(): string {
    return this._user.getValue()?.name;
  }

  public get canReview(): boolean {
    const user = this._user.getValue();
    return user?.isAdmin || user?.isReviewer;
  }

  public get role(): string {
    const user = this._user.getValue();
    if (user.isAdmin) {
      return 'Administrator';
    } else if (user.isReviewer) {
      return 'Reviewer';
    } else if (user.isWriter) {
      return 'Writer';
    } else {
      return 'Reader';
    }
  }

  public get roleDescription(): string {
    const user = this._user.getValue();
    if (user.isAdmin) {
      return 'Can authorize operations from any user including themselves.';
    } else if (user.isReviewer) {
      return 'Can authorize operations from any user excluding themselves.';
    } else if (user.isWriter) {
      return 'Can submit operations for review/approval.';
    } else {
      return 'Unauthenticated users only have a reader role.';
    }
  }

  public get totalEdits(): number {
    return this.activities.filter(e => e.activity == 'SubmitEdit' || e.activity == 'SubmitCreate').length;
  }
  public get totalApprovedEdits(): number {
    return this.activities.filter(e => (e.activity == 'SubmitEdit' || e.activity == 'SubmitCreate') && e.status == 'Approved').length;
  }
  public get totalPendingEdits(): number {
    return this.activities.filter(e => (e.activity == 'SubmitEdit' || e.activity == 'SubmitCreate') && e.status == 'Pending').length;
  }
  public get totalReviews(): number {
    return this.activities.filter(e => e.reviewedBy == this.name).length;
  }
  public get totalApprovedReviews(): number {
    return this.activities.filter(e => e.reviewedBy == this.name && e.status == 'Approved').length;
  }
  public get totalRejectedReviews(): number {
    return this.activities.filter(e => e.reviewedBy == this.name && e.status == 'Rejected').length;
  }

  public get activities(): UserActivityItemClass[] {
    return this._activity.getValue();
  }

  public getActivityIcon(activity: UserActivityType) : IconDefinition {
    switch (activity) {
      case UserActivityType.SubmitEdit:
        return this.fa.getIconDefinition('fas', 'square-pen')!;
      case UserActivityType.SubmitCreate:
        return this.fa.getIconDefinition('fas', 'square-plus')!;
      case UserActivityType.SubmitDelete:
        return this.fa.getIconDefinition('fas', 'square-minus')!;
      case UserActivityType.ApproveEdit:
        return this.fa.getIconDefinition('fas', 'user-pen')!;
      case UserActivityType.ApproveCreate:
        return this.fa.getIconDefinition('fas', 'user-plus')!;
      case UserActivityType.ApproveDelete:
        return this.fa.getIconDefinition('fas', 'user-minus')!;
      case UserActivityType.RejectEdit:
      case UserActivityType.RejectCreate:
      case UserActivityType.RejectDelete:
        return this.fa.getIconDefinition('fas', 'user-xmark')!;
      default:
        return null!;
    }
  }

  public getActivityClass(activity: UserActivityType): string {
    switch (activity) {
      case UserActivityType.SubmitCreate:
      case UserActivityType.SubmitDelete:
      case UserActivityType.SubmitEdit:
        return 'text-primary';
      case UserActivityType.ApproveCreate:
      case UserActivityType.ApproveDelete:
      case UserActivityType.ApproveEdit:
        return 'text-success';
      case UserActivityType.RejectCreate:
      case UserActivityType.RejectDelete:
      case UserActivityType.RejectEdit:
        return 'text-danger';
    }
  }
}
