namespace ComptaClub.Blazor.ViewModels;

public class MemberRow : IMetaEntity
{
	public Guid Id { get => Entity.Id; set => Entity.Id = value; }
	public Datas.MemberData Entity { get; set; } = default!;
    public int RowIndex { get; set; }
    public long Amount { get; set; }
    public Enums.MetaEntity MetaEntity => Enums.MetaEntity.Member;
}
