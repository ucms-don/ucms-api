namespace Ucms.Application.Abstractions.Mediator;

using MassTransit;

/// <summary>
/// Abstract base for MassTransit-backed request handlers.
/// Bridges the RequestHandler pattern with MassTransit IConsumer.
/// </summary>
public abstract class RequestHandler<TRequest, TResponse> : IConsumer<TRequest>
    where TRequest : class
{
    public async Task Consume(ConsumeContext<TRequest> context)
    {
        var result = await Handle(context.Message, context.CancellationToken);
        await context.RespondAsync(result!);
    }

    protected abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
