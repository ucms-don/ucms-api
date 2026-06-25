namespace Ucms.Application.Services;

/// <summary>
/// Foydalanuvchi SerialNumber maydonini bo'sh qoldirsa, mahsulot (Product.Code) asosida
/// noyob seriya raqami avtomatik generatsiya qilinadi.
/// </summary>
public interface ISkuSerialNumberGenerator
{
    Task<string> GenerateAsync(Guid productId, CancellationToken ct = default);
}
