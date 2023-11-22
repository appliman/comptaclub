namespace ComptaClub.Blazor.ViewModels;

public class EntryRow : IMetaEntity
{
	public Guid Id { get => Entity.Id; set => Entity.Id = value; }
	public Datas.EntryData Entity { get; set; } = default!;
	public bool IsSelected { get; set; } = false;
	public MetaEntity MetaEntity => MetaEntity.Entry;
	public int RowIndex { get; set; }
	public List<MemberRow> AssociatedMemberList { get; set; } = new();
}
