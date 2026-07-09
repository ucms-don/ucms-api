namespace Ucms.Application.Services;

public interface IStockCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
