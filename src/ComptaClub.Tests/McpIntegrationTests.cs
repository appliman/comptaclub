using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Datas;
using ComptaClub.EntityFramework;
using ComptaClub.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests;

[TestClass]
public sealed class McpIntegrationTests
{
    [TestMethod]
    public async Task Negotiation_discovery_and_authentication_are_isolated_from_cookies()
    {
        await using var _host = await McpTestHost.Create();
        var _init = await _host.Rpc("initialize", new { protocolVersion = "2025-11-25", capabilities = new { }, clientInfo = new { name = "ComptaClubTests", version = "1" } });
        Assert.IsTrue(_init.GetProperty("protocolVersion").GetString()!.Length > 0);
        var _tools = (await _host.Rpc("tools/list", new { })).GetProperty("tools").EnumerateArray().ToList();
        var _names = _tools.Select(tool => tool.GetProperty("name").GetString()).ToHashSet();
        foreach (var _name in new[] { "create_entry", "preview_ofx_import", "create_api_key", "update_forecast_budget", "upload_document", "update_club", "import_members_excel" })
        {
            Assert.IsTrue(_names.Contains(_name), _name);
        }
        Assert.IsTrue(_tools.Count >= 70);
        _host.Client.DefaultRequestHeaders.Authorization = null;
        using var _anonymous = await _host.Client.PostAsync("/mcp", new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.Unauthorized, _anonymous.StatusCode);
        Assert.IsNull(_anonymous.Headers.Location);
        _host.Client.DefaultRequestHeaders.Add("X-Api-Key", _host.Secret);
        McpTestHost.Data(await _host.Tool("get_application_info"));
        _host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "cc-invalid");
        using var _conflicting = await _host.Client.PostAsync("/mcp", new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.Unauthorized, _conflicting.StatusCode);
    }

    [TestMethod]
    public async Task Key_lifecycle_never_persists_plaintext_and_revocation_blocks_next_call()
    {
        await using var _host = await McpTestHost.Create();
        var _created = McpTestHost.Data(await _host.Tool("create_api_key", new { name = "Codex" }));
        var _id = _created.GetProperty("id").GetGuid();
        var _secret = _created.GetProperty("plainTextKey").GetString()!;
        var _version = _created.GetProperty("apiKey").GetProperty("version").GetGuid();
        Assert.IsTrue(McpApiKeyGenerator.TryParse(_secret, out _, out var _raw));
        await using (var _db = await _host.App.Services.GetRequiredService<IComptaClubDbContextFactory>().CreateDbContextAsync())
        {
            var _stored = await _db.McpApiKeys.SingleAsync(item => item.Id == _id);
            Assert.AreEqual(McpApiKeyGenerator.ComputeHash(_raw), _stored.SecretHash);
            Assert.IsFalse(System.Text.Json.JsonSerializer.Serialize(_stored).Contains(_raw, StringComparison.Ordinal));
        }
        var _get = McpTestHost.Data(await _host.Tool("get_api_key", new { id = _id }));
        Assert.IsFalse(_get.TryGetProperty("secretHash", out _));
        Assert.IsFalse(_get.TryGetProperty("plainTextKey", out _));
        var _rotation = McpTestHost.Data(await _host.Tool("rotate_api_key", new { id = _id, expectedVersion = _version }));
        var _newSecret = _rotation.GetProperty("plainTextKey").GetString()!;
        _host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secret);
        using var _old = await _host.Client.PostAsync("/mcp", new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.Unauthorized, _old.StatusCode);
        _host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _newSecret);
        McpTestHost.Data(await _host.Tool("get_application_info"));
        var _stale = await _host.Tool("update_api_key", new { id = _id, expectedVersion = _version, name = "Stale" });
        Assert.IsTrue(_stale.GetProperty("isError").GetBoolean());
        McpTestHost.Data(await _host.Tool("revoke_api_key", new { id = _id }));
        using var _revoked = await _host.Client.PostAsync("/mcp", new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.Unauthorized, _revoked.StatusCode);
        _host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _host.Secret);
        var _current = McpTestHost.Data(await _host.Tool("get_api_key", new { id = _id }));
        var _reactivate = await _host.Tool("update_api_key", new { id = _id, expectedVersion = _current.GetProperty("version").GetGuid(), name = "Reactivate" });
        Assert.IsTrue(_reactivate.GetProperty("isError").GetBoolean());
        McpTestHost.Data(await _host.Tool("archive_api_key", new { id = _id }));
        var _list = McpTestHost.Data(await _host.Tool("list_api_keys"));
        Assert.IsFalse(_list.GetProperty("list").EnumerateArray().Any(item => item.GetProperty("id").GetGuid() == _id));
    }

    [TestMethod]
    public async Task Expiration_creator_disablement_and_invalid_key_formats_are_rejected()
    {
        await using var _host = await McpTestHost.Create();
        var _result = await _host.Tool("create_api_key", new { name = "", expirationDateUtc = "2020-01-01T00:00:00Z" });
        Assert.IsTrue(_result.GetProperty("isError").GetBoolean());
        var _created = McpTestHost.Data(await _host.Tool("create_api_key", new { name = "Expires", expirationDateUtc = DateTime.UtcNow.AddHours(1) }));
        var _id = _created.GetProperty("id").GetGuid();
        var _secret = _created.GetProperty("plainTextKey").GetString()!;
        await using (var _db = await _host.App.Services.GetRequiredService<IComptaClubDbContextFactory>().CreateDbContextAsync())
        {
            await _db.McpApiKeys.Where(item => item.Id == _id).ExecuteUpdateAsync(update => update.SetProperty(item => item.ExpirationDateUtc, DateTime.UtcNow.AddMinutes(-1)));
        }
        _host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secret);
        using var _expired = await _host.Client.PostAsync("/mcp", new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.Unauthorized, _expired.StatusCode);
        _host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _host.Secret);
        McpTestHost.Data(await _host.Tool("disable_user", new { id = _host.UserId }));
        using var _disabled = await _host.Client.PostAsync("/mcp", new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.Unauthorized, _disabled.StatusCode);
        Assert.IsFalse(McpApiKeyGenerator.TryParse("cc-" + new string('z', 24) + "." + new string('0', 64), out _, out _));
        Assert.IsFalse(McpApiKeyGenerator.TryParse("ec-" + new string('0', 24) + "." + new string('0', 64), out _, out _));
    }

    [TestMethod]
    public async Task Business_tools_persist_validate_accounting_and_cover_every_domain()
    {
        await using var _host = await McpTestHost.Create();
        var _bank = McpTestHost.Data(await _host.Tool("create_bank", new { code = "MCP", label = "Banque MCP" })).GetProperty("id").GetGuid();
        McpTestHost.Data(await _host.Tool("activate_bank", new { id = _bank }));
        var _exercice = McpTestHost.Data(await _host.Tool("create_exercice", new { code = "2026", label = "Exercice", startDate = "2026-01-01", endDate = "2026-12-31", initialAmountEuros = 10m })).GetProperty("id").GetGuid();
        var _account = McpTestHost.Data(await _host.Tool("create_account", new { code = "756", label = "Cotisations", direction = "Credit" })).GetProperty("id").GetGuid();
        var _input = new { partNumber = "P1", label = "Cotisation", creationDate = "2026-10-04", valueDate = "2026-10-04", amountEuros = 12.34m, direction = "Credit", bankId = _bank, accountId = _account, exerciceId = _exercice, paymentType = "Transfer" };
        var _entryId = McpTestHost.Data(await _host.Tool("create_entry", new { input = _input })).GetProperty("id").GetGuid();
        var _entry = McpTestHost.Data(await _host.Tool("get_entry", new { id = _entryId }));
        Assert.AreEqual(12340000L, _entry.GetProperty("amount").GetInt64());
        Assert.AreEqual(_host.UserId, _entry.GetProperty("userCreatorId").GetGuid());
        var _invalid = await _host.Tool("create_entry", new { input = new { partNumber = "", label = "", creationDate = "2026-10-04", valueDate = "2026-10-04", amountEuros = 1, direction = "Credit", bankId = _bank, accountId = _account, exerciceId = _exercice, paymentType = "Transfer" } });
        Assert.IsTrue(_invalid.GetProperty("isError").GetBoolean());
        var _member = McpTestHost.Data(await _host.Tool("create_member", new { input = new { name = "Alice", email = "alice@example.org", state = "Active" } })).GetProperty("id").GetGuid();
        McpTestHost.Data(await _host.Tool("link_member_to_entry", new { entryId = _entryId, memberId = _member, amountEuros = 12.34m }));
        Assert.AreEqual(1, McpTestHost.Data(await _host.Tool("list_entry_members", new { entryId = _entryId })).GetProperty("items").GetArrayLength());
        var _budget = McpTestHost.Data(await _host.Tool("create_forecast_budget", new { name = "Budget", description = "Test" })).GetProperty("id").GetGuid();
        var _items = McpTestHost.Data(await _host.Tool("list_forecast_budget_items", new { id = _budget })).GetProperty("list");
        var _itemId = _items.EnumerateArray().Single().GetProperty("id").GetGuid();
        McpTestHost.Data(await _host.Tool("update_forecast_budget", new { id = _budget, name = "Budget 2026", description = "Prévision", amounts = new[] { new { itemId = _itemId, amountEuros = 20m } } }));
        Assert.IsTrue((await _host.Tool("create_income_statement", new { exerciceId = _exercice })).GetProperty("isError").GetBoolean());
        McpTestHost.Data(await _host.Tool("get_daily_balances", new { exerciceId = _exercice }));
        McpTestHost.Data(await _host.Tool("get_account_totals", new { exerciceId = _exercice }));
        McpTestHost.Data(await _host.Tool("update_club", new { input = new { name = "Club MCP", email = "club@example.org" } }));
        McpTestHost.Data(await _host.Tool("close_exercice", new { id = _exercice }));
        McpTestHost.Data(await _host.Tool("create_income_statement", new { exerciceId = _exercice }));
        Assert.IsTrue((await _host.Tool("update_entry", new { id = _entryId, input = _input })).GetProperty("isError").GetBoolean());
        Assert.IsTrue((await _host.Tool("delete_entry", new { id = _entryId })).GetProperty("isError").GetBoolean());
    }

    [TestMethod]
    public async Task Documents_roundtrip_by_chunks_and_refuse_unsafe_files()
    {
        await using var _host = await McpTestHost.Create();
        var _bytes = Enumerable.Range(0, 300000).Select(index => (byte)(index % 251)).ToArray();
        var _upload = McpTestHost.Data(await _host.Tool("upload_document", new { fileName = "test.pdf", mimeType = "application/pdf", contentBase64 = Convert.ToBase64String(_bytes) }));
        var _id = _upload.GetProperty("id").GetGuid();
        var _first = McpTestHost.Data(await _host.Tool("get_document_content", new { id = _id }));
        var _second = McpTestHost.Data(await _host.Tool("get_document_content", new { id = _id, offset = _first.GetProperty("nextOffset").GetInt32() }));
        var _actual = Convert.FromBase64String(_first.GetProperty("contentBase64").GetString()!).Concat(Convert.FromBase64String(_second.GetProperty("contentBase64").GetString()!)).ToArray();
        CollectionAssert.AreEqual(_bytes, _actual);
        Assert.IsTrue(_second.GetProperty("completed").GetBoolean());
        foreach (var _name in new[] { "../outside.pdf", "C:\\outside.pdf" })
        {
            Assert.IsTrue((await _host.Tool("upload_document", new { fileName = _name, mimeType = "application/pdf", contentBase64 = "YQ==" })).GetProperty("isError").GetBoolean());
        }
        Assert.IsTrue((await _host.Tool("upload_document", new { fileName = "large.pdf", mimeType = "application/pdf", contentBase64 = Convert.ToBase64String(new byte[524289]) })).GetProperty("isError").GetBoolean());
        Assert.IsTrue((await _host.Tool("upload_document", new { fileName = "bad.pdf", mimeType = "application/pdf", contentBase64 = "not base64" })).GetProperty("isError").GetBoolean());
        McpTestHost.Data(await _host.Tool("delete_document", new { id = _id }));
    }

    [TestMethod]
    public async Task Pagination_is_bounded_and_does_not_repeat_items()
    {
        await using var _host = await McpTestHost.Create();
        await using (var _db = await _host.App.Services.GetRequiredService<IComptaClubDbContextFactory>().CreateDbContextAsync())
        {
            _db.Members.AddRange(Enumerable.Range(0, 115).Select(index => new MemberData { Id = Guid.NewGuid(), Name = $"Membre {index}", Email = $"member{index}@example.org", State = ComptaClub.Datas.Enums.MemberState.Active }));
            await _db.SaveChangesAsync();
        }
        var _first = McpTestHost.Data(await _host.Tool("list_members", new { pageSize = 500 })).GetProperty("list").EnumerateArray().ToList();
        var _second = McpTestHost.Data(await _host.Tool("list_members", new { pageIndex = 1, pageSize = 100 })).GetProperty("list").EnumerateArray().ToList();
        Assert.AreEqual(100, _first.Count);
        Assert.AreEqual(15, _second.Count);
        Assert.AreEqual(115, _first.Concat(_second).Select(item => item.GetProperty("id").GetGuid()).Distinct().Count());
    }
}
