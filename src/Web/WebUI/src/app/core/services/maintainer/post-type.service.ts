import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreatePostType, PostTypeDto, PostTypeVM, UpdatePostType } from '../../models/maintainer/post-type.model';

@Injectable({
  providedIn: 'root'
})
export class PostTypeService {

  private http = inject(HttpClient);

  public getPostTypes() {
    return this.http.get<ApiResponse<PostTypeDto[]>>('api/maintainer/post-types');
  }

  public getPostTypeById(postTypeId: string | null) {
    return this.http.get<ApiResponse<PostTypeVM>>(`api/maintainer/post-types/${postTypeId}`);
  }

  public createPostType(postType: CreatePostType) {
    return this.http.post<ApiResponse<any>>('api/maintainer/post-types', postType);
  }

  public updatePostType(postTypeId: string | null, postType: UpdatePostType) {
    return this.http.put<ApiResponse<any>>(`api/maintainer/post-types/${postTypeId}`, postType);
  }

  public togglePostType(postTypeId: string | null) {
    return this.http.delete<ApiResponse<any>>(`api/maintainer/post-types/${postTypeId}`);
  }
}
