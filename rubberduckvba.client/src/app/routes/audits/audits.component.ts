import { Component, OnInit } from "@angular/core";
import { ApiClientService } from "../../services/api-client.service";
import { PendingAuditsViewModel } from "../../model/feature.model";

@Component({
  selector: 'app-audits',
  templateUrl: './audits.component.html'
})
export class AuditsAdminComponent implements OnInit {

  constructor(private api: ApiClientService) {

  }

  public pendingAudits: PendingAuditsViewModel = { edits: [], other: [] };

  ngOnInit(): void {
    this.api.getPendingAudits().subscribe(e => this.pendingAudits = e);
  }
  
}
