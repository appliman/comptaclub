using System.Text.Json;
using System.Text.Json.Serialization;

using ComptaClub;
using ComptaClub.Blazor.Pages;
using ComptaClub.Blazor.Services;
using ComptaClub.Datas.MsSql;
using ComptaClub.Datas.Sqlite;
using ComptaClub.EntityFramework;
using ComptaClub.Mail;


using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using SuperBlazorComponents;

var builder = WebApplication.CreateBuilder(args);

var globalSettings = builder.ConfigureComptaClub(args);

switch (globalSettings.DatabaseProvider.Trim().ToLowerInvariant())
{
    case "mssql":
        builder.Services.AddComptaClubMsSql(globalSettings.ConnectionString, builder.Environment.EnvironmentName);
        break;
    case "sqlite":
        builder.Services.AddComptaClubSqlite(globalSettings.ConnectionString, builder.Environment.EnvironmentName);
        break;
    default:
        throw new InvalidOperationException($"Unsupported database provider: {globalSettings.DatabaseProvider}");
}

builder.Services.AddSuperComponents();
builder.Services.AddScoped(_ => new SuperBlazorComponents.Services.SuperNotificationService { DefaultIsHtml = false });
builder.Services.AddScoped<ComptaClub.Blazor.Services.PrintService>();
builder.Services.AddScoped<ComptaClub.Blazor.Services.ListFilterQueryStringParametersService>();
builder.Services.AddSingleton<ComptaClub.Blazor.Services.EntityContextService>();

builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();

builder.Services.AddMemoryCache();

builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
		options.JsonSerializerOptions.PropertyNamingPolicy = null;
		options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
	});

builder.Services.AddDataProtection()
		.SetApplicationName(globalSettings.ApplicationName)
		.AddKeyManagementOptions(options =>
		{
			options.AutoGenerateKeys = true;
		})
		.PersistKeysToDbContext<ComptaClubDbContext>()
		.SetDefaultKeyLifetime(TimeSpan.FromDays(400));

builder.Services.AddLocalization();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
		.AddCookie(options =>
		{
			options.Cookie.Name = "ComptaClub";
			options.SlidingExpiration = true;
			options.ExpireTimeSpan = TimeSpan.FromDays(15);
			options.Cookie.HttpOnly = true;
		});

builder.Services.AddSingleton<DigicodeEmailSender>();

builder.Logging.AddOpenTelemetry(options =>
{
	options.IncludeScopes = true;
	options.IncludeFormattedMessage = true;
	options.ParseStateValues = true;
	options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService($"{builder.Environment.EnvironmentName}.ComptaClub"));
	options.AddOtlpExporter(opt =>
	{
		opt.Endpoint = new Uri($"{globalSettings.OtlpEndpoint}");
		// opt.Headers = settings.OltpHeaders;
		opt.Protocol = OtlpExportProtocol.Grpc;
	});
});


/* ----------------------------------------------------------------------- */

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
}
else
{
	app.UseDeveloperExceptionPage();
}

app.UseRequestLocalization("fr-FR");

app.UseRouting();
app.MapControllers();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();


app.MapRazorComponents<App>()
		.AddInteractiveServerRenderMode();

await app.Services.GetRequiredService<IComptaClubDbContextFactory>().MigrateAsync();

var mediator = app.Services.GetRequiredService<ChannelMediator.IMediator>();
await mediator.Send(new WarmupRequest());

await app.RunAsync();
