import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-confirm-delete-dialog',
  template: `
    <div class="toast" role="alert" aria-live="assertive" aria-atomic="true">
      <div class="toast-body">
        Hello, world! This is a toast message.
        <div class="mt-2 pt-2 border-top">
          <button type="button" class="btn btn-primary btn-sm">Take action</button>
          <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="toast">Close</button>
        </div>
      </div>
    </div>

  `,
  styleUrls: ['./confirm-delete-dialog.component.scss']
})
export class ConfirmDeleteDialogComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {

  }

}
