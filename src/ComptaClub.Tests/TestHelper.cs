using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Models.Banks;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Models.Users;
using ComptaClub.Configuration;
using ComptaClub.Datas;
using ComptaClub.Datas.Sqlite;
using ComptaClub.EntityFramework;
using ComptaClub.Extensions;

using ChannelMediator;

using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests
{
	public static class TestHelper
	{
		public async static Task<WebApplication> CreateWebApplication()
		{
			var builder = WebApplication.CreateBuilder();
			builder.Environment.EnvironmentName = "Test";
			builder.Configuration.AddJsonFile("appsettings.test.json", optional: false);

			var configuredConnectionString = builder.Configuration.GetConnectionString("TEST")
				?? throw new InvalidOperationException("ConnectionStrings:TEST is required.");
			var sqliteConnectionString = new SqliteConnectionStringBuilder(configuredConnectionString)
			{
				DataSource = $"ComptaClubTest-{Guid.NewGuid():N}"
			}.ConnectionString;

			var settings = builder.ConfigureComptaClub();
			settings.DatabaseProvider = "Sqlite";
			settings.ConnectionString = sqliteConnectionString;
			builder.Services.AddSingleton(_ => new SqliteConnection(sqliteConnectionString));
			builder.Services.AddComptaClubSqlite(sqliteConnectionString, builder.Environment.EnvironmentName);
			var app = builder.Build();
			try
			{
				await app.Services.GetRequiredService<SqliteConnection>().OpenAsync();
				await app.Services.GetRequiredService<IComptaClubDbContextFactory>().MigrateAsync();
				return app;
			}
			catch
			{
				await app.DisposeAsync();
				throw;
			}
		}

		public async static Task<List<Datas.AccountData>> GetOrCreatePlan(this IMediator mediator)
		{
			var planFile = System.IO.Path.Combine(System.Environment.CurrentDirectory, "InitialAccountingPlan.json");
			var planFileContent = System.IO.File.ReadAllText(planFile);

			var plan = System.Text.Json.JsonSerializer.Deserialize<List<Datas.AccountData>>(planFileContent, ComptaClub.JsonSerializer.Options);

			await mediator.Send(new ImportAccountingPlanRequest(plan!));

			plan = await mediator.Send(new GetPlanRequest());

			return plan!;
		}

		public async static Task<Datas.BankData> GetOrCreateBank(this IMediator mediator, string bankName)
		{
			var bank = await mediator.Send(new GetBankByFilterRequest(i => i.Code == bankName));
			if (bank == null)
			{
				bank = await mediator.Send(new CreateBankRequest("MyBank", "My Bank"));
				await mediator.Send(new SaveEntityRequest<Datas.BankData>(bank));
			}
			return bank;
		}

		public async static Task<Datas.ExerciceData> GetOrCreateExercice(this ChannelMediator.IMediator mediator, string code)
		{
			var exercice = await mediator.Send(new GetExerciceByFilterRequest(i => i.Code == code));
			if (exercice == null)
			{
				exercice = await mediator.Send(new CreateExerciceRequest(code, "test", 0));
				await mediator.Send(new SaveEntityRequest<Datas.ExerciceData>(exercice));
			}
			return exercice;
		}

		public async static Task<Datas.UserData> GetOrCreateUser(this IMediator mediator, string email)
		{
			var user = await mediator.Send(new GetUserByFilterRequest(i => i.Email = email));
			if (user == null)
			{
				user = await mediator.Send(new CreateUserRequest($"{Guid.NewGuid()}", email));
				await mediator.Send(new SaveEntityRequest<Datas.UserData>(user));
			}
			return user;
		}

		public static string GetRandomName()
		{
			return $"{Guid.NewGuid()}";
		}

		public async static Task<EntryData?> CreateAndSaveRandomCreditEntry(this IMediator mediator,
			ExerciceData exercice,
			UserData user,
			BankData bank,
			Guid accountId,
			DateTime entryDate)
		{
			var random = new Random(100);
			var amount = random.Next(1, 100);
			var entry = await mediator.Send(new CreateEntryRequest());
			entry.CreationDate = entry.ValueDate = entryDate.ToDayId();
			entry.Label = GetRandomName();
			entry.PartNumber = GetRandomName();
			entry.BankId = bank.Id;
			entry.AccountId = accountId;
			entry.ExerciceId = exercice.Id;
			entry.Amount = amount * 1000000;
			entry.AccountDirection = Enums.AccountDirection.Credit;
			entry.PaymentType = Enums.PaymentType.Transfer;
			entry.UserCreatorId = user.Id;

			var saveEntryResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));

			return saveEntryResult.HasError ? null : entry;
		}

		public async static Task<EntryData?> CreateAndSaveRandomDebitEntry(this IMediator mediator,
			ExerciceData exercice,
			UserData user,
			BankData bank,
			Guid accountId,
			DateTime entryDate)
		{
			var random = new Random(100);
			var amount = random.Next(1, 100);
			var entry = await mediator.Send(new CreateEntryRequest());
			entry.CreationDate = entry.ValueDate = entryDate.ToDayId();
			entry.Label = GetRandomName();
			entry.PartNumber = GetRandomName();
			entry.BankId = bank.Id;
			entry.AccountId = accountId;
			entry.ExerciceId = exercice.Id;
			entry.Amount = amount * 1000000;
			entry.AccountDirection = Enums.AccountDirection.Debit;
			entry.PaymentType = Enums.PaymentType.Debit;
			entry.UserCreatorId = user.Id;

			var saveEntryResult = await mediator.Send(new SaveEntityRequest<Datas.EntryData>(entry));

			return saveEntryResult.HasError ? null : entry;
		}

	}
}
