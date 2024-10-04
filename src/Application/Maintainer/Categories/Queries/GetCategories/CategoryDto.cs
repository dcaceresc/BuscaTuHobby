﻿namespace Application.Maintainer.Categories.Queries.GetCategories;

public class CategoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public bool IsActive { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Category, CategoryDto>();
        }
    }
}

