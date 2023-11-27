using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Models.Banks;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Models.Users;
using ComptaClub.Datas;
using ComptaClub.Extensions;

using EFScriptableMigration;

using MediatR;

using Microsoft.AspNetCore.Builder;
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

			var cs = builder.Configuration.GetConnectionString("TEST");
			var dbTest = new Datas.ComptaClubDbContext(new Datas.DbConfiguration
			{
				ConnectionString = cs!,
				EnvironmentName = builder.Environment.EnvironmentName
			});
			await dbTest.Database.EnsureDeletedAsync();
			await dbTest.Database.EnsureCreatedAsync();

			builder.Environment.EnvironmentName = "Development";
			var args = new string[]
			{
				"--env", "test",
				"--cs", cs!
			};

			var cfg = await builder.ConfigureComptaClub(args);


			var migration = new DbMigration()
			{
				ConnectionString = cfg.SqlConnectionString,
				SchemaName = "ComptaClub",
				EmbededTypeReference = typeof(StartupExtensions)
			};

			await migration.Start();

			var app = builder.Build();

			return app;
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

		public async static Task<Datas.ExerciceData> GetOrCreateExercice(this MediatR.IMediator mediator, string code)
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


		public async static Task CleanupDatabase(this IServiceProvider serviceProvider)
		{
			var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<Datas.ComptaClubDbContext>>();
			var db = await dbContextFactory.CreateDbContextAsync();

			await db.Database.BeginTransactionAsync();

			await db.Accounts.ExecuteDeleteAsync();
			await db.Banks.ExecuteDeleteAsync();
			await db.DocumentsByEntities.ExecuteDeleteAsync();
			await db.Documents.ExecuteDeleteAsync();
			await db.Exercices.ExecuteDeleteAsync();
			await db.Members.ExecuteDeleteAsync();
			await db.RolesByUsers.ExecuteDeleteAsync();
			await db.Users.ExecuteDeleteAsync();
			await db.AssociatedMemberListByEntries.ExecuteDeleteAsync();

			await db.Database.CommitTransactionAsync();
		}
	}
}
