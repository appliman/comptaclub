using System.Reflection;

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
}
