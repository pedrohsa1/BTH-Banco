using MediatR;
using Microsoft.Extensions.Logging;

namespace Bth.Common.Application.Behaviors;

internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        logger.LogInformation("Processando {RequestName}", requestName);

        TResponse response = await next();

        logger.LogInformation("Concluído {RequestName}", requestName);
        return response;
    }
}
