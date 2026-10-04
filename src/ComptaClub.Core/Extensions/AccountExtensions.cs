using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Accounts;

namespace ComptaClub.Extensions;
public static class AccountExtensions
{
	public static async Task<AccountData?> GetAccountById(this IMediator mediator, Guid accountId)
	{
		var account = await mediator.Send(new GetAccountByFilterRequest(f => f.GetById(accountId)));
		return account;
	}

	public static async Task<List<AccountData>> GetPlan(this IMediator mediator)
	{
		var plan = await mediator.Send(new GetPlanRequest());
		return plan;
	}

	public static IList<AccountData> ToFlatList(this IList<AccountData> plan)
	{
		var result = new List<AccountData>();
		while (true)
		{
			var item = plan.FirstOrDefault();
			if (item == null)
			{
				break;
			}
			plan.Remove(item);
			result.Add(item);
			if (item.Children.Any())
			{
				var flat = item.Children.ToFlatList();
				result.AddRange(flat);
			}
		}
		return result;
	}

    public static void Levelize(this IEnumerable<AccountData> list, int level = 0)
    {
        var _items = list.ToList();
        var _byId = _items.ToDictionary(item => item.Id);
        var _children = _items.Where(item => item.ParentAccountId.HasValue)
            .ToLookup(item => item.ParentAccountId!.Value);
        var _queue = new Queue<AccountData>();
        foreach (var _item in _items)
        {
            _item.Level = -1;
            if (!_item.ParentAccountId.HasValue)
            {
                _item.Level = level;
                _queue.Enqueue(_item);
            }
            else if (!_byId.ContainsKey(_item.ParentAccountId.Value))
            {
                throw new InvalidDataException("La hiérarchie contient un parent introuvable.");
            }
        }
        var _visited = 0;
        while (_queue.TryDequeue(out var _parent))
        {
            _visited++;
            foreach (var _child in _children[_parent.Id])
            {
                _child.Level = _parent.Level + 1;
                _queue.Enqueue(_child);
            }
        }
        if (_visited != _items.Count)
        {
            throw new InvalidDataException("La hiérarchie contient un cycle.");
        }
    }

	public static void Hierarchize(this IList<AccountData> list)
	{
		var flatList = list.OrderBy(i => i.Level).ToList();
		list.Clear();
		while (true)
		{
			var first = flatList.FirstOrDefault();
			if (first == null)
			{
				break;
			}

			// Recherche du parent
			if (!first.ParentAccountId.HasValue)
			{
				if (!list.Contains(first))
				{
					list.Add(first);
				}
			}
			else
			{
				var parent = list.DeepFindParent(first.ParentAccountId.Value);
				if (parent is not null &&
					!parent.Children.Contains(first))
				{
					first.Level = parent.Level + 1;
					parent.Children.Add(first);
				}
			}

			flatList.Remove(first);
		}
	}

	public static AccountData? DeepFindParent(this IList<AccountData> list, Guid parentId)
	{
		foreach (var item in list)
		{
			if (item.Id == parentId)
			{
				return item;
			}
			if (item.Children.Any())
			{
				var result = item.Children.DeepFindParent(parentId);
				if (result != null)
				{
					return result;
				}
			}
		}
		return null;
	}

	public static AccountData? DeepFind(this IList<AccountData> list, Guid id)
	{
		foreach (var item in list)
		{
			if (item.Id == id)
			{
				return item;
			}
			if (item.Children.Any())
			{
				var result = item.Children.DeepFind(id);
				if (result != null)
				{
					return result;
				}
			}
		}
		return null;
	}

	public static IEnumerable<AccountData> GetLeafList(this IList<AccountData> list)
	{
		var result = new List<AccountData>();
		foreach (var item in list)
		{
			if (!item.Children.Any())
			{
				result.Add((AccountData)item.Clone());
			}
			else
			{
				result.AddRange(item.Children.GetLeafList());
			}
		}
		return result;
	}

	public static IEnumerable<AccountData> GetParentList(this AccountData account, IEnumerable<AccountData> list)
	{
		var result = new List<AccountData>();
		result.Add(account);
		if (account.ParentAccountId is not null)
		{
			var parent = list.Single(i => i.Id == account.ParentAccountId);
			result.AddRange(parent.GetParentList(list));
		}
		return result;
	}

	public static List<Guid> GetIdListWithAllChildren(this AccountData account)
	{
		var result = new List<Guid>();

		result.Add(account.Id);

		foreach (var item in account.Children)
		{
			result.AddRange(item.GetIdListWithAllChildren());
		}

		return result;
	}

	public static async Task<IEnumerable<AccountData>> GetAllAccounts(this IMediator mediator)
	{
		var filter = new AccountListFilter
		{
			PageSize = int.MaxValue
		};
		var page = await mediator.Send(new GetPagedEntityListRequest<AccountListFilter, AccountData>(filter));

		var list = new List<AccountData>();
		foreach (var data in page.List)
		{
			data.Level = data.ParentAccountId == null ? 0 : -1;
			list.Add(data);
		}
		return list;
	}

}
