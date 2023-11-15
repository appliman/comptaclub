namespace ComptaClub.Blazor.ViewModels;

public class AssociatedMemberToEntryRow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MemberRow Member { get; set; } = new();
    public long Amount { get; set; }
}
