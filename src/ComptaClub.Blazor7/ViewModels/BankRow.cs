using ComptaClub.Datas;

namespace ComptaClub.Blazor.ViewModels;

public class BankRow : IMetaEntity
{
	public Guid Id { get => Entity.Id; set => Entity.Id = value; }
	public int RowIndex { get; set; }
    public Enums.MetaEntity MetaEntity => Enums.MetaEntity.Bank;
    public Datas.BankData Entity { get; set; } = default!;
}
