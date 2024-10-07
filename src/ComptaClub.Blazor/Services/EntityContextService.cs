using System.Reflection;

using ComptaClub.Blazor.ViewModels;

namespace ComptaClub.Blazor.Services;

public class EntityContextService
{
	private IEnumerable<ContextInfo>? _contextInfos;

	public event EventHandler<string> ChangeTabByName = default!;
	public event EventHandler<string> TabChanged = default!;

	public IEnumerable<ContextInfo> GetContextInfoList(MetaEntity metaEntity, ContextLocation contextLocation)
	{
		var result = new List<ContextInfo>();
		result.AddRange(GetAllContextInfos().Where(ci => ci.MetaEntity == metaEntity && ci.ContextLocation == contextLocation));
		return result;
	}

	IEnumerable<ContextInfo> GetAllContextInfos()
	{
		if (_contextInfos is not null)
		{
			return _contextInfos;
		}
		// On récupère les composants par réflexion
		var componentTypes = from type in AppDomain.CurrentDomain.GetAssemblies()
								.Where(a => a.FullName is not null && a.FullName.StartsWith("ComptaClub"))
								.SelectMany(i => i.GetTypes())
							 where type.GetCustomAttributes<EntityContextAttribute>() is not null
							 select type;

		var list = new List<ContextInfo>();
		foreach (var type in componentTypes)
		{
			var attrs = type.GetCustomAttributes<EntityContextAttribute>()!;
			if (attrs is not null)
			{
				foreach (var attr in attrs)
				{
					list.Add(new ContextInfo()
					{
						MetaEntity = attr.MetaEntity,
						ComponentType = type,
						Title = attr.Title,
						ExcludeRoutes = attr.ExcludeRouteLists,
						Icon = attr.Icon,
						IconColor = attr.IconColor,
						DefaultPosition = attr.DefaultPosition,
						ContextLocation = attr.ContextLocation
					});
				}
			}
		}
		_contextInfos = list;
		return _contextInfos;
	}

	public void OnChangeTabName(string tabName)
	{
		ChangeTabByName?.Invoke(this, tabName);
	}

	public void OnTabChanged(string tabName)
	{
		TabChanged?.Invoke(this, tabName);
	}
}
