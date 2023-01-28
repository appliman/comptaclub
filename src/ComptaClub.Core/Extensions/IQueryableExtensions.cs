
using System.Linq.Expressions;

namespace ComptaClub.Extensions
{
	public static class IQueryableExtensions
	{
		public static void EnsureGoodFilter(this Models.IListFilter filter)
		{
			filter.PageIndex = Math.Max(0, filter.PageIndex);
			filter.PageIndex = Math.Min(5000, filter.PageIndex);
			filter.PageSize = Math.Max(1, filter.PageSize);
			filter.PageSize = Math.Min(200, filter.PageSize);
			filter.Sanitize();
			if (filter.Search != null)
			{
				filter.Search = filter.Search.Replace("%", "[%]");
			}
		}

		public static IOrderedQueryable<T> OrderWithFilter<T, TKey>(this IQueryable<T> query, 
			Models.IListFilter filter, 
			Expression<Func<T, TKey>> defaultSortColumn, 
			System.ComponentModel.ListSortDirection defaultSortDirection = System.ComponentModel.ListSortDirection.Ascending, 
			string? thenByColumnName = null, 
			System.ComponentModel.ListSortDirection thenSortDirection = System.ComponentModel.ListSortDirection.Ascending
			)
		{
			IOrderedQueryable<T>? orderedQuery = null;

			if (!string.IsNullOrWhiteSpace(filter.SortByName))
			{
				if (filter.SortDirection == System.ComponentModel.ListSortDirection.Ascending)
				{
					orderedQuery = query.OrderBy(filter.SortByName);
				}
				else
				{
					orderedQuery = query.OrderByDescending(filter.SortByName);
				}
			}
			else
			{
				if (defaultSortDirection == System.ComponentModel.ListSortDirection.Ascending)
				{
					orderedQuery = query.OrderBy(defaultSortColumn);
				}
				else
				{
					orderedQuery = query.OrderByDescending(defaultSortColumn);
				}
			}

			if (!string.IsNullOrWhiteSpace(thenByColumnName))
			{
				if (thenSortDirection == System.ComponentModel.ListSortDirection.Ascending)
				{
					orderedQuery = orderedQuery.ThenOrderBy(thenByColumnName);
				}
				else
				{
					orderedQuery = orderedQuery.ThenOrderByDescending(thenByColumnName);
				}
			}

			return orderedQuery;
		}

		public static IQueryable<T> GetPagedWithFilter<T>(this IQueryable<T> query, Models.IListFilter filter)
		{
			if (filter.Skip.HasValue)
			{
				return query.Skip(filter.Skip.Value).Take(filter.PageSize);
			}
			return query.Skip(filter.PageIndex * filter.PageSize).Take(filter.PageSize);
		}

		public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string memberName)
		{
            var split = memberName.Split(' ');
			if (split.Length == 1)
			{
				return InternalOrderBy(query, memberName, "OrderBy");
			}
			else
			{
				if (split[1].Equals("desc", StringComparison.InvariantCultureIgnoreCase))
				{
                    return InternalOrderBy(query, split[0], "OrderByDescending");
                }
                else
				{
                    return InternalOrderBy(query, split[0], "OrderBy");
                }
            }
        }

		public static IOrderedQueryable<T> ThenOrderBy<T>(this IQueryable<T> query, string memberName)
		{
			return InternalOrderBy(query, memberName, "ThenBy");
		}

        public static IOrderedQueryable<T> OrderByAscending<T>(this IQueryable<T> query, string memberName)
        {
            return InternalOrderBy(query, memberName, "OrderBy");
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string memberName)
		{
			return InternalOrderBy(query, memberName, "OrderByDescending");
		}

		public static IOrderedQueryable<T> ThenOrderByDescending<T>(this IQueryable<T> query, string memberName)
		{
			return InternalOrderBy(query, memberName, "ThenByDescending");
		}

		private static IOrderedQueryable<T> InternalOrderBy<T>(this IQueryable<T> query, string memberName, string methodName)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (string.IsNullOrWhiteSpace(memberName))
			{
				throw new ArgumentException("memberName does not be null or empty");
			}

			if (!query.IsColumnExists(memberName))
			{
				string message = $"Sort Column {memberName} does not exists in query {query}";
				throw new System.Data.InvalidExpressionException(message);
			}

