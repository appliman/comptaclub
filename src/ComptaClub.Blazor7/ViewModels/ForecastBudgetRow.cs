namespace ComptaClub.Blazor.ViewModels;

public class ForecastBudgetRow : IMetaEntity
{
    public Guid Id { get => Entity.Id; set => Entity.Id = value; }
    public int RowIndex { get; set; }
    public Enums.MetaEntity MetaEntity => Enums.MetaEntity.ForecastBudget;
    public Datas.ForecastBudgetData Entity { get; set; } = default!;
}

