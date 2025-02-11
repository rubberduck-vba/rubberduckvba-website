import { Component, Input } from '@angular/core';

@Component({
  //standalone: true,
  selector: 'loading-content',
  templateUrl: './loading-content.component.html'
})
export class LoadingContentComponent {
  @Input() public show: boolean = true;
  @Input() public label: string = 'loading...';
}
