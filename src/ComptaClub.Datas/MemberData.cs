using ComptaClub.Datas.Enums;

namespace ComptaClub.Datas;

[Table("Members")]
public class MemberData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? LicenseNumber { get; set; } = null!;
    public string? LicenseTypeName { get; set; } = null!;
    public int CreationDate { get; set; }
    public int LastUpdate { get; set; }
    public MemberState State { get; set; }
}
