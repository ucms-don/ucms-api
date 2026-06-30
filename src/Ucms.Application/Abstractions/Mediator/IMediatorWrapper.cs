namespace Ucms.Application.Abstractions.Mediator;

public interface IMediatorWrapper
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
