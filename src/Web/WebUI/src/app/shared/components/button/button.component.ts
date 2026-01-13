
import { ChangeDetectionStrategy, Component, OnInit, input } from '@angular/core';

@Component({
    selector: 'app-button',
    imports: [],
    template: `
  <button type="{{this.type()}}" class="{{this.class()}}">
    <i class="{{this.icon()}}" ></i>
    {{this.text()}}
  </button>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ButtonComponent implements OnInit{ 

  readonly type = input<string>("button");
  readonly class = input<string>("");
  readonly text = input<string>("");
  readonly icon = input<string>(""); 

  public ngOnInit(): void {

  }

}
