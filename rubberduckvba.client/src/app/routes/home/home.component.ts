import { Component, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { ApiClientService } from "../../services/api-client.service";
import { Tag } from '../../model/tags.model';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { BehaviorSubject } from 'rxjs';
import { Feature } from '../../model/feature.model';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnChanges {

  private readonly _main: BehaviorSubject<Tag> = new BehaviorSubject<Tag>(null!);
  public set main(value: Tag) {
    this._main.next(value);
  }
  public get main(): Tag {
    return this._main.getValue();
  }

  private readonly _next: BehaviorSubject<Tag> = new BehaviorSubject<Tag>(null!);
  public set next(value: Tag) {
    this._next.next(value);
  }
  public get next(): Tag {
    return this._next.getValue();
  }

  constructor(private api: ApiClientService, private fa: FaIconLibrary) {
    fa.addIconPacks(fas);
  }

  ngOnChanges(changes: SimpleChanges): void {
  }

  ngOnInit(): void {
    this.getTagInfo();
  }

  getTagInfo(): void {
    this.api.getLatestTags().subscribe(result => {
      if (result) {
        this._main.next(result.main);
        this._next.next(result.next);
      }
    });
  }

  public get tagTimestamp(): string {
    return this.main != null ? new Date(this.main.dateTimeUpdated).toISOString().replace('T', ' ') : '';
  }
}
