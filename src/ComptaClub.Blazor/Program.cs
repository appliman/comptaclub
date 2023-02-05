using System.Text.Json.Serialization;
using System.Text.Json;

using ComptaClub;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;
using Azure.Storage;
using EFScriptableMigration;
using System.Data;
using Microsoft.AspNetCore.Identity;
using System.Net;
using FluentEmail.MailKitSmtp;

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

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

var rootUri = new Uri($"https://{globalSettings.AzureStorageAccountName}.blob.core.windows.net");
var storageCredential = new StorageSharedKeyCredential(globalSettings.AzureStorageAccountName, globalSettings.AzureStorageAccountKey);
var blobServiceClient = new BlobServiceClient(rootUri, storageCredential);

var response = blobServiceClient.GetBlobContainerClient(globalSettings.AzureStorageWebAppDataProtectionContainerName);
if (!await response.ExistsAsync())
{
    await blobServiceClient.CreateBlobContainerAsync(globalSettings.AzureStorageWebAppDataProtectionContainerName);
}

var keyUri = new Uri($"https://{globalSettings.AzureStorageAccountName}.blob.core.windows.net/{globalSettings.AzureStorageWebAppDataProtectionContainerName}/{globalSettings.DataProtectionFileName}");
var blobClient = new BlobClient(keyUri, storageCredential);

builder.Services.AddDataProtection()
        .SetApplicationName(globalSettings.ApplicationName)
        .AddKeyManagementOptions(options =>
        {
            options.AutoGenerateKeys = true;
        })
        .PersistKeysToAzureBlobStorage(blobClient)
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

var fluentEmail = builder.Services.AddFluentEmail("test@email.com")
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRequestLocalization("fr-FR");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

var migration = new DbMigration()
{
	ConnectionString = globalSettings.SqlConnectionString,
	SchemaName = "ComptaClub",
	EmbededTypeReference = typeof(ComptaClub.Datas.StartupExtensions)
};

await migration.Start();

var mediator = app.Services.GetRequiredService<MediatR.IMediator>();
await mediator.Send(new ComptaClub.Requests.WarmupRequest());

app.Run();
