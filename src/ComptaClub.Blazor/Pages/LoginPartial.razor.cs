using ComptaClub.Blazor.ViewModels;
using ComptaClub.Contracts.Models.Users;
using ComptaClub.Contracts.Results;
using ComptaClub.Mail;

using ChannelMediator;

using Microsoft.Extensions.Caching.Memory;

namespace ComptaClub.Blazor.Pages;

public partial class LoginPartial : ComponentBase
{
	[Inject]
	protected NavigationManager NavigationManager { get; set; } = default!;
	[Inject]
	ILogger<LoginPartial> Logger { get; set; } = default!;
	[Inject]
	IMediator Mediator { get; set; } = default!;
	[Inject]
	IMemoryCache Cache { get; set; } = default!;
	[Inject]
	DigicodeEmailSender EmailSender { get; set; } = default!;

    LoginForm loginForm = new();
	Components.CustomValidator? customValidator = new();
	string submitMessage = "Envoyer le code d'accès";

	public async Task Validate()
	{
		var errors = new List<BrokenRule>();
		if (loginForm.Step == "Email")
		{
			var user = await Mediator.Send(new GetUserByFilterRequest(i => i.Email = loginForm.Email)) ;
			if (user == null)
			{
				errors.Add(
					new BrokenRule
					{
						PropertyName = "Email",
						MessageList = new List<string>() { "Adresse email inconnue" }
					});
			}
			else
			{
				loginForm.User = user;
				loginForm.GeneratedDigicode = CreateDigicode();
				try
				{
					await EmailSender.SendAsync(loginForm.Email!, loginForm.GeneratedDigicode.Value);
					Cache.Set($"login:{loginForm.TokenId}", loginForm);
					submitMessage = "Connexion";
					loginForm.Step = "Digicode";
				}
				catch (Exception ex)
				{
					Logger.LogError(ex, "L'envoi du code de connexion a échoué");
					errors.Add(
						new BrokenRule
						{
							PropertyName = "Email",
							MessageList = new List<string>() { "L'envoi du mail a échoué" }
						}
					);
				}
			}
		}
		else if (loginForm.Step == "Digicode")
		{
			if (!loginForm.Digicode.HasValue)
			{
				errors.Add(new BrokenRule()
				{
					PropertyName = "Digicode",
					MessageList = new List<string>() { "Vous devez indiquer un digicode" }
				}
				);
			}
			else
			{
				if (loginForm.Digicode.Value != loginForm.GeneratedDigicode)
				{
					errors.Add(new BrokenRule()
					{
						PropertyName = "Digicode",
						MessageList = new List<string>() { "Digicode invalide" }
					});
				}
				else
				{
					NavigationManager.NavigateTo($"/authenticate/{loginForm.TokenId}", true);
				}
			}
		}

		if (errors.Any())
		{
			customValidator!.DisplayErrors(errors);
			return;
		}

		loginForm.Step = "Digicode";
	}


	int CreateDigicode()
	{
		var digiCode = new System.Text.StringBuilder();
		int result = 0;
		var rnd = new Random();
		for (int i = 0; i < 5; i++)
		{
			var digit = $"{rnd.Next(8) + 1}";
			digiCode.Append(digit);
		}
		result = Convert.ToInt32($"{digiCode}");

		return result;
	}

}


