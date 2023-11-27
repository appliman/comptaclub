namespace ComptaClub.Contracts.Models.Members;

public class BalanceByMember
{
	public Guid MemberId { get; set; }
	public Guid ExerciceId { get; set; }
	public long Balance { get; set; }
	public int EntryCount { get; set; }
}
