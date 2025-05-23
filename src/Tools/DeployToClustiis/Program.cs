// https://appliman33.webhook.office.com/webhookb2/d61b78d8-95bd-48ea-b6b4-2e077e34c712@21a2eeea-a886-49ec-a0c9-a2d53e5f6dde/IncomingWebhook/a46fd52356154b0494e8275b33f134cc/1c8df00a-729a-4b6e-8c86-0158029b620a/V2jc032c5x4qFi6rkruRgW6umZtbFqx7QVjQFFIBW8E3I1

using System.Reflection;
using System.Text;

using DeployToClustiis;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Deploy to Clustiis");
Console.WriteLine("-----------------");

var builder = WebApplication.CreateBuilder(args);

var env = args.GetParameterValue("--env");

var copyLocal = false;
if (!string.IsNullOrWhiteSpace(env))
{
	builder.Environment.EnvironmentName = env;

	builder.Configuration
		.AddJsonFile("appsettings.json")
		.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true);
}
else
{
	copyLocal = true;
	builder.Configuration
	.AddJsonFile("appsettings.json")
	.AddJsonFile("appsettings.local.json", true);
}

var configuration = new DeployToClustiisConfiguration();
builder.Configuration.GetSection("DeployToClustiis").Bind(configuration);

var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
configuration.RootPath = System.IO.Path.Combine(location, configuration.RootPath);
var directory = new System.IO.DirectoryInfo(configuration.RootPath);

System.Diagnostics.Debug.WriteLine(directory.FullName);
Console.WriteLine(directory.FullName);

var projectListToDeploy = new List<Project>()
{
	new Project
	{
		Name = "ComptaClub",
		CsprojFileName = System.IO.Path.Combine(directory.FullName, "ComptaClub.Blazor\\ComptaClub.Blazor.csproj"),
		PublishPath = System.IO.Path.Combine(directory.FullName, "ComptaClub.Blazor\\bin\\debug\\net9.0\\publish"),
	},
};

foreach (var project in projectListToDeploy)
{
	Helpers.Process("dotnet", $"publish -c Debug {project.CsprojFileName}");

	var envTxtFileName = System.IO.Path.Combine(project.PublishPath, $"env.txt");
	var envTest = new StringBuilder();
	envTest.AppendLine(configuration.Environment);
	if (!string.IsNullOrWhiteSpace(project.ClientName))
	{
		envTest.AppendLine(project.ClientName);
	}
	await System.IO.File.WriteAllTextAsync(envTxtFileName, envTest.ToString());

	// On supprime le fichier appsettings.local.json 
	string excludeInZip = string.Empty;
	if (!copyLocal)
	{
		var appSettingsLocalFileName = System.IO.Path.Combine(project.PublishPath, $"appsettings.local.json");
		if (System.IO.File.Exists(appSettingsLocalFileName))
		{
			System.IO.File.Delete(appSettingsLocalFileName);
		}
		excludeInZip = "-x!appsettings.local.json";
	}

	// Supprimer le fichier zip
	var zipFileName = System.IO.Path.Combine(project.PublishPath, "..\\", $"publish.zip");
	if (System.IO.File.Exists(zipFileName))
	{
		System.IO.File.Delete(zipFileName);
	}
	Helpers.Process(@"""C:\Program Files\7-Zip\7z.exe""", @$"a -tzip -r {project.PublishPath} *", project.PublishPath);

	var fileInfo = new System.IO.FileInfo(zipFileName);

	using var form = new MultipartFormDataContent();
	form.Headers.ContentType!.MediaType = "multipart/form-data";

	var content = new FileStream(zipFileName, FileMode.Open);
	var stream = new StreamContent(content, (int)fileInfo.Length);
	form.Add(stream, $"{project.Name}.zip", $"{project.Name}.zip");

	using var httpClient = new HttpClient();
	httpClient.BaseAddress = new Uri(configuration.StagingUrl);
	httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("BASIC", configuration.ClustiisApiKey);

	Console.WriteLine("try to upload to {0}/api/upload-package", configuration.StagingUrl);
	var response = await httpClient.PostAsync("/api/upload-package", form);
	var responseContent = response.Content.ReadAsStringAsync();
	response.EnsureSuccessStatusCode();
}