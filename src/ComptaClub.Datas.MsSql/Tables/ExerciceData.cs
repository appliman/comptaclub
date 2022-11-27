namespace ComptaClub.Datas;

[Table("Exercices")]
public class ExerciceData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;

    public string? Label { get; set; } 
    public long InitialAmount { get; set; }
    public long BalanceAmount { get; set; }

    public int StartDate { get; set; }
    public int EndDate { get; set; }
    public int CreationDate { get; set; }
    public Guid? LastEntryId { get; set; }
    public bool Active { get; set; } = false;
}
