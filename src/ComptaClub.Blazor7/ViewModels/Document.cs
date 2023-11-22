using ComptaClub.Blazor.ViewModels;
using ComptaClub.Datas;

namespace ComptaClub.Blazor.ViewModels;

public class Document : IMetaEntity
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public string FileName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Base64Content { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long Size { get; set; }
    public Guid? UserOwnerId { get; set; }
    public int RowIndex { get; set; }
    public Enums.MetaEntity MetaEntity => Enums.MetaEntity.Document;
}
