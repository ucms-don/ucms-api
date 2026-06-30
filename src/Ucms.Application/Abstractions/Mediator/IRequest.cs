namespace Ucms.Application.Abstractions.Mediator;

/// <summary>
/// Marker interface for mediator request messages.
/// Physically lives in Ucms.Application but uses legacy namespace for compatibility.
/// </summary>
public interface IRequest<TResponse> { }
