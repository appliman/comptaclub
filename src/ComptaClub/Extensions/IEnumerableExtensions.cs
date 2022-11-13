namespace ComptaClub.Extensions
{
    public static class IEnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            if (list == null)
            {
                return false;
            }
            return list.Any();
        }
    }
}