			var typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };

			var pi = typeof(T).GetProperty(memberName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

			return (IOrderedQueryable<T>)query.Provider.CreateQuery(
				Expression.Call(
					typeof(Queryable),
					methodName,
					new Type[] { typeof(T), pi!.PropertyType },
					query.Expression,
					Expression.Lambda(Expression.Property(typeParams[0], pi!), typeParams))
			);

		}

		public static IQueryable<T> WhereKey<T>(this IQueryable<T> query, Models.KeyIdList keyIdList)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (keyIdList == null
				|| !keyIdList.KeyList.Any()
				|| string.IsNullOrWhiteSpace(keyIdList.PropertyName))
			{
				throw new ArgumentException("idKeyList does not be null or list empty or propertyName");
			}

			if (!query.IsColumnExists(keyIdList.PropertyName))
			{
				string message = $"Column {keyIdList.PropertyName} does not exists in query {query.ToString()}";
				throw new System.Data.InvalidExpressionException(message);
			}

			var parameter = Expression.Parameter(typeof(T));
			var property = Expression.Property(parameter, keyIdList.PropertyName);

			Expression<Func<T, bool>>? expression = null;
			Expression? body = null;

			if (keyIdList.KeyList.Count == 1)
			{
				var keyId = keyIdList.KeyList.First();
				var constant = ConvertKeyIdFrom(keyId, property.Type);
				body = Expression.Equal(property, constant);
			}
			else
			{
				var listType = typeof(List<>).MakeGenericType(new Type[] { property.Type });
				var instance = (System.Collections.IList)Activator.CreateInstance(listType)!;
				foreach (var item in keyIdList.KeyList)
				{
					if (item == null)
					{
						continue;
					}

					object? itemValue = null;
					if (item is IConvertible)
					{
						if (item is string)
						{
							itemValue = System.ComponentModel.TypeDescriptor.GetConverter(property.Type)
												.ConvertFromInvariantString($"{item}");
						}
						else
						{
							var ci = new System.Globalization.CultureInfo("en-us");
							itemValue = Convert.ChangeType(item, property.Type, ci);
						}
					}
					else
					{
						itemValue = System.ComponentModel.TypeDescriptor.GetConverter(property.Type)
											.ConvertFromInvariantString($"{item}");
					}

					if (itemValue != null)
					{
						instance.Add(itemValue);
					}
				}
				var containsMethod = instance.GetType().GetMethod("Contains", new Type[] { property.Type })!;

				var constantList = Expression.Constant(instance, listType);
				body = Expression.Call(constantList, containsMethod, property);
			}

			expression = Expression.Lambda<Func<T, bool>>(body, parameter);
			return query.Where(expression);
		}

		public static bool IsColumnExists<T>(this IQueryable<T> query, string columnName)
		{
			var elementType = query.ElementType;
			foreach (var pi in elementType.GetProperties())
			{
				if (pi.Name.Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		public static async Task<(int Count, IEnumerable<T> List)> GetPagedDataList<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> defaultSort, Models.IListFilter filter)
		{
			var count = 0;
			if (filter.KeyIdList.KeyList.Count == 0)
			{
				if ((filter.PageIndex == 0
					&& filter.ComputeRowCount == ComputeRowCount.OnlyInFirstPage)
					|| filter.ComputeRowCount == ComputeRowCount.InAllPages)
				{
					count = await query.CountAsync();
				}

				query = query.OrderWithFilter(filter, defaultSort, filter.SortDirection)
							.GetPagedWithFilter(filter);
			}
			else
			{
				var pageSize = Math.Max(filter.PageSize, filter.KeyIdList.KeyList.Count);

				var idList = filter
								.KeyIdList
								.KeyList
								.Skip(filter.PageIndex * pageSize)
								.Take(pageSize)
								.ToList();

				if (idList.Count == 0)
				{
					count = 0;
					return (count, new List<T>());
				}

				query = query.WhereKey(new Models.KeyIdList()
				{
					KeyList = idList,
					PropertyName = filter.KeyIdList.PropertyName
				});

				count = filter.KeyIdList.KeyList.Count;
			}

			var datalist = await query.ToListAsync();
			if (!datalist.Any())
			{
				count = 0;
			}
			return (count, datalist);
		}

		public static void SetById(this Models.IListFilter filter, object id)
		{
			filter.KeyIdList.PropertyName = "Id";
			filter.KeyIdList.KeyList.Add(id);
		}

		private static ConstantExpression ConvertKeyIdFrom(object keyId, Type type)
		{
			ConstantExpression? constant = null;
			object? key = null;
			if (keyId is IConvertible)
			{
				if (keyId is string)
				{
					key = System.ComponentModel.TypeDescriptor.GetConverter(type!).ConvertFromInvariantString($"{keyId}");
				}
				else
				{
					var ci = new System.Globalization.CultureInfo("en-US");
					key = Convert.ChangeType(keyId, type, ci);
				}
			}
			else
			{
				key = System.ComponentModel.TypeDescriptor.GetConverter(type!).ConvertFromInvariantString($"{keyId}");
			}
			constant = Expression.Constant(key);
			return constant;
		}

	}
}
