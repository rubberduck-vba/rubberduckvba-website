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
}
