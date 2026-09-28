using Bth.Common.Domain;
using MediatR;

namespace Bth.Common.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
