namespace ComptaClub.Datas;

[Table("DocumentsByEntities")]
public class DocumentByEntity : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public MetaEntity MetaEntity { get; set; }
    public Guid EntityId { get; set; }
}
