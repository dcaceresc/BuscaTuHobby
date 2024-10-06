﻿namespace Application.Maintainer.Menus.Commands.CreateMenu;
public record CreateMenu(string MenuName, int MenuOrden) : IRequest<ApiResponse>;

public class CreateMenuHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateMenu, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateMenu request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = Menu.Create(request.MenuName,request.MenuOrden);

            _context.Menus.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Menu creado correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo crear el menu");
        }
    }
}