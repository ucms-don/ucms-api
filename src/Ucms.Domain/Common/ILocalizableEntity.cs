namespace Ucms.Domain.Common;

public interface ILocalizableEntity
{
    string Name { get; set; }
    string NameRu { get; set; }
    string? NameEn { get; set; }
    string? NameKa { get; set; }
}
