namespace Ucms.Application.Services;

/// <summary>
/// Foydalanuvchi WorkType.Code maydonini bo'sh qoldirsa, bazada mavjud bo'lmagan,
/// bir xil stildagi ("WT-000001") noyob kod avtomatik generatsiya qilinadi.
/// </summary>
public interface IWorkTypeCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
