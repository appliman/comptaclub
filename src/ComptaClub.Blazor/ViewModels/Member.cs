using ComptaClub.Blazor.ViewModels;
using ComptaClub.Datas;

namespace ComptaClub.Blazor.ViewModels;

public class Member : IMetaEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? LicenseNumber { get; set; } = null!;
    public string? LicenseTypeName { get; set; } = null!;
    public DateTime CreationDate { get; set; }
    public int RowIndex { get; set; }
    public long Amount { get; set; }
    public Enums.MetaEntity MetaEntity => Enums.MetaEntity.Member;
}
