import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { categoryDto, categoryVM, createCategoryCommand, createSubCategoryCommand, subCategoryDto, subCategoryVM } from '../models/category.model';

@Injectable({
  providedIn: 'root'
})
export class CategoriesService {

  readonly CategoriesGetPath = environment.apiUrl + '/api/Categories';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<categoryDto>>{
    return this.http.get<Array<categoryDto>>(this.CategoriesGetPath)
  }

  GetbyId(id:string | null):Observable<categoryVM>{
    return this.http.get<categoryVM>(`${this.CategoriesGetPath}/${id}`);
  }

  Create(category : createCategoryCommand): Observable<any>{
    return this.http.post<createCategoryCommand>(this.CategoriesGetPath,category);
  }

  Update(id:string | null,category:categoryVM):Observable<any>{
    return this.http.put(`${this.CategoriesGetPath}/${id}`, category);
  }

  Toggle(id:number){
    return this.http.delete(`${this.CategoriesGetPath}/${id}`);
  }

  GetAllSubCategory(categoryId:string):Observable<Array<subCategoryDto>>{
    return this.http.get<Array<subCategoryDto>>(`${this.CategoriesGetPath}/${categoryId}/Subcategories`);
  }

  GetSubCategorybyId(id:string | null, categoryId :string | null):Observable<subCategoryVM>{
    return this.http.get<subCategoryVM>(`${this.CategoriesGetPath}/${categoryId}/Subcategories/${id}`);
  }

  CreateSubCategory(category : createSubCategoryCommand): Observable<any>{
    return this.http.post<createSubCategoryCommand>(`${this.CategoriesGetPath}/${category.categoryId}/Subcategories`,category);
  }

  UpdateSubCategory(id:string | null,categoryId:string | null,category:subCategoryVM):Observable<any>{
    return this.http.put(`${this.CategoriesGetPath}/${categoryId}/Subcategories/${id}`, category);
  }

  ToggleSubCategory(id:number, categoryId:string |null){
    return this.http.delete(`${this.CategoriesGetPath}/${categoryId}/Subcategories/${id}`);
  }



}
