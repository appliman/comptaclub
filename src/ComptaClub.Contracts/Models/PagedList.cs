using System.Collections;

namespace ComptaClub.Contracts.Models;

public class PagedList<TList>
	where TList : IEnumerable
{
	public TList List { get; set; } = default!;
	public PagedTotal Total { get; set; } = new();
}
