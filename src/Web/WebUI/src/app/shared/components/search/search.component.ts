import { ChangeDetectionStrategy, Component, output, signal } from '@angular/core';

@Component({
  selector: 'app-search',
  imports: [],
  templateUrl: './search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchComponent {
  public value = signal('');
  readonly search = output<string>();

  onInput(event: Event) {
    const term = (event.target as HTMLInputElement).value;
    this.value.set(term);
    this.search.emit(term);
  }

  clear() {
    this.value.set('');
    this.search.emit('');
  }
}