namespace ComptaClub.Datas;

[Table("DocumentsByEntities")]
public class DocumentByEntityData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public MetaEntity MetaEntity { get; set; }
    public Guid EntityId { get; set; }
    public Guid DocumentId { get; set; }
    public DateTime CreationDate { get; set; }
}
