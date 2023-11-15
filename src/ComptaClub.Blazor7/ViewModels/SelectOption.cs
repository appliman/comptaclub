namespace ComptaClub.Blazor.ViewModels
{
	public class SelectOption<T>
	{
		public SelectOption(T value, string text, bool selected)
		{
			Value = value;
			Text = text;
			Selected = selected;
		}

		public T Value { get; set; }
		public string Text { get; set; }
		public bool Selected { get; set; }
	}
}
