namespace Ucms.Application.Abstractions.Storage;

public interface IAvatarStorageClient
{
    /// <summary>
    /// Foydalanuvchi avatarini saqlaydi va ommaviy URL manzilini qaytaradi.
    /// Сохраняет аватар пользователя и возвращает публичный URL.
    /// </summary>
    Task<string> SaveAsync(Guid userId, string fileExtension, Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Foydalanuvchining oldingi avatarini (agar mavjud bo'lsa) o'chiradi.
    /// Удаляет предыдущий аватар пользователя (если есть).
    /// </summary>
    void DeleteExisting(string? avatarUrl);
}
