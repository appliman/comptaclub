using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ChannelMediator;
using ComptaClub.Contracts.Models.ApiKeys;
using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Models.Banks;
using ComptaClub.Contracts.Models.Clubs;
using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Models.ForecastBudget;
using ComptaClub.Contracts.Models.IncomeStatements;
using ComptaClub.Contracts.Models.Members;
using ComptaClub.Contracts.Models.Stats;
using ComptaClub.Contracts.Models.Users;
using ComptaClub.Contracts.Results;
using ComptaClub.Datas;
using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace ComptaClub.Blazor.Services.Mcp;

[McpServerToolType]
public sealed class ImportMcpTools(IMediator mediator, IComptaClubDbContextFactory dbContextFactory, ICurrentApplicationUser currentUser)
    : McpToolsBase(mediator, dbContextFactory, currentUser)
{
    [McpServerTool(Name = "preview_ofx_import", ReadOnly = true)]
    [Description("Analyse un OFX base64 de 512 Kio maximum, retourne les écritures candidates et identifiants déjà importés. Ne sauvegarde rien.")]
    public Task<CallToolResult> Preview(string contentBase64, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            var _bytes = DecodeFile(contentBase64);
            await EnsureContext(cancellationToken);
            var _imports = ComptaClub.Import.OfxParser.ParseFromContent(Encoding.UTF8.GetString(_bytes)).ToList();
            using var _stream = new MemoryStream();
            await _stream.WriteAsync(_bytes, cancellationToken);
            var _entries = (await Mediator.Send(new ImportEntryListFromStreamRequest(_stream), cancellationToken)).DistinctBy(item => item.ImportId).ToList();
            var _available = _entries.Select(item => item.ImportId).ToHashSet();
            return new { entries = _entries, alreadyImportedIds = _imports.Select(item => item.TransactionID).Where(id => !_available.Contains(id)).Distinct().ToList(),
                duplicatedIdsInFile = _imports.GroupBy(item => item.TransactionID).Where(group => group.Count() > 1).Select(group => group.Key).ToList(),
                saved = false, moneyScale = MONEY_SCALE };
        }, cancellationToken);

    [McpServerTool(Name = "save_ofx_entries", Destructive = false)]
    [Description("Réanalyse le même fichier OFX, valide et enregistre les lignes sélectionnées. Détecte à nouveau les doublons avant chaque sauvegarde. Montants en euros.")]
    public Task<CallToolResult> Save(string contentBase64, OfxSelection[] selections, CancellationToken cancellationToken = default) =>
        Run(async () =>
        {
            if (selections.Length > 100 || selections.Select(item => item.ImportId).Distinct().Count() != selections.Length)
            {
                throw new ArgumentException("Sélectionnez au maximum 100 identifiants OFX uniques.");
            }
            var _bytes = DecodeFile(contentBase64);
            await EnsureContext(cancellationToken);
            var _imports = ComptaClub.Import.OfxParser.ParseFromContent(Encoding.UTF8.GetString(_bytes))
                .DistinctBy(item => item.TransactionID).ToDictionary(item => item.TransactionID);
            var _actor = await Actor(cancellationToken);
            var _prepared = new List<(string ImportId, EntryData Entry)>();
            foreach (var _selection in selections)
            {
                if (!_imports.TryGetValue(_selection.ImportId, out var _import))
                {
                    throw new ArgumentException("Un identifiant sélectionné est absent du fichier OFX.");
                }
                var _entry = await Mediator.Send(new ComptaClub.Import.Models.CreateEntryFromOfxImportRequest(_import), cancellationToken);
                EntryMcpTools.Apply(_entry, _selection.Entry);
                _entry.UserCreatorId = _actor;
                _prepared.Add((_selection.ImportId, _entry));
            }
            var _result = new OfxBatchResult { ChangeCount = 0 };
            foreach (var _selection in _prepared)
            {
                var _saved = await Mediator.Send(new SaveOfxEntryRequest(_selection.Entry), cancellationToken);
                _result.Items.Add(_saved);
                _result.ChangeCount += _saved.ChangeCount ?? 0;
                foreach (var _rule in _saved.ErrorBrokenRuleList)
                {
                    foreach (var _message in _rule.MessageList)
                    {
                        _result.AddErrorBrokenRule(_selection.ImportId, _message);
                    }
                }
                foreach (var _rule in _saved.WarningBrokenRuleList)
                {
                    foreach (var _message in _rule.MessageList)
                    {
                        _result.AddWarningBrokenRule(_selection.ImportId, _message);
                    }
                }
            }
            return _result;
        }, cancellationToken);

    private async Task EnsureContext(CancellationToken cancellationToken)
    {
        if (await Mediator.Send(new GetActiveExerciceRequest(), cancellationToken) is null
            || await Mediator.Send(new GetActiveBankRequest(), cancellationToken) is null)
        {
            throw new ArgumentException("Configurez un exercice et une banque actifs avant l’import OFX.");
        }
    }
}
