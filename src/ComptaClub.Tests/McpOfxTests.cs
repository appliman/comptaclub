using System.Text;
using ComptaClub.Datas;
using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ComptaClub.Tests;

[TestClass]
public sealed class McpOfxTests
{
    private const string OFX_CONTENT = """
OFXHEADER:100
DATA:OFXSGML
VERSION:102
SECURITY:NONE
ENCODING:USASCII
CHARSET:1252
COMPRESSION:NONE
OLDFILEUID:NONE
NEWFILEUID:NONE

<OFX>
<SIGNONMSGSRSV1><SONRS><STATUS><CODE>0<SEVERITY>INFO</STATUS><DTSERVER>20261004145300<LANGUAGE>ENG</SONRS></SIGNONMSGSRSV1>
<BANKMSGSRSV1><STMTTRNRS><TRNUID>1<STATUS><CODE>0<SEVERITY>INFO</STATUS><STMTRS>
<CURDEF>EUR
<BANKACCTFROM><BANKID>12345<ACCTID>123456789<ACCTTYPE>CHECKING</BANKACCTFROM>
<BANKTRANLIST><DTSTART>20261001000000<DTEND>20261004000000
<STMTTRN><TRNTYPE>CREDIT<DTPOSTED>20261004120000<TRNAMT>12.34<FITID>MCP-OFX-001<NAME>Cotisation<MEMO>Import de test</STMTTRN>
</BANKTRANLIST>
<LEDGERBAL><BALAMT>0.00<DTASOF>20261004145300</LEDGERBAL>
</STMTRS></STMTTRNRS></BANKMSGSRSV1>
</OFX>
""";

    [TestMethod]
    public async Task Preview_does_not_save_and_selected_rows_are_deduplicated_on_save()
    {
        await using var _host = await McpTestHost.Create();
        var _file = Convert.ToBase64String(Encoding.UTF8.GetBytes(OFX_CONTENT));
        Assert.IsTrue((await _host.Tool("preview_ofx_import", new { contentBase64 = _file })).GetProperty("isError").GetBoolean());
        var _bank = McpTestHost.Data(await _host.Tool("create_bank", new { code = "OFX", label = "Banque OFX" })).GetProperty("id").GetGuid();
        McpTestHost.Data(await _host.Tool("activate_bank", new { id = _bank }));
        var _exercice = McpTestHost.Data(await _host.Tool("create_exercice", new { code = "OFX2026", label = "Exercice", startDate = "2026-01-01", endDate = "2026-12-31" })).GetProperty("id").GetGuid();
        var _account = McpTestHost.Data(await _host.Tool("create_account", new { code = "756", label = "Cotisations", direction = "Credit" })).GetProperty("id").GetGuid();
        var _preview = McpTestHost.Data(await _host.Tool("preview_ofx_import", new { contentBase64 = _file }));
        Assert.AreEqual(1, _preview.GetProperty("entries").GetArrayLength());
        await using (var _db = await _host.App.Services.GetRequiredService<IComptaClubDbContextFactory>().CreateDbContextAsync())
        {
            Assert.AreEqual(0, await _db.Entries.CountAsync());
        }
        var _selections = new[] { new { importId = "MCP-OFX-001", entry = new
        {
            partNumber = "OFX1", label = "Cotisation OFX", creationDate = "2026-10-04", valueDate = "2026-10-04",
            amountEuros = 12.34m, direction = "Credit", bankId = _bank, accountId = _account,
            exerciceId = _exercice, paymentType = "Transfer"
        } } };
        var _invalidSelection = new[] { _selections[0], _selections[0] with { importId = "ABSENT" } };
        Assert.IsTrue((await _host.Tool("save_ofx_entries", new { contentBase64 = _file, selections = _invalidSelection })).GetProperty("isError").GetBoolean());
        await using (var _db = await _host.App.Services.GetRequiredService<IComptaClubDbContextFactory>().CreateDbContextAsync())
        {
            Assert.AreEqual(0, await _db.Entries.CountAsync());
        }
        var _first = McpTestHost.Data(await _host.Tool("save_ofx_entries", new { contentBase64 = _file, selections = _selections }));
        Assert.AreEqual(1, _first.GetProperty("changeCount").GetInt32());
        var _second = McpTestHost.Data(await _host.Tool("save_ofx_entries", new { contentBase64 = _file, selections = _selections }));
        Assert.AreEqual(0, _second.GetProperty("changeCount").GetInt32());
        Assert.IsTrue(_second.GetProperty("hasWarning").GetBoolean());
        var _after = McpTestHost.Data(await _host.Tool("preview_ofx_import", new { contentBase64 = _file }));
        Assert.AreEqual(0, _after.GetProperty("entries").GetArrayLength());
        Assert.AreEqual("MCP-OFX-001", _after.GetProperty("alreadyImportedIds")[0].GetString());
        await using var _verified = await _host.App.Services.GetRequiredService<IComptaClubDbContextFactory>().CreateDbContextAsync();
        var _entry = await _verified.Entries.SingleAsync();
        Assert.AreEqual(12340000L, _entry.Amount);
        Assert.AreEqual(_host.UserId, _entry.UserCreatorId);
    }
}
