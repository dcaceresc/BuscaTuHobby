import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { createProductCommand, productDto, productVM } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductsService {

  readonly productsGetPath = environment.apiUrl + '/api/Products';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<productDto>>{
    return this.http.get<Array<productDto>>(this.productsGetPath)
  }

  GetbyId(id:string | null):Observable<productVM>{
    return this.http.get<productVM>(`${this.productsGetPath}/${id}`);
  }

  Create(universe:productVM): Observable<any>{
    return this.http.post<createProductCommand>(this.productsGetPath,universe);
  }

  Update(id:string | null,product:productVM):Observable<any>{
    return this.http.put(`${this.productsGetPath}/${id}`, product);
  }

  Toggle(id:number){
    return this.http.delete(`${this.productsGetPath}/${id}`);
  }
}