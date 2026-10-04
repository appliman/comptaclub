using ComptaClub.Datas;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace ComptaClub.DatabaseConverter;

internal static class TableCatalog
{
    public static IReadOnlyList<ITableDefinition> Tables { get; } =
    [
        new TableDefinition<AccountData>("Accounts"),
        new TableDefinition<BankData>("Banks"),
        new TableDefinition<ClubData>("Clubs"),
        new TableDefinition<DataProtectionKey>("DataProtectionKeys", hasIdentityKey: true),
        new TableDefinition<DocumentData>("Documents"),
        new TableDefinition<DocumentContentData>("DocumentContents"),
        new TableDefinition<DocumentByEntityData>("DocumentsByEntities"),
        new TableDefinition<ExerciceData>("Exercices"),
        new TableDefinition<UserData>("Users"),
        new TableDefinition<McpApiKeyData>("McpApiKeys", isOptional: true),
        new TableDefinition<RoleByUserData>("RolesByUsers"),
        new TableDefinition<MemberData>("Members"),
        new TableDefinition<EntryData>("Entries"),
        new TableDefinition<AssociatedMemberListByEntryData>("AssociatedMemberListByEntries"),
        new TableDefinition<IncomeStatementData>("IncomeStatements"),
        new TableDefinition<IncomeStatementItemData>("IncomeStatementItems"),
        new TableDefinition<ForecastBudgetData>("ForecastBudgets"),
        new TableDefinition<ForecastBudgetItemData>("ForecastBudgetItems")
    ];
}
