namespace ComptaClub.Datas;

public class ModelRegister
{
    private List<Type> ModelList { get; set; } = new();

    public void AddModel<T>()
        where T : class, new()
    {
        if (ModelList.Contains(typeof(T)))
        {
            return;
        }
        ModelList.Add(typeof(T));
    }

    internal IEnumerable<Type> GetList()
    {
        return ModelList;
    }
}
