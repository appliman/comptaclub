using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using ComptaClub;

using EFScriptableMigration;

using FluentEmail.MailKitSmtp;

using LogRWebMonitor;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

var globalSettings = await builder.ConfigureComptaClub(args);

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ComptaClub.Blazor.Mapping.Profile>();
});

builder.Services.AddScoped<Radzen.DialogService>();
builder.Services.AddScoped<Radzen.NotificationService>();
builder.Services.AddScoped<Radzen.TooltipService>();
builder.Services.AddScoped<Radzen.ContextMenuService>();
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
        .PersistKeysToDbContext<ComptaClub.Datas.ComptaClubDbContext>()
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

var rootFolder = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
var emailTemplatesFolder = System.IO.Path.Combine(rootFolder, @$"Pages\EmailTemplates");
var outputEmails = System.IO.Path.Combine(rootFolder, @$"emailout");
if (!System.IO.Directory.Exists(outputEmails))
{
    System.IO.Directory.CreateDirectory(outputEmails);
}

var fluentEmail = builder.Services.AddFluentEmail(globalSettings.AdminUserEmail)
    .AddRazorRenderer(emailTemplatesFolder);

if (globalSettings.SmtpProviderName == "local")
{
    fluentEmail.AddSmtpSender(new System.Net.Mail.SmtpClient()
    {
        DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory,
        PickupDirectoryLocation = outputEmails
    });
}
else if (globalSettings.SmtpProviderName == "smtp")
{
    var credentials = new NetworkCredential(globalSettings.SmtpUserName, globalSettings.SmtpPassword);
    fluentEmail.AddSmtpSender(new System.Net.Mail.SmtpClient()
    {
        EnableSsl = globalSettings.SmtpEnableSsl,
        Host = globalSettings.SmtpHost,
        Port = globalSettings.SmtpPort,
        Credentials = credentials
    });
}
else if (globalSettings.SmtpProviderName == "mimekit")
{
    fluentEmail.AddMailKitSender(new SmtpClientOptions
    {
        UseSsl = globalSettings.SmtpEnableSsl,
        Server = globalSettings.SmtpHost,
        Port = globalSettings.SmtpPort,
        User = globalSettings.SmtpUserName,
        Password = globalSettings.SmtpPassword,
        RequiresAuthentication = true
    });
}

builder.AddLogRWebMonitor(config =>
{
    config.EnvironmentName = builder.Environment.EnvironmentName;
    config.HostName = "ComptaClub";
});

/* ----------------------------------------------------------------------- */

var app = builder.Build();

app.UseLogRWebMonitor();

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

app.MapRazorComponents<ComptaClub.Blazor.Pages.App>()
        .AddInteractiveServerRenderMode();

app.UseAuthentication();
app.UseAuthorization();

/*
app.MapFallbackToPage("/_Host");
app.MapBlazorHub();
app.MapRazorPages();
*/

var migration = new DbMigration()
{
    ConnectionString = globalSettings.SqlConnectionString,
    SchemaName = "ComptaClub",
    EmbededTypeReference = typeof(ComptaClub.Datas.StartupExtensions)
};

await migration.Start();

var mediator = app.Services.GetRequiredService<MediatR.IMediator>();
await mediator.Send(new WarmupRequest());

app.Run();
