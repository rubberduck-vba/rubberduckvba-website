import { Component, OnInit } from "@angular/core";
import { ApiClientService } from "../../services/api-client.service";
import { FeatureOperation, PendingAuditsViewModel } from "../../model/feature.model";

@Component({
    selector: 'app-audits',
    templateUrl: './audits.component.html',
    styleUrls: ['./audits.component.css'],
    standalone: false
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
