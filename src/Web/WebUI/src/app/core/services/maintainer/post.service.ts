import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreatePost, PostDto, PostVM, UpdatePost } from '../../models/maintainer/post.model';

@Injectable({
  providedIn: 'root'
})
export class PostService {

  private http = inject(HttpClient);

  public getPosts() {
    return this.http.get<ApiResponse<PostDto[]>>('api/maintainer/posts');
  }

  public getPostById(postId: string | null) {
    return this.http.get<ApiResponse<PostVM>>(`api/maintainer/posts/${postId}`);
  }

  public createPost(post: CreatePost) {
    return this.http.post<ApiResponse<string>>('api/maintainer/posts', post);
  }

  public updatePost(postId: string | null, post: UpdatePost) {
    return this.http.put<ApiResponse<any>>(`api/maintainer/posts/${postId}`, post);
  }

  public togglePost(postId: string | null) {
    return this.http.delete<ApiResponse<any>>(`api/maintainer/posts/${postId}`);
  }
}
