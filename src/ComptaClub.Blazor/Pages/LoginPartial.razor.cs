using ComptaClub.Blazor.ViewModels;
using ComptaClub.Contracts.Models.Users;
using ComptaClub.Contracts.Results;

using FluentEmail.Core;
using FluentEmail.Core.Models;

using MediatR;

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
	IFluentEmail FluentEmail { get; set; } = default!;
	[Inject]
	Configuration.ComptaClubSettings GlobalSettings { get; set; } = default!;


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
				Cache.Set($"login:{loginForm.TokenId}", loginForm);
				Logger.LogInformation("Digicode : {0}", loginForm.GeneratedDigicode);

				var sendResult = await SendEmailConnection();
				if (!sendResult.Successful)
				{
					Logger.LogError(string.Join(",", sendResult.ErrorMessages));
					errors.Add(
						new BrokenRule
						{
							PropertyName = "Email",
							MessageList = new List<string>() { "L'envoi du mail a échoué" }
						}
					);
				}
				else
				{
					submitMessage = "Connexion";
					loginForm.Step = "Digicode";
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

	async Task<SendResponse> SendEmailConnection()
	{
		var emailTemplatesFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
		emailTemplatesFolder = Path.Combine(emailTemplatesFolder, "Pages", "EmailTemplates", "digicode.cshtml");

		var email = FluentEmail.SetFrom(GlobalSettings.ContactEmailAdress, GlobalSettings.ContactName);
		email.To(loginForm.Email);
		email.Subject("Votre code d'accès");
		email.UsingTemplateFromFile(emailTemplatesFolder, loginForm);
		email.Tag("workaround");

		var errorMessage = string.Empty;
		try
		{
			var sendResult = await email.SendAsync();
			return sendResult;
		}
		catch (Exception ex)
		{
			ex.Data["TemplateFolder"] = emailTemplatesFolder;
			ex.Data["From"] = GlobalSettings.ContactEmailAdress;
			ex.Data["Email"] = loginForm.Email;
			Logger.LogError(ex, ex.Message);
			errorMessage = ex.Message;
		}
		return new SendResponse()
		{
			ErrorMessages = new List<string>() { errorMessage }
		};
	}
}


