namespace ComptaClub.Blazor.Services;

public interface IContextRefreshable
{
    Task RefreshFromContext(object metaEntity);
}
