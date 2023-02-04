using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Extensions;

public static class ModelExtensions
{
    internal static string GetSHA256(this string input)
    {
        using var crypto = System.Security.Cryptography.SHA256.Create();
        var buffer = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = crypto.ComputeHash(buffer);
        var result = string.Join(string.Empty, from b in hash select b.ToString("X2"));
        return result;
    }

    public static void Sanitize<T>(this T model)
    // where T : new()
    {
        if (model == null)
        {
            return;
        }

        var properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(string))
            {
                var value = property.GetValue(model);
                if (value != null)
                {
                    var stringValue = (string)value;
                    property.SetValue(model, stringValue.Trim());
                }
            }
        }
    }

    public static IList<Datas.AccountData> ToFlatList(this IList<Datas.AccountData> plan)
    {
        var result = new List<Datas.AccountData>();
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

    public static void Levelize(this IEnumerable<Datas.AccountData> list, int level = 0)
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

    public static void Hierarchize(this IList<Datas.AccountData> list)
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

    public static Datas.AccountData? DeepFindParent(this IList<Datas.AccountData> list, Guid parentId)
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

    public static Datas.AccountData? DeepFind(this IList<Datas.AccountData> list, Guid id)
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

    public static IEnumerable<Datas.AccountData> GetLeafList(this IList<Datas.AccountData> list)
    {
        var result = new List<Datas.AccountData>();
        foreach (var item in list)
        {
            if (!item.Children.Any())
            {
                result.Add((Datas.AccountData) item.Clone());
            }
            else
            {
                result.AddRange(item.Children.GetLeafList());
            }
        }
        return result;
    }


}
