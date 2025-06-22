import { Component, OnInit } from "@angular/core";
import { ApiClientService } from "../../services/api-client.service";
import { FeatureEditViewModel, FeatureOperation, FeatureOperationViewModel, PendingAuditsViewModel } from "../../model/feature.model";
import { Change, diffWords } from "diff";
import { encode } from "html-entities";

@Component({
  selector: 'app-audits',
  templateUrl: './audits.component.html',
  styleUrls: ['./audits.component.css']
})
export class AuditsAdminComponent implements OnInit {

  constructor(private api: ApiClientService) {

  }

  public pendingAudits: PendingAuditsViewModel = { edits: [], other: [] };

  ngOnInit(): void {
    this.api.getAllPendingAudits().subscribe(e => this.pendingAudits = e);
  }

  public get deleteOp() { return FeatureOperation.Delete; }
  public get createOp() { return FeatureOperation.Create; }

  public getDiffHtml(before: string, after: string): string {
    const diff = diffWords(before, after, { ignoreCase: false });
    return diff.map((part: Change) => {
      const htmlEncodedValue = encode(part.value);
      if (part.added) {
        console.log(`added: ${part.value}`);
        return `<span class="text-diff-added">${htmlEncodedValue}</span>`;
      } else if (part.removed) {
        console.log(`removed: ${part.value}`);
        return `<span class="text-diff-removed">${htmlEncodedValue}</span>`;
      } else {
        return part.value;
      }
    }).join('');
  }

  public onApproveEdit(edit:FeatureEditViewModel): void {
    this.api.approvePendingAudit(edit.id).subscribe(() => {
      console.log(`Feature edit audit record id ${edit.id} was approved.`);
      window.location.reload();
    });
  }

  public onRejectEdit(edit:FeatureEditViewModel): void {
    this.api.rejectPendingAudit(edit.id).subscribe(() => {
      console.log(`Feature edit audit record id ${edit.id} was rejected.`);
      window.location.reload();
    });
  }

  public onApproveOperation(op: FeatureOperationViewModel): void {
    this.api.approvePendingAudit(op.id).subscribe(() => {
      console.log(`Feature operation audit record id ${op.id} was approved.`);
      window.location.reload();
    });
  }

  public onRejectOperation(op: FeatureOperationViewModel): void {
    this.api.rejectPendingAudit(op.id).subscribe(() => {
      console.log(`Feature operation audit record id ${op.id} was rejected.`);
      window.location.reload();
    });
  }
}
