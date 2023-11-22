using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Contracts.Models.ForecastBudget;
using ComptaClub.Enums;

namespace ComptaClub.Extensions;
public static class ForecastBudgetExtensions
{
    public static async Task<ForecastBudgetData?> GetForecastBudgetById(this IMediator mediator, Guid id)
    {
        var filter = new ForecastBudgetListFilter();
        filter.GetById(id);
        var result = await mediator.Send(new GetPagedEntityListRequest<ForecastBudgetListFilter, ForecastBudgetData>(filter));
        return result.List.FirstOrDefault();
    }

    public static async Task<List<ForecastBudgetItemData>> GetForecastBudgetItemList(this IMediator mediator, Guid forecasetBudgetId)
    {
        var filter = new ForecastBudgetItemListFilter();
        filter.ForeCastBudgetId = forecasetBudgetId;
        var page = await mediator.Send(new GetPagedEntityListRequest<ForecastBudgetItemListFilter, ForecastBudgetItemData>(filter));

        var result = page.List.ToList();
        foreach (var item in result)
        {
            item.Level = item.ParentForecastBudgetItemId is null ? 0 : -1;
        }

        return result;
    }

    public static void Levelize(this IEnumerable<ForecastBudgetItemData> list, int level = 0)
    {
        var unlevelizedList = list.Where(i => i.Level == -1);
        foreach (var item in unlevelizedList)
        {
            var parent = list.SingleOrDefault(i => i.Id == item.ParentForecastBudgetItemId
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

    public static void Hierarchize(this IList<ForecastBudgetItemData> list)
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
            if (!first.ParentForecastBudgetItemId.HasValue)
            {
                if (!list.Contains(first))
                {
                    list.Add(first);
                }
            }
            else
            {
                var parent = list.DeepFindParent(first.ParentForecastBudgetItemId.Value);
                if (parent != null
                    && !parent.Children.Contains(first))
                {
                    first.Level = parent.Level + 1;
                    parent.Children.Add(first);
                }
            }

            flatList.Remove(first);
        }
    }

    public static ForecastBudgetItemData? DeepFindParent(this IList<ForecastBudgetItemData> list, Guid parentId)
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

    public static IEnumerable<ForecastBudgetItemData> GetLeafList(this IList<ForecastBudgetItemData> list)
    {
        var result = new List<ForecastBudgetItemData>();
        foreach (var item in list)
        {
            if (!item.Children.Any())
            {
                result.Add((ForecastBudgetItemData)item.Clone());
            }
            else
            {
                result.AddRange(item.Children.GetLeafList());
            }
        }
        return result;
    }

    public static void ComputeTotal(this ForecastBudgetData forecastBudget)
    {
        // Calcul des regroupements de totaux
        ComputeSubTotal(forecastBudget.ItemList);

        forecastBudget.CreditTotal = forecastBudget.ItemList.Where(i => i.Direction == AccountDirection.Credit).Sum(i => i.Amount);
        forecastBudget.DebitTotal = forecastBudget.ItemList.Where(i => i.Direction == AccountDirection.Debit).Sum(i => i.Amount);
    }

    private static void ComputeSubTotal(List<ForecastBudgetItemData> items)
    {
        foreach (var subItem in items)
        {
            if (subItem.Children.Any())
            {
                subItem.Amount = subItem.Children.Sum(i => i.Amount);
                ComputeSubTotal(subItem.Children);
            }
        }
    }
}
