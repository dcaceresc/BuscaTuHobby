export interface PostTypeDto {
  postTypeId: string;
  postTypeName: string;
  isActive: boolean;
}

export interface PostTypeVM {
  postTypeId: string;
  postTypeName: string;
}

export interface CreatePostType {
  postTypeName: string;
}

export interface UpdatePostType {
  postTypeId: string;
  postTypeName: string;
}
