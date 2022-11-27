using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;

namespace ComptaClub.Extensions
{
    public static class EnumExtensions
    {
		public static T? GetAttribute<T>(this Enum value) where T : Attribute
		{
			var type = value.GetType();
			var memberInfo = type.GetMember(value.ToString());
			var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
			return attributes.Length > 0 ? (T)attributes[0] : null;
		}

		public static string ToName(this Enum value)
        {
			var attribute = value.GetAttribute<DisplayAttribute>();
			return attribute == null ? $"{value}" : attribute.Name!;
		}

		public static List<Models.EnumExtension> GetEnumExtensions<T>()
			where T : struct
		{
			return GetEnumExtensions(typeof(T));
		}

		public static List<EnumExtension> GetEnumExtensions(this Type enumeration)
		{
			var result = new List<EnumExtension>();
			if (enumeration == null)
			{
				return result;
			}
			var values = Enum.GetValues(enumeration);

			foreach (var value in values)
			{
				var fi = value.GetType().GetField($"{value}");
				var attr = fi!.GetCustomAttribute<DisplayAttribute>();
				if (attr != null)
				{
					result.Add(new EnumExtension
					{
						Key = (int)value,
						Name = attr.Name,
						Description = attr.Description ?? attr.Name
					});
				}
				else
				{
					result.Add(new EnumExtension
					{
						Key = (int)value,
						Name = fi!.Name,
						Description = fi.Name
					});
				}
			}
			return result;
		}

	}
}
