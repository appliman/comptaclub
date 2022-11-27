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

var builder = WebApplication.CreateBuilder(args);

var cfg = await builder.ConfigureComptaClub();

var migration = new DbMigration()
{
    ConnectionString = cfg.settings.SqlConnectionString,
    SchemaName = "ComptaClub",
    EmbededTypeReference = typeof(ComptaClub.Datas.StartupExtensions)
};

builder.Services.AddAutoMapper(typeof(Program));

await migration.Start();

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

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = cfg.settings.CookieName;
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(15);
                    options.Cookie.HttpOnly = true;
                });

var rootUri = new Uri($"https://{cfg.settings.AzureStorageAccountName}.blob.core.windows.net");
var storageCredential = new StorageSharedKeyCredential(cfg.settings.AzureStorageAccountName, cfg.settings.AzureStorageAccountKey);
var blobServiceClient = new BlobServiceClient(rootUri, storageCredential);

var response = blobServiceClient.GetBlobContainerClient(cfg.settings.AzureStorageWebAppDataProtectionContainerName);
if (!await response.ExistsAsync())
{
    await blobServiceClient.CreateBlobContainerAsync(cfg.settings.AzureStorageWebAppDataProtectionContainerName);
}

var keyUri = new Uri($"https://{cfg.settings.AzureStorageAccountName}.blob.core.windows.net/{cfg.settings.AzureStorageWebAppDataProtectionContainerName}/{cfg.settings.DataProtectionFileName}");
var blobClient = new BlobClient(keyUri, storageCredential);

builder.Services.AddDataProtection()
        .SetApplicationName(cfg.settings.ApplicationName)
        .AddKeyManagementOptions(options =>
        {
            options.AutoGenerateKeys = true;
        })
        .PersistKeysToAzureBlobStorage(blobClient)
        .SetDefaultKeyLifetime(TimeSpan.FromDays(400));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
