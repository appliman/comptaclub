using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace ComptaClub.Tests;

[TestClass]
public class LocalConfigurationTests
{
    [TestMethod]
    public void Development_loads_secrets_from_local_json()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"comptaclub-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);

        try
        {
            File.WriteAllText(Path.Combine(folder, "appsettings.local.json"), """
                {
                  "ComptaClub": {
                    "ConnectionString": "Server=localhost;Database=LocalTest",
                    "AzureStorageConnectionString": "UseDevelopmentStorage=true",
                    "SmtpPassword": "local-password"
                  }
                }
                """);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = folder,
                EnvironmentName = Environments.Development
            });

            var settings = builder.ConfigureComptaClub();

            Assert.AreEqual("Server=localhost;Database=LocalTest", settings.ConnectionString);
            Assert.AreEqual("local-password", settings.SmtpPassword);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
