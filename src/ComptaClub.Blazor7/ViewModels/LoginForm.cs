using System.ComponentModel.DataAnnotations;

namespace ComptaClub.Blazor.ViewModels;

public class LoginForm
{
	[Required]
	public string? Email { get; set; }
	public string Step { get; set; } = "Email";
	public int? Digicode { get; set; }
	public Guid TokenId { get; set; } = Guid.NewGuid();
	public Datas.UserData? User { get; set; }

	public int? GeneratedDigicode { get; set; }
	public DateTime ExpirationDate { get; set; } = DateTime.Now.AddMinutes(10);
}
