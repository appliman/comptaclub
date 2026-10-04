using ClosedXML.Excel;
using ComptaClub.Datas;
using ComptaClub.EntityFramework;
using ComptaClub.Enums;
using ComptaClub.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests;

[TestClass]
public sealed class McpFileAndHierarchyTests
{
    [TestMethod]
    public async Task Excel_import_persists_valid_members_and_rejects_invalid_files()
    {
        await using var _host = await McpTestHost.Create();
        using var _workbook = new XLWorkbook();
        var _sheet = _workbook.AddWorksheet("Membres");
        var _headers = new[] { "Numéro de licence", "Prénom", "Nom", "Email", "Type de licence" };
        var _values = new[] { "MCP-XLSX-1", "Camille", "Test", "camille@example.org", "Adhérent" };
        for (var _column = 0; _column < _headers.Length; _column++)
        {
            _sheet.Cell(1, _column + 1).Value = _headers[_column];
            _sheet.Cell(2, _column + 1).Value = _values[_column];
        }
        using var _stream = new MemoryStream();
        _workbook.SaveAs(_stream);
        var _content = Convert.ToBase64String(_stream.ToArray());
        var _imported = McpTestHost.Data(await _host.Tool("import_members_excel", new { fileName = "membres.xlsx", contentBase64 = _content }));
        Assert.AreEqual(1, _imported.GetProperty("importedCount").GetInt32());
        await using var _db = await _host.App.Services.GetRequiredService<IComptaClubDbContextFactory>().CreateDbContextAsync();
        Assert.AreEqual("Camille Test", (await _db.Members.SingleAsync()).Name);
        var _repeat = McpTestHost.Data(await _host.Tool("import_members_excel", new { fileName = "membres.xlsx", contentBase64 = _content }));
        Assert.AreEqual(0, _repeat.GetProperty("importedCount").GetInt32());
        Assert.IsTrue(_repeat.GetProperty("hasWarning").GetBoolean());
        _sheet.Cell(2, 1).Value = "MCP-XLSX-INVALID";
        _sheet.Cell(2, 4).Value = "adresse invalide";
        using var _invalidStream = new MemoryStream();
        _workbook.SaveAs(_invalidStream);
        var _invalid = await _host.Tool("import_members_excel", new { fileName = "membres.xlsx", contentBase64 = Convert.ToBase64String(_invalidStream.ToArray()) });
        Assert.IsTrue(_invalid.GetProperty("isError").GetBoolean());
        Assert.IsTrue(_invalid.GetProperty("structuredContent").GetProperty("errors").GetArrayLength() > 0);
        foreach (var _file in new[] { "membres.csv", "../membres.xlsx" })
        {
            Assert.IsTrue((await _host.Tool("import_members_excel", new { fileName = _file, contentBase64 = _content })).GetProperty("isError").GetBoolean());
        }
        Assert.IsTrue((await _host.Tool("import_members_excel", new { fileName = "invalide.xlsx", contentBase64 = "aW52YWxpZA==" })).GetProperty("isError").GetBoolean());
    }

    [TestMethod]
    public async Task Account_updates_reject_cycles_and_missing_parents()
    {
        await using var _host = await McpTestHost.Create();
        var _parent = McpTestHost.Data(await _host.Tool("create_account", new { code = "7", label = "Produits", direction = "Credit" })).GetProperty("id").GetGuid();
        var _child = McpTestHost.Data(await _host.Tool("create_account", new { code = "75", label = "Cotisations", direction = "Credit", parentId = _parent })).GetProperty("id").GetGuid();
        var _cycle = await _host.Tool("update_account", new { id = _parent, code = "7", label = "Produits", direction = "Credit", parentId = _child });
        Assert.IsTrue(_cycle.GetProperty("isError").GetBoolean());
        var _missing = await _host.Tool("create_account", new { code = "76", label = "Parent absent", direction = "Credit", parentId = Guid.NewGuid() });
        Assert.IsTrue(_missing.GetProperty("isError").GetBoolean());
        await using var _db = await _host.App.Services.GetRequiredService<IComptaClubDbContextFactory>().CreateDbContextAsync();
        Assert.AreEqual(2, await _db.Accounts.CountAsync());
        Assert.IsNull((await _db.Accounts.SingleAsync(item => item.Id == _parent)).ParentAccountId);
    }

    [TestMethod]
    public void Deep_budget_totals_are_recomputed_from_leaves_and_cycles_are_rejected()
    {
        var _root = new ForecastBudgetItemData { Id = Guid.NewGuid(), Amount = 999, Direction = AccountDirection.Credit };
        var _branch = new ForecastBudgetItemData { Id = Guid.NewGuid(), ParentForecastBudgetItemId = _root.Id, Amount = 999, Direction = AccountDirection.Credit };
        var _leaf = new ForecastBudgetItemData { Id = Guid.NewGuid(), ParentForecastBudgetItemId = _branch.Id, Amount = 12340000, Direction = AccountDirection.Credit };
        var _budget = new ForecastBudgetData { ItemList = [_root, _branch, _leaf] };
        _budget.ItemList.Levelize();
        _budget.ItemList.Hierarchize();
        _budget.ComputeTotal();
        Assert.AreEqual(12340000L, _root.Amount);
        Assert.AreEqual(12340000L, _branch.Amount);
        Assert.AreEqual(12340000L, _budget.CreditTotal);
        _root.ParentForecastBudgetItemId = _leaf.Id;
        Assert.ThrowsExactly<InvalidDataException>(() => new[] { _root, _branch, _leaf }.Levelize());
    }
}
