import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPencil, faPlus, faPowerOff, faRotate, IconDefinition } from '@fortawesome/free-solid-svg-icons';

@Component({
    selector: 'app-button',
    imports: [
        CommonModule, FontAwesomeModule
    ],
    template: `
  <button type="{{this.type()}}" class="{{this.class()}}">
    <fa-icon [icon]="faIcon"></fa-icon>
    {{this.text()}}
  </button>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ButtonComponent implements OnInit{ 

  readonly type = input<string>("button");
  readonly class = input<string>("");
  readonly text = input<string>("");
  readonly icon = input<string>(""); 

  public faIcon! : IconDefinition 

  public ngOnInit(): void {
    switch(this.icon()){
      case "add":
        this.faIcon = faPlus;
        break;
      case "edit":
        this.faIcon = faPencil;
        break;
      case "toggle":
        this.faIcon = faPowerOff;
        break;
      case "refresh":
        this.faIcon = faRotate;
        break;
      default:
        this.faIcon = faPlus;
        break;
    }
  }

}
