import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { UniverseService } from 'src/app/core/services/universe.service';
import { UniverseVm } from 'src/app/shared/models/universe-vm';

@Component({
  selector: 'app-modal-universe',
  templateUrl: './modal-universe.component.html',
  styleUrls: ['./modal-universe.component.scss']
})
export class ModalUniverseComponent implements OnInit {

  @Input() header!:string;
  @Input() universe!:UniverseVm;
  profileForm!:FormGroup;
  submitted = false;
  loading = false;


  constructor(
    public activeModal: NgbActiveModal,
    public fb:FormBuilder,
    private universeService:UniverseService
  )
  { }

  ngOnInit(): void {
    this.profileForm = this.fb.group({
      id:[],
      name:['',Validators.required]
    });
  }

  get f() { return this.profileForm.controls; }


  onSubmit(){
    this.submitted = true;

    if (this.profileForm.invalid)
      return;

    this.loading = true;
    this.universeService.Post(this.profileForm.value)
      .subscribe(
        data => {
          alert(data);
        },
        error => {
          alert(error);
          this.loading = false;
        }
      );

  }



}
