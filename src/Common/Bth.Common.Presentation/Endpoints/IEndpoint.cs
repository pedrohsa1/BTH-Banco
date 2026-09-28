using Microsoft.AspNetCore.Routing;

namespace Bth.Common.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
