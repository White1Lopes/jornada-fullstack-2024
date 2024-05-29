using System.Security.Claims;
using Fina.Api.Common.Api;
using Fina.Core.Handlers;
using Fina.Core.Models;
using Fina.Core.Requests.Categories;
using Fina.Core.Responses;

namespace Fina.Api.Endpoints.Categories;

public class UpdateCategoryEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id}", HandleAsync)
              .WithName("Categories: Update")
              .WithSummary("Atualiza uma categorias")
              .WithDescription("Atualiza uma categorias")
              .WithOrder(2)
              .Produces<Response<Category>?>();

    private static async Task<IResult> HandleAsync(
        ICategoryHandler handler,
        UpdateCategoryRequest request,
        long id)
    {
        // request.UserId = user.Identity?.Name ?? String.Empty;
        request.UserId = ApiConfiguration.UserId;
        request.Id = id;
        var response = await handler.UpdateAsync(request);
        return response.IsSuccess
            ? TypedResults.Ok(response)
            : TypedResults.BadRequest(response);
    }
}