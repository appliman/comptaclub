namespace ComptaClub.ViewModels
{
	public class RadioValue<T>
	{
		public T Value { get; set; } = default(T)!;
		public string Text { get; set; } = null!;
		public bool Selected { get; set; }
	}
}
