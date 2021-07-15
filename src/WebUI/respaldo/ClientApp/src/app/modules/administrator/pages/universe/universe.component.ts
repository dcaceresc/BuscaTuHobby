import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { UniversesService } from 'src/app/core/services/universes.service';
import { UniverseVm } from 'src/app/shared/models/universe-vm';


@Component({
  templateUrl: './universe.component.html',
  styleUrls: ['./universe.component.css']
})
export class UniverseComponent implements OnInit {

  form: FormGroup;
  submitted:boolean = false;
  loading:boolean = false;
  universes:UniverseVm[]

  constructor(
    private formBuilder:FormBuilder,
    private universesService:UniversesService
  ) { }

  ngOnInit() {

    this.universesService.Get().subscribe(items => this.universes = items);

    this.form = this.formBuilder.group({
      name: [null,Validators.required]
    });
  }

  onSubmit(){

    this.submitted = true;

    //stop here if form is invalid
    if (this.form.invalid) {

      return;
    }
    this.loading = true;

    let universe = {
      name : this.form.value.name
    }

    console.log(JSON.stringify(universe))

    // this.universesService.universesCreate(this.form.value)
    // .pipe(first())
    //   .subscribe(
    //     data => {
    //       console.log(data);
    //       this.loading = false;
    //     },
    //     error => {
    //       console.log(error);
    //       this.loading = false;
    //     }
    //   );

  }

}
