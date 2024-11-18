import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPencil, faPlus, faPowerOff, faRotate, IconDefinition } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [
    CommonModule,FontAwesomeModule
  ],
  template: `
  <button type="{{this.type}}" class="{{this.class}}">
    <fa-icon [icon]="faIcon"></fa-icon>
    {{this.text}}
  </button>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ButtonComponent implements OnInit{ 

  @Input() type: string = "button";
  @Input() class: string = "";
  @Input() text: string = "";
  @Input() icon: string = ""; 

  public faIcon! : IconDefinition 

  public ngOnInit(): void {
    switch(this.icon){
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
