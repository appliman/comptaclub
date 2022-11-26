using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Extensions;

public static class ModelExtensions
{
	public static void Levelize(this IEnumerable<Models.Account> list, int level = 0)
	{
		var unlevelizedList = list.Where(i => i.Level == -1);
		foreach (var item in unlevelizedList)
		{
			var parent = list.SingleOrDefault(i => i.Id == item.ParentAccountId 
													&& i.Level > -1);
			if (parent != null)
			{
				item.Level = parent.Level + 1;
			}
		}
		if (unlevelizedList.Any())
		{
			list.Levelize(level++);
		}
	}

	public static void Hierarchize(this IList<Models.Account> list)
	{
		var flatList = list.OrderBy(i => i.Level).ToList();
		var searchList = list.ToList();
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
				if (parent != null)
				{
					if (!parent.Children.Contains(first))
					{
						first.Level = parent.Level + 1;
						parent.Children.Add(first);
					}
				}
			}

			flatList.Remove(first);
		}
	}

	public static Models.Account? DeepFindParent(this IList<Models.Account> list, Guid parentId)
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

    internal static string GetSHA256(this string input)
    {
        using var crypto = System.Security.Cryptography.SHA256.Create();
        var buffer = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = crypto.ComputeHash(buffer);
        var result = string.Join(string.Empty, from b in hash select b.ToString("X2"));
        return result;
    }

}
