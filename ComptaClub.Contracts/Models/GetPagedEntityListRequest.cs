namespace ComptaClub.Contracts.Models;

public record GetPagedEntityListRequest<F, D> : IRequest<PagedList<IEnumerable<D>>>
	where F : class, IListFilter, new()
	where D : class, IPrimaryKey, new()
{
	readonly Action<F>? predicateFilter = null;
	readonly F? filter = null;

	public GetPagedEntityListRequest(Action<F>? predicateFilter)
	{
		this.predicateFilter = predicateFilter;
	}

	public GetPagedEntityListRequest(F? filter)
	{
		this.filter = filter;
	}

	public F GetFilter(F inputFilter)
	{
		F result = default!;
		if (filter != null)
		{
			result = filter!;
		}
		else if (predicateFilter != null)
		{
			predicateFilter.Invoke(inputFilter);
			result = inputFilter;
		}

		return result;
	}
}
