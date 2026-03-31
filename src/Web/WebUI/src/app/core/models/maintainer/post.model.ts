export interface PostDto {
  postId: string;
  postTitle: string;
  postTypeName: string;
  categories: string[];
  isActive: boolean;
}

export interface PostVM {
  postId: string;
  postTitle: string;
  postContent: string;
  postTypeId: string;
  categoryIds: string[];
}

export interface CreatePost {
  postTitle: string;
  postContent: string;
  postTypeId: string;
  categoryIds: string[];
}

export interface UpdatePost {
  postId: string;
  postTitle: string;
  postContent: string;
  postTypeId: string;
  categoryIds: string[];
}
