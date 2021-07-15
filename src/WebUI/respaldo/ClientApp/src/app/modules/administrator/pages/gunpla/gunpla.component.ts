import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { first } from 'rxjs/operators';
import { GradesService } from 'src/app/core/services/grades.service';
import { GunplasService } from 'src/app/core/services/gunplas.service';
import { ManufacturersService } from 'src/app/core/services/manufacturers.service';
import { ScalesService } from 'src/app/core/services/scales.service';
import { SeriesService } from 'src/app/core/services/series.service';
import { GradeVm } from 'src/app/shared/models/grade-vm';
import { GunplaVm } from 'src/app/shared/models/gunpla-vm';
import { ManufacturerVm } from 'src/app/shared/models/manufacturer-vm';
import { PhotoVm } from 'src/app/shared/models/photo-vm';
import { ScaleVm } from 'src/app/shared/models/scale-vm';
import { SerieVm } from 'src/app/shared/models/serie-vm';


@Component({
  templateUrl: './gunpla.component.html',
  styleUrls: ['./gunpla.component.css']
})
export class GunplaComponent implements OnInit {

  form: FormGroup;
  submitted:boolean = false;
  loading:boolean = false;
  gunpla:GunplaVm
  grades:GradeVm[]
  scales:ScaleVm[]
  manufacturers:ManufacturerVm[]
  series:SerieVm[]
  photos:PhotoVm[]
  gunplaId:number


  constructor(
    private formBuilder:FormBuilder,
    private gunplasService:GunplasService,
    private gradeService:GradesService,
    private scalesService:ScalesService,
    private manufacturersService:ManufacturersService,
    private seriesService:SeriesService
  ) { }

  ngOnInit() {

    this.gradeService.Get().subscribe(items => this.grades = items);
    this.scalesService.Get().subscribe(items => this.scales = items);
    this.manufacturersService.Get().subscribe(items => this.manufacturers = items);
    this.seriesService.Get().subscribe(items => this.series = items);


    this.form = this.formBuilder.group({
      name: ['',Validators.required],
      gradeId: ['',Validators.required],
      scaleId: ['',Validators.required],
      manufacturerId:['',Validators.required],
      serieId : ['',Validators.required],
      base: ['',Validators.required],
      description : ['',Validators.required],
      realeaseDate:['',Validators.required]
    });
  }

  selectFiles(files:any[]) {
    if (files && files.length > 0) {

      for (let index = 0; index < files.length; index++) {
        let file = files[index];

        let photo:PhotoVm
        photo.order = 0
        photo.imageData = file



      }

    }
  }



  onSubmit(){

    this.submitted = true;

    //stop here if form is invalid
    if (this.form.invalid) {

      return;
    }
    this.loading = true;



    // this.gunplasService.gunplasCreate(this.form.value)
    //   .pipe(first())
    //   .subscribe(
    //     data => {
    //       //this.gunplaId = data
    //     },
    //     error => {
    //       console.log(error);
    //       this.loading = false;
    //     }
    //   );





  }

}
