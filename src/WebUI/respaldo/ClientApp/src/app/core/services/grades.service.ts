import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GradeVm } from 'src/app/shared/models/grade-vm';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GradesService {

  readonly GradesGetPath = environment.apiUrl + '/api/Grades';

  constructor(
    private http:HttpClient
  ) { }

  Get():Observable<Array<GradeVm>>{
    return this.http.get<Array<GradeVm>>(this.GradesGetPath)
  }
}
